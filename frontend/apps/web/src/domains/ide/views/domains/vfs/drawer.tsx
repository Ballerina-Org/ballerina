import React from "react";

import {getZippedWorkspace, Node, WorkspaceState} from "playground-core";
import {BasicFun, FormsConfigMerger, Option, Unit} from "ballerina-core";
import {List} from "immutable";
import { MultiSelectCheckboxControlled } from "./workspace-picker.tsx";
import {VscExport} from "react-icons/vsc";

type DrawerProps = {
    mode: 'upload' | 'select-current-folder';
    workspace: WorkspaceState;
    name: string;
    drawerId?: string;
    onSelectedFolder: (folder: Node) => void;
    onSelectedFile: (file: Node) => void;
    onAddNewFile: (path: string) => void;
};

export function Drawer({ mode, workspace, drawerId = 'ide-drawer', name, onSelectedFolder, onSelectedFile, onAddNewFile }: DrawerProps) {
    const [root, setRoot] = React.useState<Option<Node>>(Option.Default.none());
    const [nodes, setNodes] = React.useState<Node>(workspace.nodes);

    return (
        <div className="drawer pt-16">
            <input id={drawerId} type="checkbox" className="drawer-toggle" />
            <div className="drawer-content" />

            <div className="drawer-side top-16 h-[calc(100vh-4rem)] z-40">
                <label htmlFor={drawerId} aria-label="close sidebar" className="drawer-overlay  !bg-white opacity-50" />

                <ul className="menu bg-base-200 text-base-content min-h-full w-[40vw] p-4">
                    <div className="flex w-full">
                        <button 
                            className="btn btn-accent btn-dash"
                            onClick={async ()=> {
                                const response = await getZippedWorkspace(name);
                                if(response.kind == "errors") return;
                                debugger
                                const blob = new Blob([response.value], { type: "application/zip" });
                                const url = window.URL.createObjectURL(blob);

                                const a = document.createElement("a");
                                a.href = url;
                                a.download = `${name}.zip`; 
                                document.body.appendChild(a);
                                a.click();
                                a.remove();
                                window.URL.revokeObjectURL(url);
                            }}
                        >
                            Export
                            <VscExport size={20} />
                        </button>
                    </div>
                    <div className="mt-4">
                        <MultiSelectCheckboxControlled
                            mode={'reader'} 
                            workspace={workspace}
                            onSelectedFolder={onSelectedFolder}
                            onAddNewFile={onAddNewFile}
                            onSelectedFile={onSelectedFile} />
                        </div>
                </ul>
            </div>
        </div>
    );
}