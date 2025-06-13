/** @jsxImportSource @emotion/react */
import { css } from '@emotion/react'
import React from "react";

const color = 'white'

export const HorizontalButtonContainer: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <div
        css={css`
      display: flex;
      flex-direction: row;
      gap: 16px;           // space between buttons
      align-items: center; // vertically align buttons
      width: fit-content;  // container fits buttons (optional)
      margin: 0 auto;      // center horizontally (optional)
    `}
    >
        {children}
    </div>
);
export const Tab:React.FC<{ name: string}> = props=> {
    return (
        <div
            css={css`
        padding: 12px;
        background-color: hotpink;
        font-size: 12px;
        border-radius: 4px;
        &:hover {
          color: ${color};
        }
      `}
        >
            {props.name}
        </div>
    )
}