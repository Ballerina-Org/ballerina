import React from "react";
import {FlatNode, FormsMode, getOrInitSpec, Ide, postVfs, VirtualFolders} from "playground-core";
import {BasicFun, Updater, Value} from "ballerina-core";
import {VscFileZip, VscFolderLibrary} from "react-icons/vsc";
import {LocalStorage_SpecName} from "playground-core/ide/domains/storage/local.ts";

type AddSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecButtons = (props: AddSpecProps): React.ReactElement => {

    const origin = 'create';
    return  <div className="join">
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
                        const formsMode: FormsMode = { kind: 'compose' };
                        const vfs = await getOrInitSpec(origin, formsMode, props.create.name.value);
                        if(vfs.kind == "errors") {
                            props.setState(Ide.Updaters.CommonUI.chooseErrors(vfs.errors))
                            return;
                        }
            
                        LocalStorage_SpecName.set(props.create.name.value);
                 
                        const u: Updater<Ide> =
                            Ide.Updaters.Phases.lockedPhase(origin,'manual', props.create.name.value, VirtualFolders.Operations.buildWorkspaceFromRoot('create', vfs.value), formsMode)
                        
                        props.setState(u);
                    }
                    }
                >Create</button>
                <label
                    htmlFor="my-drawer" 
                    className="btn tooltip tooltip-bottom join-item mr-2"
                    onClick={()=>{
                        const u = Ide.Updaters.Phases.startUpload('upload-folder')
                        props.setState(u)
                    }}
                    data-tip="Create with uploading files">
                    <VscFolderLibrary className="mt-2" size={20}/>
                </label>
                <label
                    htmlFor="my-drawer"
                    className="btn tooltip tooltip-bottom join-item mr-2"
                    onClick={()=>{
                        const u = Ide.Updaters.Phases.startUpload('upload-zip')
                        props.setState(u)
                    }}
                    data-tip="Create with uploading files">
                    <VscFileZip className="mt-2" size={20}/>
                </label>
            </form>
        </div>

}
