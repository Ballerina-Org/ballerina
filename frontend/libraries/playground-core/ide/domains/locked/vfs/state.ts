import {
    Debounced,
    Option,
    replaceWith,
    simpleUpdater,
    Synchronized,
    Updater,
    Value,
    ValueOrErrors
} from "ballerina-core";
import { Map } from "immutable";
import {FlatNode} from "./upload/model";
import {Ide} from "../../../state";
import {LockedSpec} from "../state";
import {SpecOrigin, SpecMode} from "../../spec/state";
export type TopLevelKey = "types" | "forms" | "apis" | "launchers" | "typesV2" | "schema" | "config";

export type JsonPrimitive = string | number | boolean | null;
export type JsonValue = JsonPrimitive | JsonValue[] | { [k: string]: JsonValue };
export type JsonSection = Record<string, JsonValue>;
export type KnownSections = Partial<Record<TopLevelKey, JsonSection>>;

export type SelectedWorkspace =
    | { kind: 'folder', folder: FlatNode }
    | { kind: 'file', folder: FlatNode, file: FlatNode }

export type ProgressiveWorkspace =
    (
    | { kind: 'stale', nodes: FlatNode }
    | { kind: 'unstale', nodes: FlatNode, current: SelectedWorkspace }
    ) 
    & Readonly<SpecMode>
    & Readonly<SpecOrigin>

export const ProgressiveWorkspace = {
    Default: (nodes: FlatNode, mode: SpecMode, origin: SpecOrigin): ProgressiveWorkspace => ({
        kind: 'stale',
        nodes: nodes,
        ...mode,
        ...origin
    }),
    Updater: {
        changeFileContent: (newContent: any): Updater<ProgressiveWorkspace> =>
            Updater(workspace => {
                if (!(workspace.kind === "unstale" && workspace.current.kind === "file")) {
                    window.alert("design issue: vfs from file content");
                    return workspace;
                }

                return ({
                    ...workspace,
                    current: {
                        ...workspace.current,
                        file: FlatNode.Updaters.Template.fileContent(newContent)(workspace.current.file)
                    }
                })
            }),
        selectFile: (file: FlatNode): Updater<ProgressiveWorkspace> =>
            Updater(workspace => {
                const folder = FlatNode.Operations.findFolderByPath(workspace.nodes, file.metadata.path);
                debugger
                if(folder == null) {
                    window.alert("design issue: vfs from file content select file");
                    return workspace;
                }
                return ({
                    ...workspace,
                    kind: 'unstale',
                    current: {
                        kind: 'file',
                        file: file,
                        folder: folder
                    }
                })
            }),
        selectFolder: (folder: FlatNode): Updater<ProgressiveWorkspace> =>
            Updater(workspace => {
                if (!(workspace.kind === "unstale")) {
                    window.alert("design issue: vfs from file content");
                    return workspace;
                }

                return ({
                    ...workspace,
                    current: {
                        ...workspace.current,
                        kind: 'folder',
                        folder: folder
                    }
                })
            })
    }

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
        markLeaves: (n: FlatNode): FlatNode => {
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
            node: FlatNode,
            pathParts: string[],
            fileOrJson: File | Record<string, unknown>
        ): Promise<FlatNode> => {
            if (pathParts.length === 0) return node;

            const [head, ...rest] = pathParts;
            const existing = node.children?.find(c => c.name === head);

            if (rest.length === 0) {
                const content: Record<string, unknown> =
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

                const fileNode: FlatNode = {
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
        // insert: async (node: FlatNode, pathParts: string[], file: File): Promise<FlatNode> => {
        //     if (pathParts.length === 0) return node;
        //
        //     const [head, ...rest] = pathParts;
        //     const existing = node.children?.find(c => c.name === head);
        //
        //     if (rest.length === 0) {
        //         //TODO: make it a properly validated in the state
        //         const content = await (async () => {
        //             try {
        //                 return JSON.parse(await file.text());
        //             } catch {
        //                 return {};
        //             }
        //         })();
        //         const fileNode: FlatNode = {
        //             id: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
        //             name: head,
        //             parent: node.id ?? null,
        //             isBranch: false,
        //             metadata: {
        //                 kind: "file",
        //                 path: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
        //                 size: file.size,
        //                 checked: true,
        //                 content: content
        //             }
        //         };
        //         node.children = [...(node.children ?? []), fileNode];
        //         return node;
        //     } else {
        //         let dirNode = existing && existing.metadata.kind === "dir" ? existing : undefined;
        //
        //         if (!dirNode) {
        //             dirNode = {
        //                 id: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
        //                 name: head,
        //                 parent: node.id ?? null,
        //                 isBranch: true,
        //                 metadata: {
        //                     kind: "dir",
        //                     path: [...(node.metadata.path === "root" ? [] : [node.metadata.path]), head].join("/"),
        //                     checked: true
        //                 },
        //                 children: [],
        //             };
        //             node.children = [...(node.children ?? []), dirNode];
        //         }
        //
        //         await VirtualFolders.Operations.insert(dirNode, rest, file); 
        //         return node;
        //     }
        // },
        buildTreeFromFolder(
            files: FileList,
            root: FlatNode
        ): Promise<FlatNode> {
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
            files: { path: string[]; content: Record<string, unknown> }[],
            root: FlatNode
        ): Promise<FlatNode> {
            return Array.from(files).reduce(
                (promiseAcc, file) =>
                    promiseAcc.then(async (acc) => {
       
                        const parts = file.path;
                        return VirtualFolders.Operations.insert(acc, parts, file.content);
                    }),

                Promise.resolve(root)
            );
        },
        fileListToTree: async (files: FileList): Promise<ValueOrErrors<FlatNode, string>> => {
            try {
                const root: FlatNode = {
                    id: "root",
                    name: "root",
                    parent: null,
                    isBranch: true,
                    metadata: {kind: "dir", path: "root", checked: true},
                    children: [],
                };
                const tree = await VirtualFolders.Operations.buildTreeFromFolder(files, root);
                const result = VirtualFolders.Operations.markLeaves(tree);
                return ValueOrErrors.Default.return(result);
            }
            catch(e:any) {
                return ValueOrErrors.Default.throwOne(e);
            }
        },
        fileArrayToTree: async (files: { path: string[]; content: Record<string, unknown> }[]): Promise<ValueOrErrors<FlatNode, string>> => {
            try {
                const root: FlatNode = {
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
        ,

    }
}

// export const VfsWorkspace = {
//     Updaters: {
//         Core: {
//             ...simpleUpdater<VfsWorkspace>()("selectedFile"),
//             ...simpleUpdater<VfsWorkspace>()("merged"),
//             ...simpleUpdater<VfsWorkspace>()("selectedFolder"),
//             ...simpleUpdater<VfsWorkspace>()("schema"),
//         }
//     }
// }


