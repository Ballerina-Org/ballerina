import React from "react";
import {Node, getOrInitSpec, Ide, initSpec, postVfs, VirtualFolders, FlatNode, WorkspaceState} from "playground-core";
import {BasicFun, BasicUpdater, Option, Updater, Value} from "ballerina-core";
import {MultiSelectCheckboxControlled} from "../vfs/workspace-picker.tsx"
import {LocalStorage_SpecName} from "playground-core/ide/domains/storage/local.ts";
import {SpecMode, SpecOrigin} from "playground-core/ide/domains/spec/state.ts";

type AddSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecUploadFolder = (props: AddSpecProps): React.ReactElement => {
    const [node, setNode] = React.useState<Option<Node>>(Option.Default.none());
    const handlePick = React.useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
        const list = e.currentTarget.files;
        if (!list || list.length === 0) return;

        const node = await VirtualFolders.Operations.fileListToTree(list);

        if(node.kind == "errors") props.setState(Ide.Updaters.CommonUI.chooseErrors(node.errors))
        else setNode(Option.Default.some(node.value));
    }, []);
    const formsMode: SpecMode = { mode: 'scratch', entry: 'upload-manual' };
    const specOrigin: SpecOrigin = { origin: 'creating'}
    return  (props.phase == 'choose' && props.choose.entry == 'upload-folder' && props.choose.progressIndicator == 'upload-started')
        ?
            <div className="card bg-gray-200 text-black w-full  mt-12">
                <div className="card-body items-start gap-3">

                    <div className="card-body items-start gap-3">
                        <h2 className="card-title">Select folder</h2>
                        <div className="flex flex-wrap gap-3">
                            <input
                                type="file"
                                multiple
                                onChange={handlePick}
                                className="file-input file-input-ghost"
                                {...({ webkitdirectory: '', directory: '' } as any)}
                            />
                        </div>
                        <div className="mt-4">
                            { node.kind == "r"
                                && <MultiSelectCheckboxControlled
                                    workspace={{
                                        kind: "view",
                                        nodes: node.value,
                                        ...formsMode,
                                        ...specOrigin,
                                    }}
                                    mode={'uploader'}
                                    onAcceptedNodes={(node: Node)=> {
                                    }
                                    } /> }
                        </div>
                    </div>
                    <div className="card-actions justify-end">
                        <button
                            onClick={async ()=>{
                                //TODO: unify this in updaters/operations
                                const vfs = await initSpec(props.name.value, formsMode);
                                if(vfs.kind == "errors") {
                                    props.setState(Ide.Updaters.CommonUI.chooseErrors(vfs.errors))
                                    return;
                                }
                                
                                if(node.kind == "r") {
                                    
                                    const u = Ide.Updaters.Phases.choosing.progressUpload()
                                    props.setState(u)
                                    
                                    const d = await postVfs(props.name.value, node.value);
                              
                                    if(d.kind == "errors") {
                                        props.setState(
                                            Ide.Updaters.CommonUI.chooseErrors(d.errors)
                                                .then(Ide.Updaters.Phases.choosing.finishUpload()))
                                        return;
                                    }
                                    debugger
                                    const u2 = 
                                        Ide.Updaters.Phases.choosing.finishUpload()
                                            .then(
                                                Ide.Updaters.Phases.choosing.toLocked(
                                                    props.name.value,
                                                    node.value,
                                                    {origin: 'creating'},
                                                    formsMode
                                                ))
                                            .then((ide: Ide) => {
                                                if(ide.phase != 'locked') return ide;
                                                
                                                if(!FlatNode.Operations.hasSingleFolderBelowRoot(node.value)) {
                                                    return ide;
                                                }
                                           
                                                return ({...ide,
                                                   locked: {
                                                    ...ide.locked, 
                                                       workspace: WorkspaceState.Updater.defaultForSingleFolder()(ide.locked.workspace) }
                                                })
                                            })
                                    props.setState(u2)
                                    LocalStorage_SpecName.set(props.name.value);

                                }
        
                            }}
                            disabled={node.kind == "l"}
                            className="btn btn-primary">Upload</button>
                    </div>

                </div>
            </div>: <></>   

}
