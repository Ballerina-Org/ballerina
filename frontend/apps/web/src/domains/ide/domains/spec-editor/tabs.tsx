/** @jsxImportSource @emotion/react */
import { css } from '@emotion/react'
import React from "react";

const color = 'white'

export const HorizontalButtonContainer: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <div
        css={css`
            display: flex;
            flex-direction: row;
            gap: 16px;
            align-items: center;
            width: fit-content;
            margin: 0 auto;
        `}
    >
        {children}
    </div>
);
export const Tab:React.FC<{ name: string, active?: boolean, size?: number }> = props=> {
    return (
        <div
            css={css`
        padding: ${props.size ??12}px;
        background-color:  ${props.active ? "#083d34" : "#46ae80"};
                color: ${props.active ? "white" : "black"};
        font-size: ${props.size ??12}px;
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