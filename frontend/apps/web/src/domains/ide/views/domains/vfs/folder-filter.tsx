import {WorkspaceState} from "playground-core";
import React from "react";
import {LockedSpec} from "playground-core/ide/domains/locked/state.ts";

type FolderFilterProps = {
    workspace: WorkspaceState
    update?: any
};

export const FolderFilter = ({
     workspace,
     update,
 }: FolderFilterProps) => {
    
    if (workspace.kind !== "selected") return null;
    
    const files = (workspace.current.folder.children || [])?.filter(x => x.metadata.kind === "file");

    return (
        <>
        <div className="w-full">
            <div className="mt-3 flex w-full">
                <div className="form">
                    <input
                        className="btn btn-square"
                        type="reset"
                       // name="virtual-files"
                        value="×"
                    />

                    {files.map((f) => {

                        return (
                            <div
                                key={f.metadata.path}
                                className="tooltip tooltip-bottom"
                                data-tip={`Load ${f.name} into editor`}
                            >
                                <input
                                    className="btn"
                                    type="checkbox"
                                    //name="virtual-files"
                                    checked={workspace.current.kind === "file"  && workspace.current.file.name === f.name}
                                    aria-label={f.name}
                                    onChange={async () => {
                                        const s_u = 
                                            LockedSpec.Updaters.Core.workspace(WorkspaceState.Updater.selectFile(f))
                                        update(s_u);
                                    }}
                                />
                            </div>
                        );
                    })}
                </div>
            </div>
        </div></>
    );
};
