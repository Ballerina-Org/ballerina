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
                <div className="filter mb-7 join-item space-x-1">
                    <input
                        className="btn filter-reset"
                        type="radio"
                        name="virtual-files"
                        aria-label="All"
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
                                    type="radio"
                                    name="virtual-files"
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
