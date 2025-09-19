import * as React from "react";
import TreeView, {NodeId,  flattenTree } from "react-accessible-treeview";
import { VscFolder, VscFile } from "react-icons/vsc";

import {FlatNode, IFlatMetadata, INode, Meta, VirtualFolders} from "playground-core";
import {BasicFun, Option, Unit} from "ballerina-core";

/*
* used for:
* 1. preselect files/folders for upload 
* 2. selecting current folder after upload (must be a leaf folder)
* */

type AccessibleTreeVfsProps = {
    mode: 'upload' | 'select-current-folder';
    initNodes: FlatNode[];
    onUpload: BasicFun<FlatNode[], void>;
    onSelectedFolder: BasicFun<FlatNode, void>
    expandFoldersByDefault?: boolean;
};

export function AccessibleTreeVfs({
      mode,
      initNodes,
      onUpload,
      onSelectedFolder,
      expandFoldersByDefault = mode == 'select-current-folder'
  }: AccessibleTreeVfsProps) {

    //const data = React.useMemo<FlatNode[]>(() => nodes, [nodes]);
    const [data, setData] = React.useState<FlatNode[]>(initNodes);
    const [selectedFolder, setSelectedFolder] = React.useState<Option<FlatNode>>(Option.Default.none());
    // const folderIds: NodeId[] = React.useMemo(
    //     () => data.filter(d => d.isBranch || (d.children?.length ?? 0) > 0).map(d => d.id),
    //     [data]
    // );
    const folderIds: NodeId[] = initNodes.filter(d => d.isBranch || (d.children?.length ?? 0) > 0).map(d => d.id);
    const folder2 = {
        name: "",
        children: [
            {
                name: "src",
                children: [{ name: "index.js" }, { name: "styles.css" }],
            },
            {
                name: "node_modules",
                children: [
                    {
                        name: "react-accessible-treeview",
                        children: [{ name: "index.js" }],
                    },
                    { name: "react", children: [{ name: "index.js" }] },
                ],
            },
            {
                name: ".npmignore",
            },
            {
                name: "package.json",
            },
            {
                name: "webpack.config.js",
            },
        ],
    };

    const data2 = flattenTree(folder2);
    const renderFolderToggle = React.useCallback(
        (element: FlatNode) => {
            //const isFolder = element.metadata?.kind === "dir";
            //if (!isFolder) return null;

            const path = String(element.metadata?.path ?? element.id).split('/');
           // const checked = stagedPath?.split('/') === path;
            
           // const hasFiles = folderHasDirectFiles(nodes, path);
            const md =
                mode == "select-current-folder" ? <input
                    type="checkbox"
                    className="toggle toggle-xs ml-1"
                    checked={element.metadata.checked}
                    disabled={!element.metadata.isLeafFolder}
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
                        e.stopPropagation();
   
                        setSelectedFolder(Option.Default.some(element))
                        const el = document.querySelector(".accept-selected") as HTMLElement | null;
                        el?.scrollIntoView({ behavior: "smooth", block: "start" });
                    }}
                    onClick={(e) => e.stopPropagation()}
                    aria-label={`Stage folder ${element.name}`}
                    //title={hasFiles ? "Stage this folder" : "No files directly in this folder"}
                />:<input
                    type="checkbox"
                    className="toggle toggle-xs ml-1"
                    checked={element.metadata.checked}
                    disabled={false}
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
                        e.stopPropagation();
                        const next = data.map(x => x.id == element.id ? ({...x, metadata: {...x.metadata, checked: !x.metadata.checked} }): x)
                        setData(next);
                        onUpload(next);
                        //const next = e.currentTarget.checked ? path : null;
                        //onStageChange?.(next);
                        const el = document.querySelector(".accept-selected") as HTMLElement | null;
                        el?.scrollIntoView({ behavior: "smooth", block: "start" });
                    }}
                    onClick={(e) => e.stopPropagation()}
                    aria-label={`Stage folder ${element.name}`}
                    //title={hasFiles ? "Stage this folder" : "No files directly in this folder"}
                />
            return (md);
        
        },
        [mode, initNodes]
    );
    debugger
    return (
        <>{
            mode == 'upload' ? 
                <button
                    //disabled={!canAccept}
                    className="btn btn-accent btn-block mt-7 accept-selected"
                    onClick={() => {
                        onUpload(initNodes);
                    }}
                >
                    `Upload selected items (${initNodes.length})` 
                </button>
               :<button
                    disabled={selectedFolder.kind == "l"}
                    className="btn btn-accent btn-block mt-7 accept-selected"
                    onClick={() => {
                        selectedFolder.kind == "r" && onSelectedFolder(selectedFolder.value);
                    }}
                >
                    `Accept selected folder (${selectedFolder.kind == "r" ? selectedFolder.value.name : ""})`
                </button>}
        <div className="card bg-base-100 shadow w-full">
            <div className="card-body p-3">
                <TreeView
                    data={data2 as any[]}
                    aria-label="Virtual Files"
                    className="text-sm"
                    defaultExpandedIds={expandFoldersByDefault ? folderIds : []}
                    nodeRenderer={(args: any) => {
                        debugger
                        const element = args.element as FlatNode;
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
                                
                                {/*{renderFolderToggle(element)}*/}
                                {!isFolder && typeof element.metadata?.size === "number" && (
                                    <span className="badge badge-ghost badge-xs ml-1">
                    {VirtualFolders.Operations.formatBytes(Number(element.metadata.size))}
                  </span>
                                )}
                                {/*{isFolder && stagedPath === element.metadata?.path && (*/}
                                {/*    <span className="badge badge-primary badge-xs ml-1">staged</span>*/}
                                {/*)}*/}
                            </div>
                        );
                    }}
                />
            </div>
        </div></>
    );
}

export default AccessibleTreeVfs;