import React, { useState } from "react";
import {Map} from "immutable";
import {Launcher, PassthroughLauncher, Option, ParsedLaunchers} from "ballerina-core";
import {V1} from "playground-core";

const radioGroupStyle: React.CSSProperties = {
    display: "flex",
    gap: "1rem",
    padding: "1rem",
};

const labelStyle: React.CSSProperties = {
    display: "flex",
    alignItems: "center",
    gap: "0.5rem",
    cursor: "pointer",
    border: "1px solid #ccc",
    padding: "0.3rem 0.5rem",
    borderRadius: "8px",
    backgroundColor: "#f9f9f9",
    transition: "0.2s ease",
};

const inputStyle: React.CSSProperties = {
    appearance: "none",
    width: "1rem",
    height: "1rem",
    border: "2px solid #888",
    borderRadius: "50%",
    position: "relative",
    outline: "none",
};

const checkedStyle: React.CSSProperties = {
    ...inputStyle,
    borderColor: "#007acc",
    backgroundColor: "#007acc",
    boxShadow: "inset 0 0 0 3px white",
};

// type Option = { value: string; label: string };

type RadioButtonsProps = {
    options: string[];
    onChange?: (value: string) => void;
};

const RadioButtons = (props: RadioButtonsProps) => {
    const [selected, setSelected] = useState(Option.Default.none());

    const handleChange = (value: string) => {
        setSelected(Option.Default.some(value));
        props.onChange?.(value); // invoke if provided
    };

    return (
        // <div style={radioGroupStyle}>
        //   {props.options.map((opt) => (
        //     <label key={opt.value} style={labelStyle}>
        //       <input
        //         type="radio"
        //         name="choice"
        //         value={opt.value}
        //         checked={selected === opt.value}
        //         onChange={() => handleChange(opt.value)}
        //         style={selected === opt.value ? checkedStyle : inputStyle}
        //       />
        //       {opt.label}
        //     </label>
        //   ))}
        // </div>
        <div className="join">
          
           {props.options.map((opt) => (<input onChange={() => handleChange(opt)} className="join-item btn" type="radio" name="options" aria-label={opt} checked={selected.kind == "r" && selected.value === opt} />))}

        </div>
    );
};

export default RadioButtons;
