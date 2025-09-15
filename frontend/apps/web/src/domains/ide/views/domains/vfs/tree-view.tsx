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

type FlatNode = INode<Meta>;
export const flattenVfsToINodes = (root: VirtualFolderNode): INode<Meta>[] =>{
    const out: FlatNode[] = [];

    const visit = (node: VirtualFolderNode, parent: string | number | null) => {
        if (node.kind === "folder") {
            const childIds: (string | number)[] = [];
            node.children.forEach(ch => {
                childIds.push(ch.kind === "folder" ? ch.path : ch.value.path);
            });

            out.push({
                id: node.path,
                name: node.name,
                parent,
                children: childIds,
                isBranch: true,
                metadata: {
                    kind: "dir",
                    path: node.path,
                    isLeafFolder: VirtualFolders.Operations.isLeafFolderNode(node),
                    staged: node.staged === true,
                },
            });

            node.children.forEach(ch => {
                if (ch.kind === "folder") {
                    visit(ch, node.path);
                } else {
                    const f = ch.value;
                    const filename = f.path.split("/").pop() || f.path;
                    out.push({
                        id: f.path,
                        name: filename,
                        parent: node.path,
                        children: [], 
                        metadata: {
                            kind: "file",
                            path: f.path,
                            size: f.fileRef?.size,
                        },
                    });
                }
            });
        } else {
            const f = node.value;
            const filename = f.path.split("/").pop() || f.path;
            out.push({
                id: f.path,
                name: filename,
                parent,
                children: [],
                metadata: {
                    kind: "file",
                    path: f.path,
                    size: f.fileRef?.size,
                },
            });
        }
    };

    visit(root, null);
    return out;
}


type AccessibleTreeVfsProps = {
    root: VirtualFolderNode;
    stagedPath?: string | null;
    onStageChange?: (nextPath: string | null) => void;
    expandFoldersByDefault?: boolean;
};

type FolderNode = Extract<VirtualFolderNode, { kind: "folder" }>;

function isFolder(n: VirtualFolderNode): n is FolderNode {
    return n.kind === "folder";
}

function findFolderByPath(root: VirtualFolderNode, path: string): FolderNode  {
    let found: FolderNode | null = null;

    const visit = (node: VirtualFolderNode): void => {
        if (found) return;
        if (isFolder(node)) {
            if (node.path === path) {
                found = node;
                return;
            }
            node.children.forEach(child => visit(child));
        }
    };

    visit(root);
    return found! as FolderNode;
}

export function getDirectFilesFromFolder(root: VirtualFolderNode, folderPath: string ):  { folder: FolderNode; files: VirtualJsonFile[] }   {

    const folder = findFolderByPath(root, folderPath);


    const files: VirtualJsonFile[] = [];
    folder?.children.forEach(child => {
        if (child.kind === "file") files.push(child.value);
    });
    return { folder, files };
}

function folderHasDirectFiles(root: VirtualFolderNode, folderPath: string): boolean {
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
    const data = React.useMemo<INode<Meta>[]>(() => flattenVfsToINodes(root), [root]);

    const folderIds: NodeId[] = React.useMemo(
        () => data.filter(d => d.isBranch || (d.children?.length ?? 0) > 0).map(d => d.id),
        [data]
    );
    
    const renderFolderToggle = React.useCallback(
        (element: INode<Meta>) => {
            const isFolder = element.metadata?.kind === "dir";
            if (!isFolder) return null;

            const path = String(element.metadata?.path ?? element.id);
            const checked = stagedPath === path;
            
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
                        onStageChange?.(next);
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