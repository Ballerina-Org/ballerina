/** @jsxImportSource @emotion/react */
import { css } from '@emotion/react'
import {IDE, IDEView} from "playground-core";
import {TmpJsonEditor} from "../domains/spec-editor/json-editor.tsx";
import "react-grid-layout/css/styles.css";
import React, {useEffect, useRef} from "react";
import {HorizontalButtonContainer, Tab} from "../domains/spec-editor/tabs.tsx";
import {ActionsAndMessages} from "./messages.tsx";
import {FormDisplayTemplate} from "../domains/forms/form-display.tsx";
import {replaceWith} from "ballerina-core";

export const Grid: React.FC<{
    header: React.ReactNode;
    left: React.ReactNode;
    right: React.ReactNode;
}> = ({ header, left, right }) => {
    const leftRef = useRef<HTMLDivElement>(null);
    const dividerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const left = leftRef.current;
        const divider = dividerRef.current;
        if (!left || !divider) return;

        let isResizing = false;

        const handleMouseDown = () => {
            isResizing = true;
            document.body.style.cursor = "col-resize";
        };

        const handleMouseMove = (e: MouseEvent) => {
            if (!isResizing) return;
            const newWidth = e.clientX;
            left.style.width = `${newWidth}px`;
            left.style.flex = "none"; 
        };

        const handleMouseUp = () => {
            isResizing = false;
            document.body.style.cursor = "default";
        };

        //divider.addEventListener("mousedown", handleMouseDown);
        divider.addEventListener("mousedown", () => {
            isResizing = true;
            document.body.style.cursor = "col-resize";
        });
        document.addEventListener("mousemove", handleMouseMove);
        document.addEventListener("mouseup", handleMouseUp);

        return () => {
            divider.removeEventListener("mousedown", handleMouseDown);
            document.removeEventListener("mousemove", handleMouseMove);
            document.removeEventListener("mouseup", handleMouseUp);
        };
    }, []);

    return (
        <div
            css={css`
        height: 100vh;
        display: flex;
        flex-direction: column;
        width: 100vw;
      `}
        >
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
            <div
                css={css`
          flex: 1;
          display: flex;
          flex-direction: row;
          width: 100%;
        position: relative;
        z-index: 10;
        pointer-events: auto;
          min-height: 0;
        `}
            >
                <div
                    ref={leftRef}
                    css={css`
            flex: 1;
            overflow: auto;
            border-right: 1px solid #e5e7eb;
            //padding: 1rem;
            min-width: 100px;
          `}
                >
                    {left}
                </div>
                <div
                    ref={dividerRef}
                    css={css`
                        width: 3px;  
                        min-width: 3px;
                        background: lightgray;
                        cursor: col-resize;
                        //user-select: none;
                        //background: red; /* debug */
                        z-index: 10;
          `}
                ></div>
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
};
