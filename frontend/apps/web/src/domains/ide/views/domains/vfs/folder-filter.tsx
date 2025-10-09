import {addMissingVfsFiles, moveIntoOwnFolder, WorkspaceState} from "playground-core";
import React from "react";
import {LockedSpec} from "playground-core/ide/domains/locked/state.ts";
import {VscAdd, VscCopilot, VscMove} from "react-icons/vsc";
import {BasicFun, Unit} from "ballerina-core";

type FolderFilterProps = {
    workspace: WorkspaceState
    update?: any,
    addMissingVfsFiles: BasicFun<void, Promise<void>>
    moveIntoOwnFolder: BasicFun<void, Promise<void>> }

export const FolderFilter = ({
     workspace,
     update,
     addMissingVfsFiles,
     moveIntoOwnFolder,
 }: FolderFilterProps) => {
    
    if (workspace.kind !== "selected") return null;
    
    const files = (workspace.current.folder.children || [])?.filter(x => x.metadata.kind === "file");
    const specialFilesMissing = workspace.mode == 'explore' && files?.filter(f => f.name.includes("_schema")).length == 0
    return (
        <>
        <div className="w-full">
            {specialFilesMissing && <div className="ml-3 chat chat-start">
                <div className="chat-bubble chat-bubble-info">
                    <div className="flex items-center w-full">
                        <p className="flex-1">Add schema files?</p>
                        <button
                            className="ml-2 btn btn-xs"
                            onClick={async () => await addMissingVfsFiles()}
                        ><VscAdd size={15} /></button>
                    </div>
                </div>
            </div> }
            <div className="flex ">                       
                <div className="mr-3 mt-12 "><VscCopilot size={15} /></div>
                <div className="ml-3 chat chat-start">
                    <div className="chat-bubble chat-bubble-info">
                        <div className="flex items-center w-full">

                            <p className="flex-1">Move into own folder</p>
                            <button
                                className="ml-2 btn btn-xs"
                                onClick={async () => await moveIntoOwnFolder() }
                            ><VscMove size={15} /></button>
                        </div>
                    </div>
                </div>
            </div>

            <div className="flex w-full">
                
                <div className="form mt-7">
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
