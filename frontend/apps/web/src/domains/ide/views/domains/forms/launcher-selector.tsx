import React, { useState } from "react";
import {Option} from "ballerina-core";

const LauncherSelector = (props: {
    options: string[];
    onChange?: (value: any) => void;
}) => {
    const [selected, setSelected] = useState<any>(Option.Default.none());

    const handleChange = (value: any) => {

        setSelected(Option.Default.some(value));
        props.onChange?.(value);
    };

    return (
        <div className="join">
            {props.options.map((opt) => (<input onClick={() => handleChange(opt)} className="join-item btn" type="radio" name="options" aria-label={opt} checked={selected.kind == "r" && selected.value === opt} />))}
        </div>
    );
};

export default LauncherSelector;
