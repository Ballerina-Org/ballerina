import React from "react";

import {FlatNode, FormsMode, VfsWorkspace, VirtualFolders} from "playground-core";
import {BasicFun, FormsConfigMerger, Option, Unit} from "ballerina-core";
import {List} from "immutable";
import MultiSelectCheckboxControlled from "./example.tsx";


type DrawerProps = {
    mode: 'upload' | 'select-current-folder';
    vfs: VfsWorkspace;
    drawerId?: string;
    formsMode: FormsMode;
    onSelectedFolder: (folder: FlatNode) => void;
    onSelectedFile: (file: FlatNode) => void;
};

export function Drawer({ mode, formsMode, vfs, drawerId = 'ide-drawer', onSelectedFolder, onSelectedFile }: DrawerProps) {
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
                    </div>
                    <div className="mt-4">
                        <MultiSelectCheckboxControlled
                            mode={'reader'} 
                            formsMode={formsMode}
                            onSelectedFolder={onSelectedFolder}
                            onSelectedFile={onSelectedFile}
                            nodes={nodes} />
                        </div>
                </ul>
            </div>
        </div>
    );
}