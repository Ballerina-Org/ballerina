import React, {Dispatch, SetStateAction} from "react";
import {Ide} from "playground-core";
import {Themes} from "../../theme-selector.tsx";
import {BasicFun, BasicUpdater} from "ballerina-core";
import {HorizontalDropdown} from "../../dropdown.tsx";

type SpecificationLabelProps = Ide;

export const SpecificationLabel = (props: SpecificationLabelProps): React.ReactElement => {
    return props.phase == 'locked' ? <fieldset className="fieldset pl-5">
        <legend className="fieldset-legend pl-2">Specification name</legend>
        <input disabled={true} type="text" className="input" value={props.create.name.value} placeholder="Spec name" />
    </fieldset> : <></>
}