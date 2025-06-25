/** @jsxImportSource @emotion/react */
import React, { useState } from 'react';
import styled from '@emotion/styled';

const icons = ["🍎", "🍌", "🍇", "🍉", "🍍", "🍒", "🥝", "🥥", "🍑"];

const Wrapper = styled.div`
    display: flex;
    align-items: center;
    margin-left: 50px;
    margin-right:50px;

    min-width: 250px;
    max-width: 250px;
    gap: 1rem;
    overflow: visible;
    flex-shrink: 0;
`;

const Select = styled.select`
    flex: 1; /* fills remaining space within wrapper */
    padding: 0.5rem 1rem;
    width: 100%; // or a fixed value like 150px
    min-width: 100px;
    padding-right: 2rem;
    border: 1px solid #ccc;
    border-radius: 8px;
    font-size: 1rem;
    background-color: white;
    cursor: pointer;

    &:hover {
        border-color: #888;
    }

    &:focus {
        outline: none;
        border-color: #007acc;
    }
`;

type Props = {
  label: string;
  options: string[];
  onChange?: (value: string) => void;
};

export const HorizontalDropdown: React.FC<Props> = ({ label, options, onChange }) => {
  const [selected, setSelected] = useState<string>("");

  const getIcon = (text: string) => {
    // Use text hash to pick consistent icon
    const hash = [...text].reduce((acc, c) => acc + c.charCodeAt(0), 0);
    return icons[hash % icons.length];
  };
  const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    setSelected(value);
    if (onChange) onChange(value);
  };
  return (
    <Wrapper>
      {/*<Label htmlFor="dropdown">{label}</Label>*/}
      <Select
        id="dropdown"
        value={selected}
        onChange={handleChange}
      >
        <option value="" disabled hidden>{label}</option>
        {options.map((opt) => (
          <option key={opt} value={opt}>
            {getIcon(opt)} {opt}
          </option>
        ))}
      </Select>
    </Wrapper>
  );
};
