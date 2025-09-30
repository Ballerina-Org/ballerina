import React from "react";
import {Ide, initSpec} from "playground-core";
import {BasicFun, Updater, Value} from "ballerina-core";
import {VscFileZip, VscFolderLibrary} from "react-icons/vsc";
import {LocalStorage_SpecName} from "playground-core/ide/domains/storage/local.ts";
import {SpecMode, SpecOrigin} from "playground-core/ide/domains/spec/state.ts";

type AddSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecButtons = (props: AddSpecProps): React.ReactElement => {
    
    return  <div className="join">
            <input
                type="text"
                className="input join-item w-1/2 ml-5"
                placeholder="Spec name"
                value={props.name.value}
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
                        const formsMode: SpecMode = { mode: 'scratch', entry: 'upload-manual' };
                        const specOrigin: SpecOrigin = { origin: 'creating'}
                        const vfs = await initSpec(props.name.value, formsMode);
                        if(vfs.kind == "errors") {
                            props.setState(Ide.Updaters.CommonUI.chooseErrors(vfs.errors))
                            return;
                        }
            
                        LocalStorage_SpecName.set(props.name.value);
                 
                        const u: Updater<Ide> =
                            Ide.Updaters.Phases.choosing.toLocked(props.name.value, vfs.value.folders, specOrigin, formsMode)
                        
                        props.setState(u);
                    }
                    }
                >Create spec</button>
                <label
                    htmlFor="my-drawer" 
                    className="btn tooltip tooltip-bottom join-item mr-2"
                    onClick={()=>{
                        const u = Ide.Updaters.Phases.choosing.startUpload('upload-folder');
                        props.setState(u)
                    }}
                    data-tip="Create with uploading files">
                    <VscFolderLibrary className="mt-2" size={20}/>
                </label>
                <label
                    htmlFor="my-drawer"
                    className="btn tooltip tooltip-bottom join-item mr-2"
                    onClick={()=>{
                        const u = Ide.Updaters.Phases.choosing.startUpload('upload-zip');
                        props.setState(u)
                    }}
                    data-tip="Create with uploading files">
                    <VscFileZip className="mt-2" size={20}/>
                </label>
            </form>
        </div>

}
