import * as vscode from "vscode";
import * as fs from "node:fs/promises";
import * as path from "node:path";
import { spawn } from "node:child_process";

type ProjectFileDto = {
  name?: string;
  sources?: string[];
  inputProjects?: string[];
};

type BuildIssue = {
  message: string;
  file: string;
  line: number;
};

const LANGUAGE_ID = "bl";
const DIAGNOSTIC_SOURCE = "bise-sql";
const BUILD_DEBOUNCE_MS = 700;

export function activate(context: vscode.ExtensionContext): void {
  const output = getOutputChannel();
  output.appendLine("BL Language Tools activated.");

  const diagnostics = vscode.languages.createDiagnosticCollection(LANGUAGE_ID);
  context.subscriptions.push(diagnostics);
  const debouncedBuilder = createDebouncedBuildScheduler(diagnostics);
  context.subscriptions.push({ dispose: () => debouncedBuilder.dispose() });

  const buildActiveDisposable = vscode.commands.registerCommand("bl.buildActiveProject", async () => {
    const doc = vscode.window.activeTextEditor?.document;

    if (!doc || !isBlOrBlprojDocument(doc)) {
      void vscode.window.showInformationMessage("Open a .bl or .blproj file to build.");
      return;
    }

    await runBuildFromDocument(doc, diagnostics, true);
  });

  const showOrderDisposable = vscode.commands.registerCommand("bl.showBuildOrder", async () => {
    const doc = vscode.window.activeTextEditor?.document;

    if (!doc || !isBlOrBlprojDocument(doc)) {
      void vscode.window.showInformationMessage("Open a .bl or .blproj file to inspect build order.");
      return;
    }

    const projectPath = await resolveProjectForDocument(doc.uri.fsPath);

    if (!projectPath) {
      void vscode.window.showWarningMessage("No .blproj found for the active file.");
      return;
    }

    try {
      const orderedFiles = await computeBuildOrder(projectPath);
      const rel = orderedFiles.map((f) => vscode.workspace.asRelativePath(f));
      const message = [
        `Project: ${vscode.workspace.asRelativePath(projectPath)}`,
        "Build order:",
        ...rel.map((x, i) => `${i + 1}. ${x}`)
      ].join("\n");

      void vscode.window.showInformationMessage("BL build order computed. See output panel for details.");
      const output = getOutputChannel();
      output.clear();
      output.appendLine(message);
      output.show(true);
    } catch (error) {
      void vscode.window.showErrorMessage(`Failed to resolve build order: ${toMessage(error)}`);
    }
  });

  const saveDisposable = vscode.workspace.onDidSaveTextDocument(async (doc: vscode.TextDocument) => {
    if (!isBlOrBlprojDocument(doc)) {
      return;
    }

    output.appendLine(`Detected save: ${vscode.workspace.asRelativePath(doc.uri)}`);

    await debouncedBuilder.schedule(doc, false);
  });

  context.subscriptions.push(buildActiveDisposable, showOrderDisposable, saveDisposable);

  const active = vscode.window.activeTextEditor?.document;
  if (active && isBlOrBlprojDocument(active)) {
    output.appendLine(`Scheduling initial build: ${vscode.workspace.asRelativePath(active.uri)}`);
    void debouncedBuilder.schedule(active, false);
  }
}

export function deactivate(): void {
  getOutputChannel().dispose();
}

let outputChannel: vscode.OutputChannel | undefined;

function getOutputChannel(): vscode.OutputChannel {
  if (!outputChannel) {
    outputChannel = vscode.window.createOutputChannel("BL Language Tools");
  }

  return outputChannel;
}

function isBlDocument(doc: vscode.TextDocument): boolean {
  return doc.languageId === LANGUAGE_ID || doc.fileName.toLowerCase().endsWith(".bl");
}

function isBlprojDocument(doc: vscode.TextDocument): boolean {
  return doc.fileName.toLowerCase().endsWith(".blproj");
}

function isBlOrBlprojDocument(doc: vscode.TextDocument): boolean {
  return isBlDocument(doc) || isBlprojDocument(doc);
}

async function runBuildFromDocument(
  doc: vscode.TextDocument,
  diagnostics: vscode.DiagnosticCollection,
  notifyOnSuccess: boolean
): Promise<void> {
  const projectPath = await resolveProjectForDocument(doc.uri.fsPath);

  if (!projectPath) {
    diagnostics.clear();
    return;
  }

  await runBuildFromProjectPath(projectPath, diagnostics, notifyOnSuccess);
}

async function runBuildFromProjectPath(
  projectPath: string,
  diagnostics: vscode.DiagnosticCollection,
  notifyOnSuccess: boolean
): Promise<void> {

  const workspaceFolder = vscode.workspace.getWorkspaceFolder(vscode.Uri.file(projectPath));
  if (!workspaceFolder) {
    return;
  }

  const biseSqlProject = path.join(workspaceFolder.uri.fsPath, "src", "playgrounds", "bise-sql", "bise-sql.fsproj");

  try {
    await fs.access(biseSqlProject);
  } catch {
    void vscode.window.showWarningMessage("Could not find bise-sql.fsproj under src/playgrounds/bise-sql.");
    return;
  }

  const output = getOutputChannel();
  output.clear();
  output.appendLine(`Building ${vscode.workspace.asRelativePath(projectPath)}...`);

  let buildOutput: { stdout: string; stderr: string; exitCode: number };

  try {
    buildOutput = await runBiseSqlBuild(biseSqlProject, projectPath, workspaceFolder.uri.fsPath);
  } catch (error) {
    void vscode.window.showErrorMessage(`Failed to execute bise-sql build: ${toMessage(error)}`);
    return;
  }

  output.appendLine(buildOutput.stdout);
  if (buildOutput.stderr.trim().length > 0) {
    output.appendLine(buildOutput.stderr);
  }

  const issues = parseIssues(buildOutput.stdout + "\n" + buildOutput.stderr, projectPath);

  applyDiagnostics(issues, diagnostics, workspaceFolder.uri.fsPath);

  if (buildOutput.exitCode === 0) {
    if (notifyOnSuccess) {
      void vscode.window.showInformationMessage("BL build succeeded.");
    }
    return;
  }

  if (issues.length === 0) {
    void vscode.window.showErrorMessage("BL build failed. See BL Language Tools output channel.");
    output.show(true);
  }
}

function createDebouncedBuildScheduler(diagnostics: vscode.DiagnosticCollection): {
  schedule: (doc: vscode.TextDocument, notifyOnSuccess: boolean) => Promise<void>;
  dispose: () => void;
} {
  const timers = new Map<string, number>();
  const generations = new Map<string, number>();
  const timerApi = globalThis as unknown as {
    setTimeout: (handler: () => void, timeoutMs: number) => number;
    clearTimeout: (timerId: number) => void;
  };

  return {
    schedule: async (doc: vscode.TextDocument, notifyOnSuccess: boolean): Promise<void> => {
      const projectPath = await resolveProjectForDocument(doc.uri.fsPath);

      if (!projectPath) {
        diagnostics.clear();
        return;
      }

      const key = normalizePath(projectPath);
      const nextGeneration = (generations.get(key) ?? 0) + 1;
      generations.set(key, nextGeneration);

      const previousTimer = timers.get(key);
      if (previousTimer) {
        timerApi.clearTimeout(previousTimer);
      }

      const timer = timerApi.setTimeout(() => {
        timers.delete(key);

        if (generations.get(key) !== nextGeneration) {
          return;
        }

        void runBuildFromProjectPath(projectPath, diagnostics, notifyOnSuccess);
      }, BUILD_DEBOUNCE_MS);

      timers.set(key, timer);
    },
    dispose: (): void => {
      for (const timer of timers.values()) {
        timerApi.clearTimeout(timer);
      }
      timers.clear();
      generations.clear();
    }
  };
}

async function resolveProjectForDocument(filePath: string): Promise<string | undefined> {
  if (filePath.toLowerCase().endsWith(".blproj")) {
    return filePath;
  }

  const localProject = await findNearestBlproj(filePath);
  if (localProject) {
    return localProject;
  }

  return findProjectContainingSource(filePath);
}

async function findNearestBlproj(filePath: string): Promise<string | undefined> {
  let cursor = path.dirname(filePath);

  while (true) {
    try {
      const entries = await fs.readdir(cursor);
      const projectName = entries.find((entry: string) => entry.toLowerCase().endsWith(".blproj"));
      if (projectName) {
        return path.join(cursor, projectName);
      }
    } catch {
      return undefined;
    }

    const parent = path.dirname(cursor);
    if (parent === cursor) {
      break;
    }
    cursor = parent;
  }

  return undefined;
}

async function findProjectContainingSource(filePath: string): Promise<string | undefined> {
  const normalizedSource = normalizePath(filePath);
  const allProjects = await vscode.workspace.findFiles("**/*.blproj", "**/{node_modules,.git,out,bin,obj}/**");

  for (const candidate of allProjects) {
    try {
      const ordered = await computeBuildOrder(candidate.fsPath);
      const match = ordered.some((entry) => normalizePath(entry) === normalizedSource);
      if (match) {
        return candidate.fsPath;
      }
    } catch {
      // Ignore malformed projects and continue scanning.
    }
  }

  return undefined;
}

async function computeBuildOrder(projectPath: string): Promise<string[]> {
  const visited = new Set<string>();
  return collectProjectSources(projectPath, visited);
}

async function collectProjectSources(projectPath: string, visited: Set<string>): Promise<string[]> {
  const normalizedProjectPath = normalizePath(projectPath);

  if (visited.has(normalizedProjectPath)) {
    throw new Error(`Circular inputProjects reference detected at ${projectPath}`);
  }

  visited.add(normalizedProjectPath);

  const projectDir = path.dirname(projectPath);
  const project = await readProjectFile(projectPath);
  const inputProjects = project.inputProjects ?? [];
  const sources = project.sources ?? [];

  const ordered: string[] = [];

  for (const inputProject of inputProjects) {
    const nestedProject = path.resolve(projectDir, inputProject);
    const nestedSources = await collectProjectSources(nestedProject, visited);
    ordered.push(...nestedSources);
  }

  for (const source of sources) {
    ordered.push(path.resolve(projectDir, source));
  }

  visited.delete(normalizedProjectPath);
  return ordered;
}

async function readProjectFile(projectPath: string): Promise<ProjectFileDto> {
  const raw = await fs.readFile(projectPath, "utf8");
  const parsed = JSON.parse(raw) as ProjectFileDto;

  if (!Array.isArray(parsed.sources)) {
    parsed.sources = [];
  }

  if (!Array.isArray(parsed.inputProjects)) {
    parsed.inputProjects = [];
  }

  return parsed;
}

async function runBiseSqlBuild(
  biseSqlProjectPath: string,
  projectFilePath: string,
  cwd: string
): Promise<{ stdout: string; stderr: string; exitCode: number }> {
  return new Promise((resolve, reject) => {
    const args = [
      "run",
      "--project",
      biseSqlProjectPath,
      "--",
      "--file",
      projectFilePath
    ];

    const env = (
      (globalThis as unknown as { process?: { env?: Record<string, string | undefined> } }).process?.env
    ) ?? undefined;
    const child = spawn("dotnet", args, { cwd, env });

    let stdout = "";
    let stderr = "";

    child.stdout.on("data", (chunk: Uint8Array | string) => {
      stdout += chunk.toString();
    });

    child.stderr.on("data", (chunk: Uint8Array | string) => {
      stderr += chunk.toString();
    });

    child.on("error", (err: unknown) => {
      reject(err);
    });

    child.on("close", (code: number | null) => {
      resolve({
        stdout,
        stderr,
        exitCode: code ?? 1
      });
    });
  });
}

function parseIssues(output: string, projectPath: string): BuildIssue[] {
  const issues: BuildIssue[] = [];
  const seen = new Set<string>();

  const regex = /Error:\s*(.+?)\s*(?:\r?\n)\.\.\.while (?:parsing|typechecking)\s+(.+?)\s+at line\s+(\d+):/g;

  let match: RegExpExecArray | null;
  while ((match = regex.exec(output)) !== null) {
    const message = match[1].trim();
    const fileFromOutput = match[2].trim();
    const line = Number.parseInt(match[3], 10);

    const absoluteFile = toAbsoluteFile(projectPath, fileFromOutput);
    const key = `${absoluteFile}:${line}:${message}`;

    if (!seen.has(key)) {
      seen.add(key);
      issues.push({ message, file: absoluteFile, line });
    }
  }

  return issues;
}

function toAbsoluteFile(projectPath: string, fileFromOutput: string): string {
  if (path.isAbsolute(fileFromOutput)) {
    return fileFromOutput;
  }

  return path.resolve(path.dirname(projectPath), fileFromOutput);
}

function applyDiagnostics(issues: BuildIssue[], diagnostics: vscode.DiagnosticCollection, workspaceRoot: string): void {
  diagnostics.clear();

  const grouped = new Map<string, vscode.Diagnostic[]>();

  for (const issue of issues) {
    const filePath = normalizePath(issue.file);

    if (!filePath.startsWith(normalizePath(workspaceRoot))) {
      continue;
    }

    const line = Math.max(0, issue.line - 1);
    const range = new vscode.Range(line, 0, line, 200);

    const diagnostic = new vscode.Diagnostic(range, issue.message, vscode.DiagnosticSeverity.Error);
    diagnostic.source = DIAGNOSTIC_SOURCE;

    const existing = grouped.get(filePath) ?? [];
    existing.push(diagnostic);
    grouped.set(filePath, existing);
  }

  for (const [filePath, diags] of grouped.entries()) {
    diagnostics.set(vscode.Uri.file(filePath), diags);
  }
}

function normalizePath(p: string): string {
  return path.normalize(p).toLowerCase();
}

function toMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return String(error);
}
