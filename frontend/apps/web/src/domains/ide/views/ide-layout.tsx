/** @jsxImportSource @emotion/react */
import { css } from '@emotion/react'
import {EditorStep, IDE, IDEView, RawJsonEditor} from "playground-core";
import {RawEditor} from "../domains/raw-editor/raw-editor";
import {TmpJsonEditor} from "../domains/raw-editor/json-editor";
import "react-grid-layout/css/styles.css";
import React, {useEffect, useRef} from "react";
import {HorizontalButtonContainer, Tab} from "../domains/raw-editor/tabs.tsx";
import {ActionsAndMessages} from "./messages.tsx";
import {FormDisplayTemplate} from "../domains/forms/form-display.tsx";
import {Debounced, Option, replaceWith, Synchronized, Value} from "ballerina-core";
import {IDEApi} from "playground-core/ide/apis/spec.ts"; //TODO: dont do it on a presentation level


export const HeaderColumnsLayout: React.FC<{
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

export const IDELayout: IDEView = (props) => (
    <HeaderColumnsLayout 
        left={<><props.RawJsonEditor{...props} view={TmpJsonEditor} /> </>}
        
        header={
            <div
                css={css`
        position: relative;
        width: 100vw;
        display: flex;
        flex-direction: row;
        align-items: center;
        min-height: 80px;
        padding: 0 32px;
      `}
            >
                <div
                    css={css`
          display: flex;
          flex-direction: row;
          align-items: center;
          gap: 8px;
        `}
                >
                    <img
                        style={{ height: 80 }}
                        src="https://github.com/Ballerina-Org/ballerina/raw/main/docs/pics/Ballerina_logo-04.svg"
                        alt="Ballerina"
                    />
                    <p>IDE</p>
                    <p css={css`
                        color: #46ae80; //#083d34;
                    `}>{props.context.rawEditor.step.kind}...</p>
                </div>
                <div css={{ flex: 1 }} />
                <div
                    css={css`
                      position: absolute;
                      left: 50%;
                      transform: translateX(-50%);
                      display: flex;
                    `}
                >
                    <HorizontalButtonContainer>
                        <Tab name="tab 1" active={true} />
                        <Tab name="tab 2" />
                    </HorizontalButtonContainer>
                </div>
            </div>}
        right={<>
            <ActionsAndMessages
                onRun={() => {
                    props.setState(
                        IDE.Updaters.Core.rawEditor(
                            RawJsonEditor.Updaters.Core.step(
                                replaceWith(EditorStep.validating())
                            )
                        ).then( _ => {
                            IDEApi.validateSpec(props.context.rawEditor.inputString)
                                .then(validation => {
                                    switch(validation.isValid) {
                                        case true:
                                            props.setState(
                                                IDE.Updaters.Core.rawEditor(
                                                    RawJsonEditor.Updaters.Core.validatedSpec(
                                                        replaceWith(
                                                            Option.Default.some(props.context.rawEditor.inputString.value)
                                                        )
                                                    ).then(
                                                        RawJsonEditor.Updaters.Core.step(replaceWith(EditorStep.running()))

                                                    )
                                                )
                                            );
                                            break;
                                        case false: {
                                            props.setState(
                                                IDE.Updaters.Core.rawEditor(
                                                    RawJsonEditor.Updaters.Core.validatedSpec(
                                                        replaceWith(Option.Default.none())
                                                    ).then(RawJsonEditor.Updaters.Core.errors(replaceWith(Option.Default.some(validation.errors))))
                                                ));

                                            break;
                                        }
                                    }
                                })
                            return _;
                        })
                    );
                  
                            

                }}
                onSave={() => alert("Saved!")}
                onFormat={() => alert("Formatted!")}
                
                clientErrors={props.context.rawEditor.errors.kind == "r" ? [props.context.rawEditor.errors.value]:[]}
                serverErrors={[]}
                clientSuccess={[]}
                serverSuccess={[]}
            />
            <FormDisplayTemplate step={props.context.rawEditor.step} spec={props.context.rawEditor.validatedSpec} />
            </>} />
);