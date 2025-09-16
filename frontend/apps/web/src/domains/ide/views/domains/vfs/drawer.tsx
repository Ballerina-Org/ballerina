import React from "react";
import { DockItem } from "../../layout.tsx";

import AccessibleTreeVfs, {getDirectFilesFromFolder} from "./tree-view";
import {VirtualFolderNode, VirtualFolders, VirtualJsonFile} from "playground-core";
import {BasicFun, Option, Unit} from "ballerina-core";

function filesToVfsFromFileList(specName: string, list: FileList): VirtualFolderNode {
    const root: VirtualFolderNode = {
        kind: "folder",
        name: specName,
        path: specName,
        children: new Map()
    };

    const ensureFolder = (folderPathParts: string[]): VirtualFolderNode => {
        let current = root;
        for (let i = 0; i < folderPathParts.length; i++) {
            const seg = folderPathParts[i];
            let next = current.children.get(seg);
            if (!next) {
                const path = current.path === "/" ? `/${seg}` : `${current.path}/${seg}`;
                next = {
                    kind: "folder",
                    name: seg,
                    path,
                    children: new Map()
                };
                current.children.set(seg, next);
            }
            if (next.kind !== "folder") {
                const path = current.path === "/" ? `/${seg}` : `${current.path}/${seg}`;
                next = {
                    kind: "folder",
                    name: seg,
                    path,
                    children: new Map()
                };
                current.children.set(seg, next);
            }
            current = next;
        }
        return current;
    };


    const defaultFolderParts = [ "core" ];

    Array.from(list).forEach((file) => {
        const rel = (file as any).webkitRelativePath as string | undefined;
        const relParts = (rel && rel.length > 0)
            ? rel.split("/").filter(Boolean)
            : [...defaultFolderParts, file.name];

        const folderParts = relParts.slice(0, -1);
        const filename = relParts[relParts.length - 1];

        const parentFolder = ensureFolder(folderParts);
        if(parentFolder.kind != "folder") return;
        const fullPath = `${parentFolder.path}/${filename}`;

        const vfile: VirtualJsonFile = {
            name: "",
            path: fullPath,
            fileRef: file,
            content: {},   
            topLevels: []      
        };

        parentFolder.children.set(filename, { kind: "file", value: vfile });
    });

    return root;
}
export const drawer = (dockItem: DockItem, selectNode: BasicFun<{ folder: VirtualFolderNode, files: VirtualJsonFile []}, void> ) => {

    const [root, setRoot] = React.useState<Option<VirtualFolderNode>>(Option.Default.none());
    const [open, setOpen] = React.useState(false);

    const [stagedPath, setStagedPath] = React.useState<string | null>(null);

    const handlePick = (e: React.ChangeEvent<HTMLInputElement>) => {
        const list = e.currentTarget.files;
        if (!list || list.length === 0) return;
        
        const specName = "spec";
        const vfsRoot = filesToVfsFromFileList(specName, list);
        
        const maybeCore = ((): string | null => {
            if (vfsRoot.kind !== "folder") return null;
            const core = vfsRoot.children.get("core");
            if (core && core.kind === "folder" && VirtualFolders.Operations.isLeafFolderNode(core)) {
                return core.path;
            }
            return null;
        })();

        if (maybeCore) {
   
            setStagedPath(maybeCore);
        } else {
            setStagedPath(null);
        }

        setRoot(Option.Default.some(vfsRoot));
    };


    const filesToEdit: Option<{ folder: VirtualFolderNode, files: VirtualJsonFile []}> = 
        root.kind == "l" ? Option.Default.none() : Option.Default.some(getDirectFilesFromFolder(root.value,  stagedPath!));
    const canAccept = stagedPath !== null && filesToEdit.kind == "r"; 


    return (
        <div className="drawer pt-16">
            <input id="my-drawer" type="checkbox" className="drawer-toggle" />
            <div className="drawer-content" />

            <div className="drawer-side top-16 h-[calc(100vh-4rem)] z-40">
                <label htmlFor="my-drawer" aria-label="close sidebar" className="drawer-overlay !bg-transparent" />

                <ul className="menu bg-base-200 text-base-content min-h-full w-[40vw] p-4">
                    
                        <>
                            <div className="flex w-full">
                                <div className="card bg-primary text-neutral-content w-1/2">
                                    <div className="card-body items-start gap-3">
                                        <h2 className="card-title">Select file</h2>
                                        <div className="flex flex-wrap gap-3">
                                            <input
                                                type="file"
                                                multiple
                                                onChange={handlePick}
                                                className="file-input file-input-ghost"
                                            />
                                        </div>
                                    </div>
                                </div>
                                <div className="divider divider-horizontal">OR</div>
                                <div className="card bg-primary text-neutral-content w-1/2">
                                    <div className="card-body items-start gap-3">
                                        <h2 className="card-title">Select folder</h2>
                                        <div className="flex flex-wrap gap-3">
                                            <input
                                                type="file"
                                                multiple
                                                onChange={handlePick}
                                                className="file-input file-input-ghost"
                                                {...({ webkitdirectory: "", directory: "" } as any)}
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <button 
                                disabled={!canAccept && filesToEdit.kind == "l"} 
                                className="btn btn-accent btn-block mt-7 accept-selected" 
                                onClick={() =>{
          
                                    return filesToEdit.kind == "r" && selectNode(filesToEdit.value) } }>
                                Accept selected
                            </button>
                           

                            {root.kind == "r" && root.value.kind === "folder" && 
                                <div className="mt-4">
                                    <AccessibleTreeVfs
                                        root={root.value}
                                        stagedPath={stagedPath}
                                        onStageChange={setStagedPath}
                                        expandFoldersByDefault
                                    />
                                </div>}
                        
                        </>
                    
                </ul>
            </div>
        </div>
    );
}