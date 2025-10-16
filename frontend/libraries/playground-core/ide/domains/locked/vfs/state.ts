import {Updater,ValueOrErrors} from "ballerina-core";
import {FlatNode, Node} from "./upload/model";
import {SpecOrigin, SpecMode} from "../../spec/state";


export type SelectedWorkspace =
    | { kind: 'folder', folder: Node }
    | { kind: 'file', folder: Node, file: Node }

export type WorkspaceState =
    (
    | { kind: 'view', nodes: Node }
    | { kind: 'selected', nodes: Node, current: SelectedWorkspace }
    ) 
    & Readonly<SpecMode>
    & Readonly<SpecOrigin>

export const WorkspaceState = {
    Default: (nodes: Node, mode: SpecMode, origin: SpecOrigin): WorkspaceState => ({
        kind: 'view',
        nodes: nodes,
        ...mode,
        ...origin
    }),
    Updater: {
        reloadContent: (next: Node): Updater<WorkspaceState> =>
            Updater(workspace => {
                return ({
                    ...workspace,
                    nodes: next,
                })
            }),
        changeFileContent: (newContent: any): Updater<WorkspaceState> =>
            Updater(workspace => {
                if (!(workspace.kind === "selected" && workspace.current.kind === "file")) {
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
        selectFile: (file: Node): Updater<WorkspaceState> =>
            Updater(workspace => {
                const folder = FlatNode.Operations.findFolderByPath(workspace.nodes, file.metadata.path);
                if(folder.kind == "l") {
                    window.alert("design issue: vfs from file content select file");
                    return workspace;
                }
          
                return ({
                    ...workspace,
                    kind: 'selected',
                    current: {
                        kind: 'file',
                        file: file,
                        folder: folder.value
                    }
                })
            }),
        reselectFileAfterMovingToOwnFolder: (): Updater<WorkspaceState> =>
            Updater(workspace => {
                if(!(workspace.kind == 'selected' && workspace.current.kind == 'file')) return workspace;
                const file =  workspace.current.file
                const path = VirtualFolders.Operations.getNestedFileInNamedFolder(file.metadata.path.split("/"))
                const newFile = FlatNode.Operations.findFileByPath(workspace.nodes, path.join("/"))
                const folder = FlatNode.Operations.findFolderByPath(workspace.nodes, path.join("/"))
                if(folder.kind == "l" ) return workspace;
                if(newFile == null) return workspace;
                return ({ 
                    ...workspace,
                    kind: 'selected',
                    current: {
                        kind: 'file',
                        file: newFile,
                        folder: folder.value
                    }
                })
            }),
        defaultForSingleFolder: (): Updater<WorkspaceState> =>
            Updater(workspace =>
                {
                    debugger
                    if(!FlatNode.Operations.hasSingleFolderBelowRoot(workspace.nodes)) {
                        return workspace;
                    }
                    
                    const files = FlatNode.Operations.getFilesForSingleFolderBelowRoot(workspace.nodes);
                    if(files.length  == 0) return workspace;
                    return WorkspaceState.Updater.selectFile(files[0])(workspace);
                }
            ),
        selectFolder: (folder: Node): Updater<WorkspaceState> =>
            Updater(workspace => {
                if(!folder.children || folder.children?.length == 0) {
                    window.alert("folder has no children");
                    return workspace;
                }
                const file = folder.children[0]
                return ({
                    ...workspace,
                    kind: 'selected',
                    current: {
                        kind: 'folder',
                        folder: folder,
                        file: file
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
            fileOrJson: File | Record<string, unknown>
        ): Promise<Node> => {
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
            root: Node
        ): Promise<Node> {
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
            try {
                const root: Node = {
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
        fileArrayToTree: async (files: { path: string[]; content: Record<string, unknown> }[]): Promise<ValueOrErrors<Node, string>> => {
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


