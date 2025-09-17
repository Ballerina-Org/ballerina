import React, {Dispatch, SetStateAction} from "react";
import {Ide} from "playground-core";
import {Themes} from "../../theme-selector.tsx";
import {BasicFun, BasicUpdater} from "ballerina-core";
import {HorizontalDropdown} from "../../dropdown.tsx";

type SelectProps = Ide & { setState: BasicFun<BasicUpdater<Ide>, void> };

export const SelectSpec = (props: SelectProps): React.ReactElement => {
    return props.phase == "choose" && props.origin == "existing" ? <HorizontalDropdown
        label={"Select spec"}
        onChange={async (name: string) => {
            const u = await Ide.Operations.toLockedSpec('existing', name);
            props.setState(u)

        }}
        options={props.existing.specs}/> : <></>
}