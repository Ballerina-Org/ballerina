import React from "react";
import {
    Node,
    initSpec,
    postVfs,
    VirtualFolders,
    FlatNode,
    WorkspaceState, Ide
} from "playground-core";
import {BasicFun, BasicUpdater, Option, replaceWith, Updater, Value} from "ballerina-core";
import {MultiSelectCheckboxControlled} from "../vfs/workspace-picker.tsx"
import {LocalStorage_SpecName} from "playground-core/ide/domains/storage/local.ts";
import {SelectionPhase} from "playground-core/ide/domains/phases/selection/state.ts";

type AddSpecProps = SelectionPhase & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecUploadFolder = (props: AddSpecProps): React.ReactElement => {
    if(props.variant.kind != "compose") return <></>
    
    const [node, setNode] = React.useState<Option<Node>>(Option.Default.none());
    const handlePick = React.useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
        const list = e.currentTarget.files;
        if (!list || list.length === 0) return;

        const node = await VirtualFolders.Operations.fileListToTree(list);

        if(node.kind == "errors") props.setState(Ide.Updaters.Core.phase.selection(SelectionPhase.Updaters.Core.errors(replaceWith(node.errors))))
        else setNode(Option.Default.some(node.value));
    }, []);
    
    return  (props.kind == 'upload-started')
        ?
            <div className="card bg-gray-200 text-black w-full m-5">
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
                                    mode={'reader'}
                                    workspace={{
                                        kind: "view",
                                        nodes: node.value,
                                        variant: props.variant
                                    }}
                                    //mode={'uploader'}
                                    onAcceptedNodes={(node: Node)=> { }} /> }
                        </div>
                    </div>
                    <div className="card-actions justify-end">
                        <button
                            onClick={async ()=>{
                                //TODO: unify this in updaters/operations
                                const vfs = await initSpec(props.name.value, props.variant);
                                if(vfs.kind == "errors") {
                                    props.setState(Ide.Updaters.Core.phase.selection(SelectionPhase.Updaters.Core.errors(replaceWith(vfs.errors))))
                                    return;
                                }
                                
                                if(node.kind == "r") {
                                    
                                    // const u = Ide.Updaters.Phases.choosing.progressUpload()
                                    // props.setState(u)
                                    
                                    const d = await postVfs(props.name.value, node.value);
                              
                                    if(d.kind == "errors") {
                                        props.setState(
                                            Ide.Updaters.Core.phase.selection(
                                                SelectionPhase.Updaters.Core.errors(replaceWith(d.errors)))
                                                .then(Ide.Updaters.Core.phase.selection(s=> ({ ...s, kind: 'upload-finished'}))))

                                        return;
                                    }
                          
                                    const u2 =
                                        Ide.Updaters.Core.phase.selection(s=> ({ ...s, kind: 'upload-finished'}))
                                            .then(
                                                Ide.Updaters.Core.phase.toLocked(props.name.value, props.variant, node.value)
                                            )
                                            .then((ide: Ide) => {
                                                if(ide.phase.kind != 'locked') return ide;
                                                
                                                if(!FlatNode.Operations.hasSingleFolderBelowRoot(node.value)) {
                                                    return ide;
                                                }
                                           
                                                return ({...ide,
                                                   locked: {
                                                    ...ide.phase.locked, 
                                                       workspace: WorkspaceState.Updater.defaultForSingleFolder()(ide.phase.locked.workspace) }
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
