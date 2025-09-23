import React, {Dispatch, SetStateAction} from "react";
import {FlatNode, getOrInitSpec, Ide, postVfs, VirtualFolders} from "playground-core";
import {Themes} from "../../theme-selector.tsx";
import {BasicFun, BasicUpdater, Option, Updater, Value} from "ballerina-core";
import {VscFolderLibrary} from "react-icons/vsc";
import MultiSelectCheckboxControlled from "../vfs/example.tsx";
import {fileListToFlatTree} from "./modal.tsx";
import {LocalStorage_SpecName} from "playground-core/ide/domains/storage/local.ts";

type AddSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecInner = (props: AddSpecProps): React.ReactElement => {
    const [node, setNode] = React.useState<Option<FlatNode>>(Option.Default.none());
    const handlePick = React.useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
        const list = e.currentTarget.files;
        if (!list || list.length === 0) return;

        const node = await fileListToFlatTree(list);
        debugger
        if(node.kind == "errors") props.setState(Ide.Updaters.CommonUI.chooseErrors(node.errors))
        else setNode(Option.Default.some(node.value));
    }, []);
    const origin = 'create';
    return <fieldset className="fieldset w-full">
        <div className="join">
            <input
                type="text"
                className="input join-item w-1/2 ml-5"
                placeholder="Spec name"
                value={props.create.name.value}
                onChange={(e) =>
                    props.setState(
                        Ide.Updaters.CommonUI.specName(Value.Default(e.target.value)))
                }
            />

            <form className={"flex"} onSubmit={(e: React.FormEvent) => e.preventDefault()}>
                <button
                    type="submit"
                    className="btn join-item tooltip tooltip-bottom mr-2"
                    data-tip="Create spec with a default files"
                    onClick={ async () => {
                        const vfs = await getOrInitSpec(origin, props.create.name.value);
                        if(vfs.kind == "errors") {
                            props.setState(Ide.Updaters.CommonUI.chooseErrors(vfs.errors))
                            return;
                        }
                        debugger
                        LocalStorage_SpecName.set(props.create.name.value);
                 
                        const u: Updater<Ide> =
                            Ide.Updaters.Phases.lockedPhase(origin,'manual', props.create.name.value, VirtualFolders.Operations.buildWorkspaceFromRoot('create', vfs.value))
                        
                        props.setState(u);
                    }
                    }
                >Create</button>
                <label
                    htmlFor="my-drawer" 
                    className="btn tooltip tooltip-bottom join-item mr-2"
                    onClick={()=>{
                        const u = Ide.Updaters.Phases.startUpload()
                        props.setState(u)
                    }}
                    data-tip="Create with uploading files">
                    <VscFolderLibrary className="mt-2" size={20}/>
                </label>
            </form>
        </div>
        {props.phase == 'choose' && props.details == 'upload-in-progress' && <progress className="progress progress-success w-56" value="100" max="100"></progress>}
        {props.phase == 'choose' && props.details == 'upload-started'
            &&
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
                             
                                const vfs = await getOrInitSpec(origin, props.create.name.value);
                                if(vfs.kind == "errors") {
                                    props.setState(Ide.Updaters.CommonUI.chooseErrors(vfs.errors))
                                    return;
                                }
     

                                if(node.kind == "r") {
                                 
                                    const ch = node.value.children!
                                    const first = ch[0]
                 
                                    const u = Ide.Updaters.Phases.progressUpload()
                                    props.setState(u)
                                    const d = await postVfs(props.create.name.value, node.value);
                              
                                    if(d.kind == "errors") {
                                        props.setState(Ide.Updaters.CommonUI.chooseErrors(d.errors).then(Ide.Updaters.Phases.finishUpload()))
                                        return;
                                    }
                                    // const t = await uploadAllFileNodes(props.create.name.value, node.value);
                                    // debugger
                                    const u2 = 
                                        Ide.Updaters.Phases.finishUpload()
                                            .then(
                                                Ide.Updaters.Phases.lockedPhase(
                                                    'create', 
                                                    'upload', 
                                                    props.create.name.value,
                                                    VirtualFolders.Operations.buildWorkspaceFromRoot('create', node.value)
                                                )   )
                                    props.setState(u2)
                                    LocalStorage_SpecName.set(props.create.name.value);

                                }
        
                            }}
                            disabled={node.kind == "l"}
                            className="btn btn-primary">Upload</button>
                    </div>

                </div>
            </div>   }
        </fieldset> 
}

export const AddSpec = (props: AddSpecProps): React.ReactElement => {
    return props.phase == "choose" && props.specOrigin == 'create'  ? 
        <AddSpecInner {...props} /> : <></>
}

