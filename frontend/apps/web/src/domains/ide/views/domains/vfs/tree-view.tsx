import * as React from "react";
import TreeView, {INode, NodeId} from "react-accessible-treeview";
import { VscFolder, VscFile } from "react-icons/vsc";

import {VirtualFolderNode, VirtualFolders, VirtualJsonFile} from "playground-core";


type Meta = {
    kind: "dir" | "file";
    path: string;
    size?: number;
    isLeafFolder?: boolean;
    staged?: boolean;
};
//

const split = (path: string) : string[] => path.split("/");
type FlatNode = INode<Meta>;



export function joinPath(segs: string[] | string): string {
    return Array.isArray(segs) ? segs.join("/") : segs;
}
const parentPath = (segs: string[]): string | null =>{
    if(segs == undefined){
        debugger
    }
    return segs.length <= 1 ? null : joinPath(segs.slice(0, segs.length - 1));}
export const flattenVfsToINodes = (root: VirtualFolderNode): INode<Meta>[] => {
    const go = (f: VirtualFolderNode): INode<Meta>[] => {
        const id = joinPath(f.path);
        if (f.kind === "file") {
            
            return [
                {
                    id,
                    name: f.name,
                    parent: parentPath(f.path),
                    metadata: {
                        kind: "file",
                        path: id,
                        size: f.fileRef?.size,
                        staged: f.sync?.status === "local",
                    },
                    children: [], 
                } as INode<Meta>,
            ];
        }
        
        const isLeafFolder = ![...f.children.values()].some(c => c.kind === "folder");
        return [
            {
                id,
                name: f.name,
                parent: parentPath(f.path),
                metadata: {
                    kind: "dir",
                    path: id,
                    isLeafFolder,
                    staged: f.staged,
                },
                children: [], // flat
            } as INode<Meta>,
            ...[...f.children.values()].flatMap(go),
        ];
    };

    return go(root);
};
type AccessibleTreeVfsProps = {
    root: VirtualFolderNode;
    stagedPath?: string | null;
    onStageChange?: (nextPath: string | null) => void;
    expandFoldersByDefault?: boolean;
};
//
// type FolderNode = Extract<VirtualFolderNode, { kind: "folder" }>;
//
// function isFolder(n: VirtualFolderNode): n is FolderNode {
//     return n.kind === "folder";
// }
//
// function findFolderByPath(root: VirtualFolderNode, path: string): FolderNode  {
//     let found: FolderNode | null = null;
//
//     const visit = (node: VirtualFolderNode): void => {
//         if (found) return;
//         if (isFolder(node)) {
//             if (node.path === path) {
//                 found = node;
//                 return;
//             }
//             node.children.forEach(child => visit(child));
//         }
//     };
//
//     visit(root);
//     return found! as FolderNode;
// }
//
// export function getDirectFilesFromFolder(root: VirtualFolderNode, folderPath: string ):  { folder: FolderNode; files: VirtualJsonFile[] }   {
//
//     const folder = findFolderByPath(root, folderPath);
//
//
//     const files: VirtualJsonFile[] = [];
//     folder?.children.forEach(child => {
//         if (child.kind === "file") files.push(child.value);
//     });
//     return { folder, files };
// }
//
function folderHasDirectFiles(root: VirtualFolderNode, folderPath: string[]): boolean {
    let has = false;
    const visit = (node: VirtualFolderNode): void => {
        if (has) return;
        if (node.kind === "folder") {
            if (node.path === folderPath) {
                node.children.forEach(ch => {
                    if (ch.kind === "file") has = true;
                });
                return;
            }
            node.children.forEach(visit);
        }
    };
    visit(root);
    return has;
}

export function AccessibleTreeVfs({
          root,
          stagedPath = null,
          onStageChange,
          expandFoldersByDefault = true
      }: AccessibleTreeVfsProps) {
    debugger
    const data = React.useMemo<INode<Meta>[]>(() => flattenVfsToINodes(root), [root]);

    const folderIds: NodeId[] = React.useMemo(
        () => data.filter(d => d.isBranch || (d.children?.length ?? 0) > 0).map(d => d.id),
        [data]
    );
    
    const renderFolderToggle = React.useCallback(
        (element: INode<Meta>) => {
            const isFolder = element.metadata?.kind === "dir";
            if (!isFolder) return null;

            const path = String(element.metadata?.path ?? element.id).split('/');
            const checked = stagedPath?.split('/') === path;
            
            const hasFiles = folderHasDirectFiles(root, path);

            return (
                <input
                    type="checkbox"
                    className="toggle toggle-xs ml-1"
                    checked={checked}
                    disabled={!hasFiles}
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
                        e.stopPropagation();
                        const next = e.currentTarget.checked ? path : null;
                        //onStageChange?.(next);
                        const el = document.querySelector(".accept-selected") as HTMLElement | null; 
                        el?.scrollIntoView({ behavior: "smooth", block: "start" });
                    }}
                    onClick={(e) => e.stopPropagation()}
                    aria-label={`Stage folder ${element.name}`}
                    title={hasFiles ? "Stage this folder" : "No files directly in this folder"}
                />
            );
        },
        [stagedPath, onStageChange, root]
    );

    return (
        <div className="card bg-base-100 shadow w-full">
            <div className="card-body p-3">
                <TreeView
                    data={data}
                    aria-label="Virtual Files"
                    className="text-sm"
                    defaultExpandedIds={expandFoldersByDefault ? folderIds : []}
                    nodeRenderer={(args: any) => {
                        const element = args.element as INode<Meta>;
                        const {getNodeProps, level, isBranch, isExpanded, handleExpand} = args;
                        const isFolder = element.metadata?.kind === "dir";
                        return (
                            <div
                                {...getNodeProps({ onClick: handleExpand })}
                                className="flex items-center gap-2 py-1"
                                style={{ marginLeft: (level - 1) * 16 }}
                            >
                                {isBranch ? (
                                    <span className={`transition-transform ${isExpanded ? "rotate-90" : ""}`}>▸</span>
                                ) : (
                                    <span className="w-3 inline-block" />
                                )}

                                {isFolder ? <VscFolder size={15} /> : <VscFile size={15} />}

                                <span className={isFolder ? "font-medium" : "opacity-80"}>
                  {element.name}
                </span>
                                
                                {renderFolderToggle(element)}
                                {!isFolder && typeof element.metadata?.size === "number" && (
                                    <span className="badge badge-ghost badge-xs ml-1">
                    {VirtualFolders.Operations.formatBytes(Number(element.metadata.size))}
                  </span>
                                )}
                                {isFolder && stagedPath === element.metadata?.path && (
                                    <span className="badge badge-primary badge-xs ml-1">staged</span>
                                )}
                            </div>
                        );
                    }}
                />
            </div>
        </div>
    );
}

export default AccessibleTreeVfs;