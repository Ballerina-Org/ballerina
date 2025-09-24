import {FlatNode, Ide, VfsWorkspace} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";
import {Option, replaceWith, SimpleCallback, Unit, Updater} from "ballerina-core";
import {LockedSpec} from "playground-core/ide/domains/locked/state.ts";

type FolderFilterProps = {
    folder: FlatNode;
    nodes: FlatNode;
    selected: Option<FlatNode>;
    update?: any
};

export const FolderFilter = ({
     folder,
     selected,
     nodes,
     update,
 }: FolderFilterProps) => {
    
    if (folder.metadata.kind !== "dir") return null;
    
    const files = (folder.children || [])?.filter(x => x.metadata.kind === "file");
    
    return (
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
                                        const s_u = LockedSpec.Updaters.Core.vfs(
                                            VfsWorkspace.Updaters.Core.selectedFile(
                                                replaceWith(
                                                    Option.Default.some(f)))
                                        );
                                        update(s_u);
                                    }}
                                />
                            </div>
                        );
                    })}
                </div>
            </div>
        </div>
    );
};
