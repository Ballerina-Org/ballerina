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

type BuildInlayHintDto = {
  type?: string;
  file?: string;
  line?: number;
  column?: number;
};

type BuildResultDto = {
  success?: boolean;
  errors?: BuildErrorDto[];
  inlayHints?: BuildInlayHintDto[];
};

type IdentifierHintDto = {
  name?: string;
  type?: string;
};

type DotAccessHintDto = {
  file?: string;
  line?: number;
  column?: number;
  objectType?: string;
  availableFields?: Record<string, string>;
};

type ScopeAccessHintDto = {
  file?: string;
  line?: number;
  column?: number;
  prefix?: string;
  availableSymbols?: Record<string, string>;
};

type FileBuiltEventDto = {
  eventType?: string;
  file?: string;
  success?: boolean;
  errors?: BuildErrorDto[];
  inlayHints?: BuildInlayHintDto[];
  identifierHints?: IdentifierHintDto[];
  dotAccessHints?: DotAccessHintDto[];
  scopeAccessHints?: ScopeAccessHintDto[];
};

type ProjectCompleteEventDto = {
  eventType?: string;
  project?: string;
  totalFiles?: number;
  totalErrors?: number;
  inlayHints?: BuildInlayHintDto[];
};

type StreamingBuildResult = {
  projectComplete?: ProjectCompleteEventDto;
  errors?: BuildErrorDto[];
  fileEvents: FileBuiltEventDto[];
  success: boolean;
};

const LANGUAGE_ID = "bl";
const DIAGNOSTIC_SOURCE = "ballerina-language-tools";
const BUILD_DEBOUNCE_MS = 50;
let buildRunSequence = 0;
let buildServerClient: BuildServerClient | undefined;
let buildServerClientKey: string | undefined;
let completionHintStore: CompletionHintStore | undefined;

export function activate(context: vscode.ExtensionContext): void {
  const output = getOutputChannel();
  output.appendLine("BL Language Tools activated.");

  const diagnostics = vscode.languages.createDiagnosticCollection(LANGUAGE_ID);
  const inlayHints = new InlayHintStore();
  completionHintStore = new CompletionHintStore();
  context.subscriptions.push(diagnostics);
  context.subscriptions.push(inlayHints);
  context.subscriptions.push(completionHintStore);
  const inlayHintsDisposable = vscode.languages.registerInlayHintsProvider(
    { language: LANGUAGE_ID, scheme: "file" },
    new BlInlayHintsProvider(inlayHints)
  );
  context.subscriptions.push(inlayHintsDisposable);

  const completionDisposable = vscode.languages.registerCompletionItemProvider(
    { language: LANGUAGE_ID, scheme: "file" },
    new BlCompletionProvider(completionHintStore),
    ".", ":"
  );
  context.subscriptions.push(completionDisposable);

  const debouncedBuilder = createDebouncedBuildScheduler(diagnostics, inlayHints);
  context.subscriptions.push({ dispose: () => debouncedBuilder.dispose() });

  const buildActiveDisposable = vscode.commands.registerCommand("bl.buildActiveProject", async () => {
    const doc = vscode.window.activeTextEditor?.document;

    if (!doc || !isBlOrBlprojDocument(doc)) {
      void vscode.window.showInformationMessage("Open a .bl or .blproj file to build.");
      return;
    }

    await runBuildFromDocument(doc, diagnostics, inlayHints, true);
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
  completionHintStore?.dispose();
  completionHintStore = undefined;
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
  inlayHints: InlayHintStore,
  notifyOnSuccess: boolean
): Promise<void> {
  const projectPath = await resolveProjectForDocument(doc.uri.fsPath);

  if (!projectPath) {
    diagnostics.clear();
    inlayHints.clearFile(doc.uri.fsPath);
    return;
  }

  await runBuildFromProjectPath(projectPath, diagnostics, inlayHints, notifyOnSuccess);
}

async function runBuildFromProjectPath(
  projectPath: string,
  diagnostics: vscode.DiagnosticCollection,
  inlayHints: InlayHintStore,
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

  inlayHints.replaceProjectHints(projectPath, buildOutput.serverResult);

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

function createDebouncedBuildScheduler(
  diagnostics: vscode.DiagnosticCollection,
  inlayHints: InlayHintStore
): {
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
        inlayHints.clearFile(doc.uri.fsPath);
        return;
      }

      const key = normalizePathKey(projectPath);
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
          await runBuildFromProjectPath(projectPath, diagnostics, inlayHints, notifyOnSuccess);
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
  const normalizedProjectPath = normalizePathKey(projectPath);
  let sourceFiles = new Set<string>();

  try {
    const orderedSources = await computeBuildOrder(projectPath);
    sourceFiles = new Set(orderedSources.map((entry) => normalizePathKey(entry)));
  } catch {
    // If the project is malformed while typing, still try to save the project file itself.
  }

  for (const doc of vscode.workspace.textDocuments) {
    if (!isBlOrBlprojDocument(doc) || !doc.isDirty) {
      continue;
    }

    const normalizedDocPath = normalizePathKey(doc.uri.fsPath);
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
  const normalizedSource = normalizePathKey(filePath);
  const allProjects = await vscode.workspace.findFiles("**/*.blproj", "**/{node_modules,.git,out,bin,obj}/**");

  for (const candidate of allProjects) {
    try {
      const ordered = await computeBuildOrder(candidate.fsPath);
      const match = ordered.some((entry) => normalizePathKey(entry) === normalizedSource);
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
  const normalizedProjectPath = normalizePathKey(projectPath);

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
    const streamingResult = await serverClient.build(projectFilePath, (event) => {
      const fileName = event.file ?? "";
      completionHintStore?.updateForFile(
        projectFilePath,
        fileName,
        event.dotAccessHints ?? [],
        event.scopeAccessHints ?? [],
        event.identifierHints ?? []
      );
    });

    const serverResult = streamingResultToBuildResult(streamingResult);
    const errors = Array.isArray(serverResult.errors) ? serverResult.errors.length : 0;
    const summary = serverResult.success
      ? `[Run #${runId}] Server response: success (streaming).`
      : `[Run #${runId}] Server response: failed with ${errors} error(s) (streaming).`;

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

function streamingResultToBuildResult(result: StreamingBuildResult): BuildResultDto {
  if (result.success && result.projectComplete) {
    return {
      success: true,
      errors: [],
      inlayHints: result.projectComplete.inlayHints ?? []
    };
  }

  const errors: BuildErrorDto[] = [];

  for (const event of result.fileEvents) {
    if (Array.isArray(event.errors)) {
      errors.push(...event.errors);
    }
  }

  if (Array.isArray(result.errors)) {
    errors.push(...result.errors);
  }

  return {
    success: false,
    errors,
    inlayHints: result.projectComplete?.inlayHints ?? []
  };
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
  const workspaceRootKey = normalizePathKey(workspaceRoot);

  for (const issue of issues) {
    const filePath = normalizePath(issue.file);
    const filePathKey = normalizePathKey(filePath);

    if (!filePathKey.startsWith(workspaceRootKey)) {
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
  return path.normalize(p);
}

function normalizePathKey(p: string): string {
  return normalizePath(p).toLowerCase();
}

class InlayHintStore implements vscode.Disposable {
  private readonly hintsByFile = new Map<string, vscode.InlayHint[]>();
  private readonly projectFiles = new Map<string, Set<string>>();
  private readonly onDidChangeEmitter = new vscode.EventEmitter<void>();

  public readonly onDidChangeInlayHints: vscode.Event<void> = this.onDidChangeEmitter.event;

  replaceProjectHints(projectPath: string, serverResult: BuildResultDto | undefined): void {
    const projectKey = normalizePathKey(projectPath);

    for (const fileKey of this.projectFiles.get(projectKey) ?? []) {
      this.hintsByFile.delete(fileKey);
    }

    const nextFiles = new Set<string>();
    const dtoHints = Array.isArray(serverResult?.inlayHints) ? serverResult.inlayHints : [];

    for (const dto of dtoHints) {
      const file = toAbsoluteFile(projectPath, dto.file ?? "");
      const line = Number.isFinite(dto.line) ? Number(dto.line) : 1;
      const column = Number.isFinite(dto.column) ? Number(dto.column) : 1;
      const hintType = typeof dto.type === "string" ? dto.type.trim() : "";

      if (hintType.length === 0) {
        continue;
      }

      const label = `: ${hintType}`;
      const position = new vscode.Position(Math.max(0, line - 1), Math.max(0, column - 1));
      const hint = new vscode.InlayHint(position, label, vscode.InlayHintKind.Type);

      hint.paddingLeft = true;

      const fileKey = normalizePathKey(file);
      const existing = this.hintsByFile.get(fileKey) ?? [];
      existing.push(hint);
      this.hintsByFile.set(fileKey, existing);
      nextFiles.add(fileKey);
    }

    this.projectFiles.set(projectKey, nextFiles);
    this.onDidChangeEmitter.fire();
  }

  clearFile(filePath: string): void {
    const fileKey = normalizePathKey(filePath);

    if (!this.hintsByFile.delete(fileKey)) {
      return;
    }

    for (const fileSet of this.projectFiles.values()) {
      fileSet.delete(fileKey);
    }

    this.onDidChangeEmitter.fire();
  }

  getHintsForFile(filePath: string, range: vscode.Range): vscode.InlayHint[] {
    const fileKey = normalizePathKey(filePath);
    const hints = this.hintsByFile.get(fileKey) ?? [];

    if (hints.length === 0) {
      return [];
    }

    return hints.filter((hint) => range.contains(hint.position));
  }

  dispose(): void {
    this.hintsByFile.clear();
    this.projectFiles.clear();
    this.onDidChangeEmitter.dispose();
  }
}

class BlInlayHintsProvider implements vscode.InlayHintsProvider {
  constructor(private readonly inlayHints: InlayHintStore) {}

  readonly onDidChangeInlayHints: vscode.Event<void> = this.inlayHints.onDidChangeInlayHints;

  provideInlayHints(document: vscode.TextDocument, range: vscode.Range): vscode.InlayHint[] {
    if (!isBlDocument(document)) {
      return [];
    }

    return this.inlayHints.getHintsForFile(document.uri.fsPath, range);
  }
}

class CompletionHintStore implements vscode.Disposable {
  private dotHintsByFile = new Map<string, DotAccessHintDto[]>();
  private scopeHintsByFile = new Map<string, ScopeAccessHintDto[]>();
  private identifierHints: IdentifierHintDto[] = [];
  private readonly onDidChangeEmitter = new vscode.EventEmitter<void>();

  public readonly onDidChange: vscode.Event<void> = this.onDidChangeEmitter.event;

  updateForFile(
    projectPath: string,
    fileName: string,
    dotHints: DotAccessHintDto[],
    scopeHints: ScopeAccessHintDto[],
    identHints: IdentifierHintDto[]
  ): void {
    const absFile = toAbsoluteFile(projectPath, fileName);
    const key = normalizePathKey(absFile);
    this.dotHintsByFile.set(key, dotHints);
    this.scopeHintsByFile.set(key, scopeHints);

    if (identHints.length > 0) {
      this.identifierHints = identHints;
    }

    this.onDidChangeEmitter.fire();
  }

  getDotHints(filePath: string): DotAccessHintDto[] {
    return this.dotHintsByFile.get(normalizePathKey(filePath)) ?? [];
  }

  getScopeHints(filePath: string): ScopeAccessHintDto[] {
    return this.scopeHintsByFile.get(normalizePathKey(filePath)) ?? [];
  }

  getIdentifierHints(): IdentifierHintDto[] {
    return this.identifierHints;
  }

  clearProject(): void {
    this.dotHintsByFile.clear();
    this.scopeHintsByFile.clear();
    this.onDidChangeEmitter.fire();
  }

  dispose(): void {
    this.dotHintsByFile.clear();
    this.scopeHintsByFile.clear();
    this.identifierHints = [];
    this.onDidChangeEmitter.dispose();
  }
}

class BlCompletionProvider implements vscode.CompletionItemProvider {
  constructor(private readonly store: CompletionHintStore) {}

  provideCompletionItems(
    document: vscode.TextDocument,
    position: vscode.Position,
    _token: vscode.CancellationToken,
    _context: vscode.CompletionContext
  ): vscode.CompletionItem[] {
    if (!isBlDocument(document)) {
      return [];
    }

    const lineText = document.lineAt(position.line).text;
    const textBefore = lineText.substring(0, position.character);

    const scopeMatch = textBefore.match(/(\w+)::$/);

    if (scopeMatch) {
      return this.provideScopeCompletions(document, position, scopeMatch[1]);
    }

    if (textBefore.endsWith(".")) {
      return this.provideDotCompletions(document, position);
    }

    return [];
  }

  private provideDotCompletions(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.CompletionItem[] {
    const dotHints = this.store.getDotHints(document.uri.fsPath);
    const cursorLine1 = position.line + 1;
    const cursorCol1 = position.character;

    const hint = dotHints.find((h) => {
      const hLine = h.line ?? 0;
      const hCol = h.column ?? 0;
      return hLine === cursorLine1 && Math.abs(hCol - cursorCol1) <= 2;
    });

    if (!hint?.availableFields) {
      return [];
    }

    return Object.entries(hint.availableFields).map(([name, type]) => {
      const item = new vscode.CompletionItem(name, vscode.CompletionItemKind.Field);
      item.detail = type;
      return item;
    });
  }

  private provideScopeCompletions(
    document: vscode.TextDocument,
    position: vscode.Position,
    prefix: string
  ): vscode.CompletionItem[] {
    const scopeHints = this.store.getScopeHints(document.uri.fsPath);
    const cursorLine1 = position.line + 1;

    const hint = scopeHints.find((h) => {
      const hLine = h.line ?? 0;
      return hLine === cursorLine1 && h.prefix === prefix;
    });

    if (hint?.availableSymbols) {
      return Object.entries(hint.availableSymbols).map(([name, type]) => {
        const item = new vscode.CompletionItem(name, vscode.CompletionItemKind.Function);
        item.detail = type;
        return item;
      });
    }

    const identHints = this.store.getIdentifierHints();
    const scopePrefix = prefix + "::";
    const items: vscode.CompletionItem[] = [];

    for (const h of identHints) {
      if (h.name && h.name.startsWith(scopePrefix)) {
        const name = h.name.substring(scopePrefix.length);
        const item = new vscode.CompletionItem(name, vscode.CompletionItemKind.Function);
        item.detail = h.type ?? "";
        items.push(item);
      }
    }

    return items;
  }
}

function getBuildServerClient(
  cwd: string,
  output: vscode.OutputChannel
): BuildServerClient {
  const nextKey = `${normalizePathKey(cwd)}`;

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

  build(
    projectFilePath: string,
    onFileBuilt?: (event: FileBuiltEventDto) => void
  ): Promise<StreamingBuildResult> {
    const work = async (): Promise<StreamingBuildResult> => {
      await this.ensureStarted();
      return this.sendRequest(projectFilePath, onFileBuilt);
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

    const args = ["server-streaming"];
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

  private async sendRequest(
    projectFilePath: string,
    onFileBuilt?: (event: FileBuiltEventDto) => void
  ): Promise<StreamingBuildResult> {
    const child = this.child;
    const stdoutReader = this.stdoutReader;

    if (!child || !stdoutReader || child.killed) {
      throw new Error("Build server is not running.");
    }

    return new Promise<StreamingBuildResult>((resolve, reject) => {
      const fileEvents: FileBuiltEventDto[] = [];

      const onLine = (line: string): void => {
        const trimmed = line.trim();

        if (trimmed.length === 0) {
          return;
        }

        let parsed: Record<string, unknown>;

        try {
          parsed = JSON.parse(trimmed) as Record<string, unknown>;
        } catch {
          return;
        }

        const eventType = parsed.eventType as string | undefined;

        if (eventType === "file-built") {
          const event = parsed as unknown as FileBuiltEventDto;
          fileEvents.push(event);

          try {
            onFileBuilt?.(event);
          } catch {
            // Ignore callback errors.
          }
        } else if (eventType === "project-complete") {
          cleanup();
          const event = parsed as unknown as ProjectCompleteEventDto;
          resolve({
            projectComplete: event,
            fileEvents,
            success: true
          });
        } else {
          cleanup();
          const result = parsed as unknown as BuildResultDto;
          resolve({
            errors: result.errors,
            fileEvents,
            success: result.success ?? false
          });
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
