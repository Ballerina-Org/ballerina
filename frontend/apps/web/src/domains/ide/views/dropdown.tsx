/** @jsxImportSource @emotion/react */
import React, { useState } from 'react';
import styled from '@emotion/styled';

type Props = {
    label: string;
    options: string[];
    onChange?: (value: string) => void;
};

export const HorizontalDropdown: React.FC<Props> = ({ label, options, onChange }) => {
    const [selected, setSelected] = useState<string>("");
    
    const handleChange = (opt:string) => {
  
        setSelected(opt);
        if (onChange) onChange(opt);
    };
    return (
        <details className="pl-12">
            <summary>{label}</summary>
            <ul className="menu dropdown-content bg-base-100 rounded-box z-1 w-52 p-2 shadow-sm">
                {
                    options.map(opt =>
                        (
                            <li>
                                <a onClick={(x) => handleChange(opt)}>
                                    {opt}
                                </a>
                            </li>
                        )
                    )
                }
            </ul>
        </details>)
}