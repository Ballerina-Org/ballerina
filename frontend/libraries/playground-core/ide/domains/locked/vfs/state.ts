import {Option, simpleUpdater} from "ballerina-core";
import { Map } from "immutable";
export type TopLevelKey = "types" | "forms" | "apis" | "launchers" | "typesV2" | "schema" | "config";

export type JsonPrimitive = string | number | boolean | null;
export type JsonValue = JsonPrimitive | JsonValue[] | { [k: string]: JsonValue };
export type JsonSection = Record<string, JsonValue>;
export type KnownSections = Partial<Record<TopLevelKey, JsonSection>>;

export type VirtualJsonFile = {
    name: string,
    path: string[];                   
    fileRef?: File;                    
    content: KnownSections;          
    topLevels: TopLevelKey[];
    sync?: { status: "local" | "synced"; etag?: string };
};

export type VirtualFolderNode =
    | { kind: "folder"; name: string; path: string[]; children: Map<string, VirtualFolderNode>, staged?: boolean }
    | { kind: "file" } & VirtualJsonFile;
export const VirtualFolderNode = {
    fromFile(f: VirtualJsonFile): VirtualFolderNode {
        return { kind: "file", ...f };
    }
};

type FolderVariant = Extract<VirtualFolderNode, { kind: "folder" }>;
type FileVariant   = Extract<VirtualFolderNode, { kind: "file" }>;

export const isFolder = (n: VirtualFolderNode): n is FolderVariant =>
    n.kind === "folder";

export const isFile = (n: VirtualFolderNode): n is FileVariant =>
    n.kind === "file";

export type VfsWorkspace = {
    root: VirtualFolderNode;                                   
    //files: VirtualJsonFile[];                                  
    merged: Option<KnownSections>;    
    selectedFolder: Option<VirtualFolderNode>;
    selectedFile: Option<VirtualJsonFile>;
};


export const VirtualFolders = {
    Operations: {
        isLeafFolderNode(node: VirtualFolderNode): boolean {
            if (node.kind !== "folder") return false;
            let onlyFiles = true;
            node.children.forEach(child => {
                if (child.kind === "folder") onlyFiles = false;
            });
            return onlyFiles;
        },
        // markFolderAsStaged(root: VirtualFolderNode, targetPath: string): void {
        //     const visit = (node: VirtualFolderNode) => {
        //         if (node.kind === "folder") {
        //             node.staged = node.path === targetPath;
        //             node.children.forEach(visit);
        //         }
        //     };
        //
        //     visit(root);
        // },
        // getStagedFiles(root: VirtualFolderNode): VirtualJsonFile[] {
        //     let stagedFiles: VirtualJsonFile[] = [];
        //
        //     const visit = (node: VirtualFolderNode) => {
        //         if (node.kind === "folder" && node.staged) {
        //             node.children.forEach(child => {
        //                 if (child.kind === "file") {
        //                     stagedFiles.push(child.value);
        //                 }
        //             });
        //         } else if (node.kind === "folder") {
        //             node.children.forEach(visit);
        //         }
        //     };
        //
        //     visit(root);
        //     return stagedFiles;
        // },
        // writeBackToStagedFolder(root: VirtualFolderNode, updatedFiles: VirtualJsonFile[]): void {
        //     const visit = (node: VirtualFolderNode): boolean => {
        //         if (node.kind === "folder" && node.staged) {
        //             const newChildren = Map<string, VirtualFolderNode>();
        //             for (const file of updatedFiles) {
        //                 const name = file.path.split("/").pop()!;
        //                 newChildren.set(name, { kind: "file", value: file });
        //             }
        //             node.children = newChildren;
        //             return true;
        //         } else if (node.kind === "folder") {
        //             node.children.forEach(child => {
        //                 if (visit(child)) return true;
        //             })
        //         }
        //         return false;
        //     };
        //
        //     visit(root);
        // },
        // mergeWorkspaceSections(root: VirtualFolderNode): KnownSections {
        //     const merged: KnownSections = {};
        //
        //     function visit(node: VirtualFolderNode): void {
        //         if (node.kind === "file") {
        //             const file = node.value;
        //
        //             for (const sectionKey of file.topLevels) {
        //                 const section = file.content[sectionKey];
        //                 if (!section) continue;
        //
        //                 if (!merged[sectionKey]) {
        //                     merged[sectionKey] = {};
        //                 }
        //
        //                 Object.assign(merged[sectionKey]!, section);
        //             }
        //         }
        //
        //         if (node.kind === "folder") {
        //             Array.from(node.children.values()).forEach(child => {
        //                 visit(child);
        //             });
        //         }
        //     }
        //
        //     visit(root);
        //     return merged;
        // },
        // buildWorkspaceFromRoot(root: VirtualFolderNode): VfsWorkspace {
        //     const files: VirtualJsonFile[] = [];
        //
        //     const collectFiles = (node: VirtualFolderNode): void => {
        //         if (node.kind === "file") {
        //             files.push(node.value);
        //         } else {
        //             node.children.forEach(child => {
        //                 collectFiles(child);
        //             });
        //         }
        //     };
        //
        //     collectFiles(root);
        //
        //     return {
        //         root,
        //         files,
        //         selectedFile: Option.Default.none(),
        //         selectedFolder: Option.Default.none(),
        //         merged: VirtualFolders.Operations.mergeWorkspaceSections(root)
        //     };
        // },
        buildWorkspaceFromRoot(origin: 'existing' | 'create', root: VirtualFolderNode): VfsWorkspace {
            
            const fs: Option<VirtualJsonFile> =
                //origin == 'create' 
               // && 
                isFolder(root) 
                && Array.from(root.children.values()).filter(x => isFile(x)).length > 0 
                    ? Option.Default.some(Array.from(root.children.values()).filter(x => isFile(x))[0])
                    : Option.Default.none();
            debugger
            return {
                root, 
                merged: Option.Default.none(),
                selectedFile: fs,
                selectedFolder: Option.Default.some(root)
            }
        },
        // createEmptySpec: async (specName: string): Promise<VfsWorkspace> => {
        //     const topLevelKeys: TopLevelKey[] = ["types", "forms", "apis", "launchers", "typesV2", "schema", "config"];
        //
        //     const files: VirtualJsonFile[] = topLevelKeys.map(key => {
        //         const fileName = `${key}.json`;
        //         const virtualPath = `${specName}/core/${fileName}`;
        //
        //         return {
        //             name: fileName,
        //             path: virtualPath,
        //             content: {
        //                 [key]: {}
        //             },
        //             topLevels: [key]
        //         };
        //     });
        //
        //     const coreFolder: VirtualFolderNode = {
        //         kind: "folder",
        //         name: "core",
        //         path: `${specName}/core`,
        //         staged: true, 
        //         children: Map(
        //             files.map(file => {
        //                 const fileName = file.path.split("/").pop()!;
        //                 return [fileName, { kind: "file", value: file }];
        //             })
        //         )
        //     };
        //
        //     const root: VirtualFolderNode = {
        //         kind: "folder",
        //         name: specName,
        //         path: specName,
        //         children: Map([
        //             ["core", coreFolder]
        //         ])
        //     };
        //
        //     return {
        //         root,
        //         files,
        //         selectedFile: Option.Default.none(),
        //         selectedFolder: Option.Default.some(coreFolder),
        //         merged: {
        //             types: {},
        //             forms: {},
        //             apis: {},
        //             launchers: {}
        //         }
        //     };
        // },
        formatBytes: (bytes?: number) => {
            if (bytes == null || !Number.isFinite(bytes)) return "";
            const k = 1024;
            const sizes = ["B", "KB", "MB", "GB", "TB"];
            const i = Math.max(0, Math.floor(Math.log(bytes) / Math.log(k)));
            const n = bytes / Math.pow(k, i);
            return `${n.toFixed(n >= 10 || i === 0 ? 0 : 1)} ${sizes[i]}`;
        },

    }
}

export const VfsWorkspace = {
    Updaters: {
        Core: {
            ...simpleUpdater<VfsWorkspace>()("selectedFile"),
            ...simpleUpdater<VfsWorkspace>()("merged"),
            ...simpleUpdater<VfsWorkspace>()("selectedFolder"),
                    
        }
    }
}