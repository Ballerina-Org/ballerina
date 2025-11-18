import React from "react";
import {Ide, IdePhase, initSpec, WorkspaceVariant} from "playground-core";
import {BasicFun, replaceWith, SimpleCallback, Updater, Value} from "ballerina-core";
import {VscFileZip, VscFolderLibrary} from "react-icons/vsc";
import {LocalStorage_SpecName} from "playground-core/ide/domains/storage/local.ts";
import {List} from "immutable";
import {SelectionPhase} from "playground-core/ide/domains/phases/selection/state.ts";

type AddSpecProps ={
    name: string
    onNameChange: BasicFun<string, void>
    startUpload: SimpleCallback
    onErrors: BasicFun<List<string>, void>
    variant: WorkspaceVariant
    setState: BasicFun<Updater<Ide>, void> //remove this
}
export const AddSpecButtons = (props: AddSpecProps): React.ReactElement => {
    
    return  <div className="join">
            <input
                type="text"
                className="input join-item w-1/2 ml-5"
                placeholder="Spec name"
                value={props.name}
                onChange={(e) => props.onNameChange(e.target.value)}
            />

            <form className={"flex"} onSubmit={(e: React.FormEvent) => e.preventDefault()}>
                <button
                    type="submit"
                    disabled={true}
                    className="btn join-item tooltip tooltip-bottom mr-2"
                    data-tip="Create spec with a default files"
                    onClick={ async () => {
                        const vfs = await initSpec(props.name, props.variant);
                        if(vfs.kind == "errors") {
                            props.setState(Ide.Updaters.Core.phase.selection(SelectionPhase.Updaters.Core.errors(replaceWith(vfs.errors))))
                            return;
                        }
            
                        LocalStorage_SpecName.set(props.name);
                 
                        const u: Updater<Ide> =
                            Ide.Updaters.Core.phase.toLocked(props.name, props.variant, vfs.value.folders)
                        
                        props.setState(u);
                    }
                    }
                >Start</button>
                <label
                    htmlFor="my-drawer"
                    className="btn tooltip tooltip-bottom join-item mr-2"
                    onClick={()=>{
                    const u = Ide.Updaters.Core.phase.selection(s=> ({ ...s, kind: 'upload-started'}));
                        props.setState(u)
                    }}
                    data-tip="Upload zipped specs">
                    <VscFileZip className="mt-2" size={20}/>
                </label>
            </form>
        </div>

}
