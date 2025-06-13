/** @jsxImportSource @emotion/react */
import { css } from '@emotion/react'
import {IDEView} from "playground-core";
import {RawEditorArea} from "../domains/raw-editor/raw-editor";
import GridLayout from "react-grid-layout";
import "react-grid-layout/css/styles.css";
import React from "react";
import {HorizontalButtonContainer, Tab} from "../domains/raw-editor/tabs.tsx";
import {ActionsAndMessages} from "./messages.tsx";

export const HeaderColumnsLayout: React.FC<{
    header: React.ReactNode;
    left: React.ReactNode;
    right: React.ReactNode;
}> = ({ header, left, right }) => (
    <div
        css={css`
      height: 100vh;
      display: flex;
      flex-direction: column;
      width: 100vw;
    `}
    >
        {/* Header */}
        <div
            css={css`
        height: 100px;
        flex-shrink: 0;
        width: 100%;
        background: #f1f5f9;
        display: flex;
        align-items: center;
        justify-content: center;
        border-bottom: 1px solid #e5e7eb;
      `}
        >
            {header}
        </div>
        {/* Columns container */}
        <div
            css={css`
        flex: 1;
        display: flex;
        flex-direction: row;
        width: 100%;
        min-height: 0; /* prevents overflow if columns are scrollable */
      `}
        >
            <div
                css={css`
          flex: 1;
          overflow: auto;
          border-right: 1px solid #e5e7eb;
        `}
            >
                {left}
            </div>
            <div
                css={css`
          flex: 1;
          overflow: auto;
        `}
            >
                {right}
            </div>
        </div>
    </div>
);

export const IDELayout: IDEView = (props) => (
    <HeaderColumnsLayout 
        left={<props.RawJsonEditor{...props} view={RawEditorArea} />} 
        header={       
            <HorizontalButtonContainer>
                <Tab
                    name="JSON code"
                />
                <Tab
                    name="Forms"
                />
                <Tab
                    name="Analyses"
                />
        </HorizontalButtonContainer>}
        right={
        
        <>
            <ActionsAndMessages
                onRun={() => alert("Running!")}
                onSave={() => alert("Saved!")}
                onFormat={() => alert("Formatted!")}
                clientErrors={["Client validation failed: Field X is required."]}
                serverErrors={[
                    "Server error: Database connection failed.",
                    "Server error: Unexpected response.",
                ]}
                clientSuccess={["All client checks passed!"]}
                serverSuccess={["Data saved successfully on server!"]}
            />
            <p>Forms</p></>} />
);