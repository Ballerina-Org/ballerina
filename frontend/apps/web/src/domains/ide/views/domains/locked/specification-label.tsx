import React from "react";
import {Ide} from "playground-core";

type SpecificationLabelProps = Ide;

export const SpecificationLabel = (props: SpecificationLabelProps): React.ReactElement => {
    return props.phase == 'locked' ? <fieldset className="fieldset pl-5 mt-7">
        <legend className="fieldset-legend pl-2">Specification name</legend>
        <input disabled={true} type="text" className="input" value={props.name.value} placeholder="Spec name" />
    </fieldset> : <></>
}