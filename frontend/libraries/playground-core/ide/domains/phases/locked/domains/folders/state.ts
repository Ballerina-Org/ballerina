import {
    BasicUpdater,
    ForeignMutationsInput, Maybe, Option,
    replaceWith,
    SimpleCallback, Template,
    Updater, 
    ValueOrErrors,
    View
} from "ballerina-core";
import {FlatNode, Node} from "./node";
import {JsonEditorTemplate} from "./editor/template";
import {LockedPhase} from "../../state";
import {JsonEditor, JsonEditorForeignMutationsExpected, JsonEditorView, JsonEditorWritableState} from "./editor/state";
import {CustomFields} from "../../../custom-fields/state";
import {customFields} from "../../../../../coroutines/custom-fields";

export type WorkspaceVariant =
    | { kind: 'compose' }
    | { kind: 'explore' }
    | { kind: 'scratch' }

export type WorkspaceContext = {}

export type WorkspaceState =
    (
    | { kind: 'view' }
    | { kind: 'selected', file: Node }
    ) & { nodes: Node, variant: WorkspaceVariant };

export type SelectedWorkspaceState = Extract<WorkspaceState, { kind: "selected" }>;

export const WorkspaceState = {
    Default: (variant: WorkspaceVariant, nodes: Node): WorkspaceState => ({
        kind: 'view',
        nodes: nodes,
        variant: variant,
    }),
    Updater: {
        maybeSelected: (u:BasicUpdater<WorkspaceState>): Updater<Maybe<LockedPhase>> =>
            Updater(ide => {
                    if (!ide || ide.workspace.kind != "selected") return ide;
                    return ({
                        ...ide,
                        workspace: u(ide.workspace)
                            
                    } as LockedPhase);
                }),
        reloadContent: (next: Node): Updater<WorkspaceState> =>
            Updater(workspace => {
                return ({
                    ...workspace,
                    nodes: {...workspace.nodes, children: next.children },
                })
            }),
        changeFileContent: (newContent: any): Updater<WorkspaceState> =>
            Updater(workspace => {
                if(workspace.kind != 'selected') return workspace;

                return ({
                    ...workspace,
                    file: { ...workspace.file, metadata: { ...workspace.file.metadata, content: newContent }}} as SelectedWorkspaceState);
            }),
        selectFile: (file: Node): Updater<WorkspaceState> =>
            Updater(workspace => 
                ({
                    nodes: workspace.nodes,
                    variant: workspace.variant,
                    kind: 'selected',
                    file: file  } as WorkspaceState)
            ),
        defaultForSingleFolder: (): Updater<WorkspaceState> =>
            Updater(workspace =>
                {

                    if(!FlatNode.Operations.hasSingleFolderBelowRoot(workspace.nodes)) {
                        return workspace;
                    }
                    
                    const files = FlatNode.Operations.getFilesForSingleFolderBelowRoot(workspace.nodes);
                    if(files.length  == 0) return workspace;
                    return WorkspaceState.Updater.selectFile(files[0])(workspace);
                }
            ),
    },
    ForeignMutations: (
        _: ForeignMutationsInput<WorkspaceContext, WorkspaceState>,
    ) => ({
    }),
}

export const VirtualFolders = {
    Operations: {
        formatBytes: (bytes?: number) => {
            if (bytes == null || !Number.isFinite(bytes)) return "";
            const k = 1024;
            const sizes = ["B", "KB", "MB", "GB", "TB"];
            const i = Math.max(0, Math.floor(Math.log(bytes) / Math.log(k)));
            const n = bytes / Math.pow(k, i);
            return `${n.toFixed(n >= 10 || i === 0 ? 0 : 1)} ${sizes[i]}`;
        },
        getNestedFileInNamedFolder(path: string[]): string[] {
            if (path.length === 0) return [];

            const fileName = path[path.length - 1];
            const withoutExt = fileName.replace(/\.[^/.]+$/, "");

            return [...path.slice(0, -1), withoutExt, fileName];
        },
        markLeaves: (n: Node): Node => {
            if (n.metadata.kind === "file") {
                return { ...n, metadata: { ...n.metadata, isLeaf: true } };
            }

            const children = (n.children ?? []).map(VirtualFolders.Operations.markLeaves);
            const hasDirChild = children.some(c => c.metadata.kind === "dir");

            return {
                ...n,
                ...(children.length ? { children } : {}),
                metadata: { ...n.metadata, isLeaf: !hasDirChild },
            };
        },
        insert: async (
            node: Node,
            pathParts: string[],
            fileOrJson: File //| Record<string, unknown>
        ): Promise<Node> => {
            if (pathParts.length === 0) return node;

            const [head, ...rest] = pathParts;
            const existing = node.children?.find(c => c.name === head);

            if (rest.length === 0) {
                debugger
                const content =
                    fileOrJson instanceof File
                        ? await (async () => {
                            try {
                                return JSON.parse(await fileOrJson.text());
                            } catch {

                                return {};
                            }
                        })()
                        : fileOrJson;

                const size =
                    fileOrJson instanceof File ? fileOrJson.size : JSON.stringify(fileOrJson).length;

                const fileNode: Node = {
                    id: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
                    name: head,
                    parent: node.id ?? null,
                    isBranch: false,
                    metadata: {
                        kind: "file",
                        path: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
                        size,
                        checked: true,
                        content,
                    },
                };

                node.children = [...(node.children ?? []), fileNode];
                return node;
            } else {
                let dirNode = existing && existing.metadata.kind === "dir" ? existing : undefined;

                if (!dirNode) {
                    dirNode = {
                        id: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
                        name: head,
                        parent: node.id ?? null,
                        isBranch: true,
                        metadata: {
                            kind: "dir",
                            path: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
                            checked: true,
                        },
                        children: [],
                    };
                    node.children = [...(node.children ?? []), dirNode];
                }

                await VirtualFolders.Operations.insert(dirNode, rest, fileOrJson);
                return node;
            }
        },
        buildTreeFromFolder(
            files: FileList,
            root: Node
        ): Promise<Node> {
            debugger
            return Array.from(files).reduce(
                (promiseAcc, file) =>
                    promiseAcc.then(async (acc) => {
                        const rel = (file as any).webkitRelativePath || file.name; 
                        const parts = rel.split(/[\\/]/).filter(Boolean);
                        return VirtualFolders.Operations.insert(acc, parts, file);
                    }),

                Promise.resolve(root)
            );
        },
        buildTreeFromZipContent(
            files: { path: string[]; content: any }[],
            root: Node
        ): Promise<Node> {
            return Array.from(files).reduce(
                (promiseAcc, file) =>
                    promiseAcc.then(async (acc) => {
       
                        const parts = file.path;
                        return VirtualFolders.Operations.insert(acc, parts, file.content);
                    }),

                Promise.resolve(root)
            );
        },
        fileListToTree: async (files: FileList): Promise<ValueOrErrors<Node, string>> => {
            console.log("Red was here")
            try {
                const root: Node = {
                    id: "root",
                    name: "root",
                    parent: null,
                    isBranch: true,
                    metadata: {kind: "dir", path: "root", checked: true},
                    children: [],
                };
                debugger
                const tree = await VirtualFolders.Operations.buildTreeFromFolder(files, root);
                const result = VirtualFolders.Operations.markLeaves(tree);
                return ValueOrErrors.Default.return(result);
            }
            catch(e:any) {
                return ValueOrErrors.Default.throwOne(e);
            }
        },
        fileArrayToTree: async (files: { path: string[]; content: string }[]): Promise<ValueOrErrors<Node, string>> => {
            try {
                const root: Node = {
                    id: "root",
                    name: "root",
                    parent: null,
                    isBranch: true,
                    metadata: {kind: "dir", path: "root", checked: true},
                    children: [],
                };
                const tree = await VirtualFolders.Operations.buildTreeFromZipContent(files, root);
                const result = VirtualFolders.Operations.markLeaves(tree);
                return ValueOrErrors.Default.return(result);
            } catch (e: any) {
                return ValueOrErrors.Default.throwOne(e);
            }
        }
    }
}
export type WorkspaceForeignMutationsExpected = {
}

export type WorkspaceView = View<
    WorkspaceState,
    WorkspaceState,
    WorkspaceForeignMutationsExpected,
    {
        JsonEditor: Template<
            WorkspaceState,
            WorkspaceState,
            JsonEditorForeignMutationsExpected,
            JsonEditorView
        >
    }
>;
