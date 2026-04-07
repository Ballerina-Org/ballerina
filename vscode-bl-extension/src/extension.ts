import * as vscode from "vscode";
import * as fs from "fs/promises";
import * as path from "path";
import { spawn, type ChildProcessWithoutNullStreams } from "child_process";
import { createInterface, type Interface } from "readline";

type ProjectFileDto = {
  name?: string;
  sources?: string[];
  inputProjects?: string[];
};

type BuildIssue = {
  message: string;
  file: string;
  line: number;
  column?: number;
};

type BuildErrorDto = {
  message?: string;
  file?: string;
  line?: number;
  column?: number;
};

type BuildResultDto = {
  success?: boolean;
  errors?: BuildErrorDto[];
};

const LANGUAGE_ID = "bl";
const DIAGNOSTIC_SOURCE = "ballerina-language-tools";
const BUILD_DEBOUNCE_MS = 50;
let buildRunSequence = 0;
let buildServerClient: BuildServerClient | undefined;
let buildServerClientKey: string | undefined;

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

    if (debouncedBuilder.isSuppressedSave(doc)) {
      return;
    }

    output.appendLine(`Detected save: ${vscode.workspace.asRelativePath(doc.uri)}`);

    await debouncedBuilder.schedule(doc, false);
  });

  const changeDisposable = vscode.workspace.onDidChangeTextDocument(async (event: vscode.TextDocumentChangeEvent) => {
    if (!isBlOrBlprojDocument(event.document)) {
      return;
    }

    if (event.contentChanges.length === 0) {
      return;
    }

    await debouncedBuilder.schedule(event.document, false);
  });

  context.subscriptions.push(buildActiveDisposable, showOrderDisposable, saveDisposable, changeDisposable);

  const active = vscode.window.activeTextEditor?.document;
  if (active && isBlOrBlprojDocument(active)) {
    output.appendLine(`Scheduling initial build: ${vscode.workspace.asRelativePath(active.uri)}`);
    void debouncedBuilder.schedule(active, false);
  }
}

export function deactivate(): void {
  disposeBuildServerClient();
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
  const output = getOutputChannel();
  output.clear();
  const runId = ++buildRunSequence;
  output.appendLine(`[Run #${runId}] Building ${vscode.workspace.asRelativePath(projectPath)}...`);

  output.appendLine(`[Run #${runId}] Working directory: ${workspaceFolder.uri.fsPath}`);
  const startedAt = new Date();
  const startedAtMs = Date.now();
  output.appendLine(`[Run #${runId}] Started at: ${startedAt.toISOString()}`);
  output.appendLine(`[Run #${runId}] Typechecking started (server mode).`);
  const typecheckStartedAtMs = Date.now();

  let buildOutput: { stdout: string; stderr: string; exitCode: number; serverResult?: BuildResultDto };

  try {
    buildOutput = await runBallerinaSqlBuild(
      projectPath,
      workspaceFolder.uri.fsPath,
      output,
      runId
    );
  } catch (error) {
    const elapsedMs = Date.now() - startedAtMs;
    output.appendLine(`[Run #${runId}] Failed after: ${formatDurationForLog(elapsedMs)}`);
    void vscode.window.showErrorMessage(`Failed to execute ballerina build: ${toMessage(error)}`);
    return;
  }

  const elapsedMs = Date.now() - startedAtMs;
  const typecheckElapsedMs = Date.now() - typecheckStartedAtMs;
  output.appendLine(
    `[Run #${runId}] Finished in: ${formatDurationForLog(elapsedMs)} (exit code ${buildOutput.exitCode})`
  );
  output.appendLine(`[Run #${runId}] Typechecking completed in: ${formatDurationForLog(typecheckElapsedMs)}`);

  output.appendLine(buildOutput.stdout);
  if (buildOutput.stderr.trim().length > 0) {
    output.appendLine(buildOutput.stderr);
  }

  const issues = buildOutput.serverResult
    ? parseIssuesFromServerResult(buildOutput.serverResult, projectPath)
    : parseIssues(buildOutput.stdout + "\n" + buildOutput.stderr, projectPath);

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
  isSuppressedSave: (doc: vscode.TextDocument) => boolean;
  dispose: () => void;
} {
  const output = getOutputChannel();
  const timers = new Map<string, number>();
  const generations = new Map<string, number>();
  const scheduledAtMs = new Map<string, number>();
  const suppressedSaveUris = new Set<string>();
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

      scheduledAtMs.set(key, Date.now());

      const timer = timerApi.setTimeout(() => {
        timers.delete(key);

        if (generations.get(key) !== nextGeneration) {
          return;
        }

        const scheduledAt = scheduledAtMs.get(key);
        scheduledAtMs.delete(key);
        const debounceDelayMs = scheduledAt ? Date.now() - scheduledAt : BUILD_DEBOUNCE_MS;
        output.appendLine(
          `[Debounce] Triggering build for ${vscode.workspace.asRelativePath(projectPath)} after ${formatDurationForLog(debounceDelayMs)}`
        );

        void (async () => {
          await saveDirtyDocumentsForProject(projectPath, suppressedSaveUris);
          await runBuildFromProjectPath(projectPath, diagnostics, notifyOnSuccess);
        })();
      }, BUILD_DEBOUNCE_MS);

      timers.set(key, timer);
    },
    isSuppressedSave: (doc: vscode.TextDocument): boolean => {
      const uri = doc.uri.toString();
      if (!suppressedSaveUris.has(uri)) {
        return false;
      }

      suppressedSaveUris.delete(uri);
      return true;
    },
    dispose: (): void => {
      for (const timer of timers.values()) {
        timerApi.clearTimeout(timer);
      }
      timers.clear();
      generations.clear();
      scheduledAtMs.clear();
      suppressedSaveUris.clear();
    }
  };
}

async function saveDirtyDocumentsForProject(projectPath: string, suppressedSaveUris: Set<string>): Promise<void> {
  const normalizedProjectPath = normalizePath(projectPath);
  let sourceFiles = new Set<string>();

  try {
    const orderedSources = await computeBuildOrder(projectPath);
    sourceFiles = new Set(orderedSources.map((entry) => normalizePath(entry)));
  } catch {
    // If the project is malformed while typing, still try to save the project file itself.
  }

  for (const doc of vscode.workspace.textDocuments) {
    if (!isBlOrBlprojDocument(doc) || !doc.isDirty) {
      continue;
    }

    const normalizedDocPath = normalizePath(doc.uri.fsPath);
    const shouldSave = isBlprojDocument(doc)
      ? normalizedDocPath === normalizedProjectPath
      : sourceFiles.has(normalizedDocPath);

    if (!shouldSave) {
      continue;
    }

    const uri = doc.uri.toString();
    suppressedSaveUris.add(uri);
    const saved = await doc.save();
    if (!saved) {
      suppressedSaveUris.delete(uri);
    }
  }
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

async function runBallerinaSqlBuild(
  projectFilePath: string,
  cwd: string,
  output: vscode.OutputChannel,
  runId: number
): Promise<{ stdout: string; stderr: string; exitCode: number; serverResult?: BuildResultDto }> {
  const serverClient = getBuildServerClient(cwd, output);

  try {
    const serverResult = await serverClient.build(projectFilePath);
    const errors = Array.isArray(serverResult.errors) ? serverResult.errors.length : 0;
    const summary = serverResult.success
      ? `[Run #${runId}] Server response: success.`
      : `[Run #${runId}] Server response: failed with ${errors} error(s).`;

    return {
      stdout: summary,
      stderr: "",
      exitCode: serverResult.success ? 0 : 1,
      serverResult
    };
  } catch (serverError) {
    output.appendLine(`[Run #${runId}] Server mode failed; falling back to one-shot build: ${toMessage(serverError)}`);
    throw serverError;
  }
}

function parseIssuesFromServerResult(serverResult: BuildResultDto, projectPath: string): BuildIssue[] {
  const errors = Array.isArray(serverResult.errors) ? serverResult.errors : [];
  const issues: BuildIssue[] = [];
  const seen = new Set<string>();

  for (const error of errors) {
    const message = (error.message ?? "Build failed").trim();
    const file = toAbsoluteFile(projectPath, error.file ?? "");
    const line = Number.isFinite(error.line) ? Number(error.line) : 1;
    const column = Number.isFinite(error.column) ? Number(error.column) : 1;
    const key = `${file}:${line}:${column}:${message}`;

    if (seen.has(key)) {
      continue;
    }

    seen.add(key);
    issues.push({
      message,
      file,
      line,
      column
    });
  }

  return issues;
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
      issues.push({ message, file: absoluteFile, line, column: 1 });
    }
  }

  return issues;
}

function toAbsoluteFile(projectPath: string, fileFromOutput: string): string {
  if (fileFromOutput.length === 0) {
    return projectPath;
  }

  if (path.isAbsolute(fileFromOutput)) {
    return fileFromOutput;
  }

  return path.resolve(path.dirname(projectPath), fileFromOutput);
}

function formatCommandForLog(command: string, args: string[]): string {
  const quote = (value: string): string => {
    if (value.length === 0) {
      return '""';
    }

    if (!/[\s"'\\]/.test(value)) {
      return value;
    }

    return `"${value.replace(/(["\\$`])/g, "\\$1")}"`;
  };

  return [quote(command), ...args.map(quote)].join(" ");
}

function formatDurationForLog(durationMs: number): string {
  if (durationMs < 1000) {
    return `${durationMs}ms`;
  }

  return `${(durationMs / 1000).toFixed(2)}s`;
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
    const column = Math.max(0, (issue.column ?? 1) - 1);
    const endColumn = Math.max(column + 1, 200);
    const range = new vscode.Range(line, column, line, endColumn);

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

function getBuildServerClient(
  cwd: string,
  output: vscode.OutputChannel
): BuildServerClient {
  const nextKey = `${normalizePath(cwd)}`;

  if (!buildServerClient || buildServerClientKey !== nextKey) {
    disposeBuildServerClient();
    buildServerClient = new BuildServerClient(cwd, output);
    buildServerClientKey = nextKey;
  }

  return buildServerClient;
}

function disposeBuildServerClient(): void {
  if (!buildServerClient) {
    return;
  }

  buildServerClient.dispose();
  buildServerClient = undefined;
  buildServerClientKey = undefined;
}

class BuildServerClient {
  private child: ChildProcessWithoutNullStreams | undefined;
  private stdoutReader: Interface | undefined;
  private readonly stderrLines: string[] = [];
  private requestChain: Promise<void> = Promise.resolve();

  constructor(
    private readonly cwd: string,
    private readonly output: vscode.OutputChannel
  ) {}

  build(projectFilePath: string): Promise<BuildResultDto> {
    const work = async (): Promise<BuildResultDto> => {
      await this.ensureStarted();
      return this.sendRequest(projectFilePath);
    };

    const promise = this.requestChain.then(work, work);
    this.requestChain = promise.then(() => undefined, () => undefined);
    return promise;
  }

  dispose(): void {
    this.stdoutReader?.close();
    this.stdoutReader = undefined;

    if (this.child && !this.child.killed) {
      this.child.kill();
    }

    this.child = undefined;
    this.stderrLines.length = 0;
  }

  private async ensureStarted(): Promise<void> {
    if (this.child && !this.child.killed) {
      return;
    }

    const args = ["server"];
    const env = (
      (globalThis as unknown as { process?: { env?: Record<string, string | undefined> } }).process?.env
    ) ?? undefined;
    const child = spawn("ballerina", args, { cwd: this.cwd, env, stdio: "pipe" });

    this.output.appendLine(`[Server] Starting: ${formatCommandForLog("ballerina", args)}`);

    const stdoutReader = createInterface({ input: child.stdout });
    this.stdoutReader = stdoutReader;
    this.child = child;
    this.stderrLines.length = 0;

    child.stderr.on("data", (chunk: Uint8Array | string) => {
      const text = chunk.toString();
      this.stderrLines.push(text);
      if (this.stderrLines.length > 40) {
        this.stderrLines.shift();
      }
    });

    child.on("error", (err: unknown) => {
      this.output.appendLine(`[Server] Process error: ${toMessage(err)}`);
      this.child = undefined;
      this.stdoutReader?.close();
      this.stdoutReader = undefined;
    });

    child.on("close", (code: number | null) => {
      this.output.appendLine(`[Server] Exited with code ${code ?? -1}.`);
      this.child = undefined;
      this.stdoutReader?.close();
      this.stdoutReader = undefined;
    });
  }

  private async sendRequest(projectFilePath: string): Promise<BuildResultDto> {
    const child = this.child;
    const stdoutReader = this.stdoutReader;

    if (!child || !stdoutReader || child.killed) {
      throw new Error("Build server is not running.");
    }

    return new Promise<BuildResultDto>((resolve, reject) => {
      const onLine = (line: string): void => {
        const trimmed = line.trim();

        if (trimmed.length === 0) {
          return;
        }

        cleanup();

        try {
          const parsed = JSON.parse(trimmed) as BuildResultDto;
          resolve(parsed);
        } catch {
          reject(new Error(`Unexpected server response: ${trimmed}`));
        }
      };

      const onClose = (): void => {
        cleanup();
        const stderrTail = this.stderrLines.join("").trim();
        reject(new Error(stderrTail.length > 0 ? stderrTail : "Build server exited before responding."));
      };

      const onError = (err: unknown): void => {
        cleanup();
        reject(err instanceof Error ? err : new Error(String(err)));
      };

      const cleanup = (): void => {
        stdoutReader.off("line", onLine);
        child.off("close", onClose);
        child.off("error", onError);
      };

      stdoutReader.on("line", onLine);
      child.once("close", onClose);
      child.once("error", onError);

      child.stdin.write(`${projectFilePath}\n`, (err?: Error | null) => {
        if (err) {
          cleanup();
          reject(err);
        }
      });
    });
  }
}

function toMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return String(error);
}
