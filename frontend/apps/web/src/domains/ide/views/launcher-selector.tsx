import React, { useState } from "react";
import {Option} from "ballerina-core";

const LauncherSelector = (props: {
    options: any[];
    onChange?: (value: any) => void;
}) => {
    const [selected, setSelected] = useState<any>(Option.Default.none());

    const handleChange = (value: any) => {
    
        setSelected(Option.Default.some(value));
        props.onChange?.(value); 
    };
  
    return (
        <div className="join">
           {props.options.map((opt) => (<input onChange={() => handleChange(opt)} className="join-item btn" type="radio" name="options" aria-label={opt.key} checked={selected.kind == "r" && selected.value?.key === opt.key} />))}
        </div>
    );
};

export default LauncherSelector;
