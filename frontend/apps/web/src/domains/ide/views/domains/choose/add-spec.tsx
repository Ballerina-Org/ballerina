import React, {Dispatch, SetStateAction} from "react";
import {getOrInitSpec, Ide} from "playground-core";
import {Themes} from "../../theme-selector.tsx";
import {BasicFun, BasicUpdater, Updater, Value} from "ballerina-core";

type AddSpecProps = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const AddSpecInner = (props: AddSpecProps): React.ReactElement => {
    return <fieldset className="fieldset ml-4">
            <div className="join">
                <input
                    type="text"
                    className="input join-item"
                    placeholder="Spec name"
                    value={props.create.name.value}
                    onChange={(e) =>
                        props.setState(
                            Ide.Updaters.CommonUI.specName(Value.Default(e.target.value)))
                    }
                />


                <form onSubmit={(e: React.FormEvent) => e.preventDefault()}>
                    <button
                        type="submit"
                        className="btn join-item"
                        onClick={ async () => {
                            const vfs = await getOrInitSpec(props.specOrigin, props.create.name.value);
                            if(vfs.kind == "errors") {
                                props.setState(Ide.Updaters.CommonUI.chooseErrors(vfs.errors))
                                return;
                            }
                            const u: Updater<Ide> = Ide.Operations.toLockedSpec('create', props.create.name.value, vfs.value)// as Updater<Ide>
                            props.setState(u);
                        }
                        }
                    >GO</button></form>

            </div>
        </fieldset> 
}

export const AddSpec = (props: AddSpecProps): React.ReactElement => {
    return props.phase == "choose" && props.specOrigin == 'create'  ? 
        <AddSpecInner {...props} /> : <></>
}

