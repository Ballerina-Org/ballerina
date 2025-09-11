import React, { useState } from "react";
import {Option} from "ballerina-core";

const LauncherSelector = (props: {
    options: string[];
    onChange?: (value: string) => void;
}) => {
    const [selected, setSelected] = useState(Option.Default.none());

    const handleChange = (value: string) => {
        setSelected(Option.Default.some(value));
        props.onChange?.(value); 
    };

    return (
        <div className="join">
           {props.options.map((opt) => (<input onChange={() => handleChange(opt)} className="join-item btn" type="radio" name="options" aria-label={opt} checked={selected.kind == "r" && selected.value === opt} />))}
        </div>
    );
};

export default LauncherSelector;
