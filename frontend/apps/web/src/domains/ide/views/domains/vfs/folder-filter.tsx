import {
    addMissingVfsFiles, FlatNode,
    getSpec,
    Ide,
    IdePhase,
    INode,
    Meta,
    moveIntoOwnFolder,
    WorkspaceState, WorkspaceVariant
} from "playground-core";
import React from "react";
import {LockedPhase} from "playground-core/ide/domains/phases/locked/state.ts";
import {VscAdd, VscCopilot, VscMove} from "react-icons/vsc";
import {BasicFun, Unit} from "ballerina-core";

type FolderFilterProps = {
    workspace: WorkspaceState
    update?: any,
    name: string,
    variant: WorkspaceVariant,
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
     variant,
     moveIntoOwnFolder,
 }: FolderFilterProps) => {
    
    if (workspace.kind !== "selected" ) return null;
    
    const folder = FlatNode.Operations.findFolderByPath(workspace.nodes, workspace.file.metadata.path)
    debugger
    if(folder.kind !== "r") return <p>Cant find folder for a file</p>
    const files = (folder.value.children || [])?.filter(x => x.metadata.kind === "file");
    const specialFilesMissing = 
        variant.kind == 'explore'
        && !workspace.file.name.endsWith("_schema.json")
        && !workspace.file.name.endsWith("_typesV2.json")
        && files && !files.map(f => f.name).includes(`${workspace.file.name}_schema.json`)
    
    const parentFileNotYetExtracted =
        variant.kind == 'explore'
        && folder.value.name != removeExtension(workspace.file.name)
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
                                    checked={ workspace.file.name === f.name}
                                    aria-label={extractKindFromFileName(f.name)}
                                    onChange={async () => {
                                        const s_u = 
                                            Ide.Updaters.Core.phase.locked(
                                            LockedPhase.Updaters.Core.workspace(WorkspaceState.Updater.selectFile(f)))
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
