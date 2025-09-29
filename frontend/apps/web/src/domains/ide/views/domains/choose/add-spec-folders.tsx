import React from "react";
import {FlatNode, FormsMode, getOrInitSpec, Ide, postVfs, VirtualFolders} from "playground-core";
import {BasicFun, BasicUpdater, Option, Updater, Value} from "ballerina-core";
import MultiSelectCheckboxControlled from "../vfs/example.tsx";
import {LocalStorage_SpecName} from "playground-core/ide/domains/storage/local.ts";

type AddSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecUploadFolder = (props: AddSpecProps): React.ReactElement => {
    const [node, setNode] = React.useState<Option<FlatNode>>(Option.Default.none());
    const handlePick = React.useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
        const list = e.currentTarget.files;
        if (!list || list.length === 0) return;

        const node = await VirtualFolders.Operations.fileListToTree(list);

        if(node.kind == "errors") props.setState(Ide.Updaters.CommonUI.chooseErrors(node.errors))
        else setNode(Option.Default.some(node.value));
    }, []);
    const origin = 'create';
    return  (props.phase == 'choose' && props.source == 'upload-folder' && props.progressIndicator == 'upload-started')
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
                                    mode={'uploader'}
                                    onAcceptedNodes={(node: FlatNode)=> {
                                    }
                                    }
                                    nodes={node.value} /> }
                        </div>
                    </div>
                    <div className="card-actions justify-end">
                        <button
                            onClick={async ()=>{
                                const formsMode: FormsMode = { kind: 'compose' };
                                const vfs = await getOrInitSpec(origin, formsMode, props.create.name.value);
                                if(vfs.kind == "errors") {
                                    props.setState(Ide.Updaters.CommonUI.chooseErrors(vfs.errors))
                                    return;
                                }
     

                                if(node.kind == "r") {
                                    
                                    const u = Ide.Updaters.Phases.progressUpload()
                                    props.setState(u)
                                    
                                    const d = await postVfs(props.create.name.value, node.value);
                              
                                    if(d.kind == "errors") {
                                        props.setState(Ide.Updaters.CommonUI.chooseErrors(d.errors).then(Ide.Updaters.Phases.finishUpload()))
                                        return;
                                    }
      
                                    const u2 = 
                                        Ide.Updaters.Phases.finishUpload()
                                            .then(
                                                Ide.Updaters.Phases.lockedPhase(
                                                    'create', 
                                                    'upload', 
                                                    props.create.name.value,
                                                    VirtualFolders.Operations.buildWorkspaceFromRoot('create', node.value),
                                                    formsMode
                                                )   )
                                    props.setState(u2)
                                    LocalStorage_SpecName.set(props.create.name.value);

                                }
        
                            }}
                            disabled={node.kind == "l"}
                            className="btn btn-primary">Upload</button>
                    </div>

                </div>
            </div>: <></>   

}
