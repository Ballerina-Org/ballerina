import React from "react";

import {FlatNode, ProgressiveWorkspace} from "playground-core";
import {BasicFun, FormsConfigMerger, Option, Unit} from "ballerina-core";
import {List} from "immutable";
import { MultiSelectCheckboxControlled } from "./workspace-picker.tsx";

type DrawerProps = {
    mode: 'upload' | 'select-current-folder';
    workspace: ProgressiveWorkspace;
    drawerId?: string;
    onSelectedFolder: (folder: FlatNode) => void;
    onSelectedFile: (file: FlatNode) => void;
};

export function Drawer({ mode, workspace, drawerId = 'ide-drawer', onSelectedFolder, onSelectedFile }: DrawerProps) {
    const [root, setRoot] = React.useState<Option<FlatNode>>(Option.Default.none());
    const [nodes, setNodes] = React.useState<FlatNode>(workspace.nodes);

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
                            workspace={workspace}
                            onSelectedFolder={onSelectedFolder}
                            onSelectedFile={onSelectedFile} />
                        </div>
                </ul>
            </div>
        </div>
    );
}