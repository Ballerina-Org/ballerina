import React, { useState } from 'react';
import {Spec} from "playground-core/ide/domains/phases/selection/state.ts";

type Props = {
    label: string;
    options: Spec[];
    onChange?: (value: Spec) => void;
};

export const Dropdown: React.FC<Props> = ({ label, options, onChange }) => {
    const [selected, setSelected] = useState<Spec>({name:""});
    
    const handleChange = (opt:Spec) => {
  
        setSelected(opt);
        if (onChange) onChange(opt);
    };
    return (
        <details className="w-64 pl-3">
            <summary>{label}</summary>
            <ul className="menu dropdown-content bg-base-100 rounded-box z-1 w-52 p-2 shadow-sm">
                {
                    options.map(opt =>
                        (
                            <li>
                                <a onClick={(x) => handleChange(opt)}>
                                    {opt.name}
                                </a>
                            </li>
                        )
                    )
                }
            </ul>
        </details>)
}