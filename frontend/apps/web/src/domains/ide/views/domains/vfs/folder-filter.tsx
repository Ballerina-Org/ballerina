import {addMissingVfsFiles, getSpec, Ide, INode, Meta, moveIntoOwnFolder, WorkspaceState} from "playground-core";
import React from "react";
import {LockedSpec} from "playground-core/ide/domains/locked/state.ts";
import {VscAdd, VscCopilot, VscMove} from "react-icons/vsc";
import {BasicFun, Unit} from "ballerina-core";
import {CommonUI} from "playground-core/ide/domains/ui/state.ts";

type FolderFilterProps = {
    workspace: WorkspaceState
    update?: any,
    name: string,
    moveIntoOwnFolder: BasicFun<void, Promise<void>> }

function extractKindFromFileName(fileName: string): string {
    const schemaRegex = /_schema(\.[^.]*)?$/;
    const typesV2Regex = /_typesV2(\.[^.]*)?$/;

    if (schemaRegex.test(fileName)) return "schema";
    if (typesV2Regex.test(fileName)) return "typesV2";

    return fileName;
}
function removeExtension(fileName: string): string {
    return fileName.replace(/\.[^/.]+$/, "");
}
export const FolderFilter = ({
     name,
     workspace,
     update,
     moveIntoOwnFolder,
 }: FolderFilterProps) => {
    
    if (workspace.kind !== "selected" ) return null;
    
    const files = (workspace.current.folder.children || [])?.filter(x => x.metadata.kind === "file");
    const specialFilesMissing = 
        workspace.mode == 'explore'
        && workspace.current.kind == 'file'
        && !workspace.current.file.name.endsWith("_schema.json")
        && !workspace.current.file.name.endsWith("_typesV2.json")
        && files && !files.map(f => f.name).includes(`${workspace.current.file.name}_schema.json`)
    
    const parentFileNotYetExtracted =
        workspace.mode == 'explore'
        && workspace.current.kind == 'file'
        && workspace.current.folder.name != removeExtension(workspace.current.file.name)
    return (
        <>
        <div className="w-full">

            {specialFilesMissing && parentFileNotYetExtracted && <div className="flex ">                       
                <div className="mr-3 mt-12 "><VscCopilot size={15} /></div>
                <div className="ml-3 chat chat-start">
                    <div className="chat-bubble chat-bubble-error">
                        <div className="flex items-center w-full">

                            <p className="flex-1">Move into own folder and add v2 files</p>
                            <button
                                className="ml-2 btn btn-xs"
                                onClick={async () => {
                                    await moveIntoOwnFolder()
                                }}
                            ><VscMove size={15} /></button>
                        </div>
                    </div>
                </div>
            </div>}

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
                                    aria-label={extractKindFromFileName(f.name)}
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
