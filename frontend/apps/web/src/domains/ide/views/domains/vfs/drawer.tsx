import React from "react";

import {FlatNode, VfsWorkspace, VirtualFolders} from "playground-core";
import {BasicFun, Option, Unit} from "ballerina-core";
import {List} from "immutable";
import MultiSelectCheckboxControlled from "./example.tsx";


type DrawerProps = {
    //selectNodes: BasicFun<FlatNode[], void>;
    //selectFolder: BasicFun<FlatNode, void>;
    mode: 'upload' | 'select-current-folder';
    vfs: VfsWorkspace;
    drawerId?: string;
    onSelectedFolder: (folder: FlatNode) => void;
};

export function Drawer({ mode, vfs, drawerId = 'ide-drawer', onSelectedFolder }: DrawerProps) {
    const [root, setRoot] = React.useState<Option<FlatNode>>(Option.Default.none());
    const [nodes, setNodes] = React.useState<FlatNode>(vfs.nodes);


    return (
        <div className="drawer pt-16">
            <input id={drawerId} type="checkbox" className="drawer-toggle" />
            <div className="drawer-content" />

            <div className="drawer-side top-16 h-[calc(100vh-4rem)] z-40">
                <label htmlFor={drawerId} aria-label="close sidebar" className="drawer-overlay  !bg-white opacity-50" />

                <ul className="menu bg-base-200 text-base-content min-h-full w-[40vw] p-4">
                    <div className="flex w-full">
                        {/*<div className="card bg-primary text-neutral-content w-1/2">*/}
                        {/*    <div className="card-body items-start gap-3">*/}
                        {/*        <h2 className="card-title">Select file</h2>*/}
                        {/*        <div className="flex flex-wrap gap-3">*/}
                        {/*            <input type="file" multiple onChange={handlePick} className="file-input file-input-ghost" />*/}
                        {/*        </div>*/}
                        {/*    </div>*/}
                        {/*</div>*/}
                        
                        {/*<div className="divider divider-horizontal">OR</div>*/}
                    </div>
                    <div className="mt-4">
                            {/*<AccessibleTreeVfs*/}
                            {/*    mode={mode}*/}
                            {/*    initNodes={nodes}*/}
                            {/*    onUpload={selectNodes}*/}
                            {/*    onSelectedFolder={selectFolder}*/}
                            {/*    expandFoldersByDefault*/}
                            {/*/>*/}
                        <MultiSelectCheckboxControlled
                            mode={'reader'} 
                            onSelectedFolder={onSelectedFolder}
                            nodes={nodes} />
                        </div>
                </ul>
            </div>
        </div>
    );
}