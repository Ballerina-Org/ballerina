import React from "react";

type SpecificationLabelProps = { name: string }

export const SpecificationLabel = (props: SpecificationLabelProps): React.ReactElement => {
    return <fieldset className="fieldset pl-5 mt-7">
        <legend className="fieldset-legend pl-2">Specification name</legend>
        <input disabled={true} type="text" className="input" value={props.name} placeholder="Spec name" />
    </fieldset> 
}