import React, { useState } from "react";

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
  padding: "0.5rem 1rem",
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

const RadioButtons = () => {
  const [selected, setSelected] = useState("ch");

  const options = [
    { value: "de", label: "de" },
    { value: "ch", label: "ch" },
    { value: "en", label: "en" },
    { value: "pl", label: "pl" },
  ];

  return (
    <div style={radioGroupStyle}>
      {options.map((opt) => (
        <label key={opt.value} style={labelStyle}>
          <input
            type="radio"
            name="choice"
            value={opt.value}
            checked={selected === opt.value}
            onChange={() => setSelected(opt.value)}
            style={selected === opt.value ? checkedStyle : inputStyle}
          />
          {opt.label}
        </label>
      ))}
    </div>
  );
};

export default RadioButtons;