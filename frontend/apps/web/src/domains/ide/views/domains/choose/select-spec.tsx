import React, {Dispatch, SetStateAction} from "react";
import {getOrInitSpec, Ide} from "playground-core";
import {Themes} from "../../theme-selector.tsx";
import {BasicFun, BasicUpdater} from "ballerina-core";
import {HorizontalDropdown} from "../../dropdown.tsx";
import {AddSpec} from "./add-spec.tsx";

type SelectProps = Ide & { setState: BasicFun<BasicUpdater<Ide>, void> };

export const SelectSpec = (props: SelectProps): React.ReactElement => {
    return props.phase == "choose" && props.specOrigin == "existing" ? <HorizontalDropdown
        label={"Select spec"}
        onChange={async (name: string) => {
            const vfs = await getOrInitSpec('existing', props.create.name.value);
            if(vfs.kind == "errors") {
                props.setState(Ide.Updaters.CommonUI.chooseErrors(vfs.errors))
                return;
            }
            const u = Ide.Operations.toLockedSpec('existing', name, vfs.value);
            props.setState(u)

        }}
        options={props.existing.specs}/> : <></>
}


