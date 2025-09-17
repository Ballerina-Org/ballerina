import React, {Dispatch, SetStateAction} from "react";
import {Ide} from "playground-core";
import {Themes} from "../../theme-selector.tsx";
import {BasicFun, BasicUpdater} from "ballerina-core";

type AddSpecProps = Ide & { setState: BasicFun<BasicUpdater<Ide>, void> };

export const AddSpec = (props: AddSpecProps): React.ReactElement => {
    return props.phase == "choose" && props.origin == "create" ? 
        <fieldset className="fieldset ml-4">
            <div className="join">
                <input
                    type="text"
                    className="input join-item"
                    placeholder="Spec name"
                    value={props.create.name.value}
                    onChange={(e) =>
                        props.setState(
                            Ide.Updaters.specName(e.target.value))
                    }
                />
                <button
                    className="btn join-item"
                    onClick={ async () => {
                        const u = await Ide.Operations.toLockedSpec('create', props.create.name.value);
                        props.setState(u);
                    }
                    }
                >GO</button>
    
            </div>
        </fieldset> : <></>
}