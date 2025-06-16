/** @jsxImportSource @emotion/react */
import {style} from "./ide-layout.styled.ts";
import {css} from '@emotion/react';
import {IDE, IDEView} from "playground-core";
import {TmpJsonEditor} from "../domains/spec-editor/json-editor.tsx";
import "react-grid-layout/css/styles.css";
import React from "react";
import {HorizontalButtonContainer, Tab} from "../domains/spec-editor/tabs.tsx";
import {ActionsAndMessages} from "./messages.tsx";
import {FormDisplayTemplate} from "../domains/forms/form-display.tsx";
import {replaceWith} from "ballerina-core";
import {Grid} from "./grid.tsx";

export const IDELayout: IDEView = (props) => (
    <Grid 
        left={<><props.RawJsonEditor{...props} view={TmpJsonEditor} /> </>}
        header={
            <div css={style.headerParent}>
                <div css={style.logoParent}>
                    <img
                        style={{ height: 80 }}
                        src="https://github.com/Ballerina-Org/ballerina/raw/main/docs/pics/Ballerina_logo-04.svg"
                        alt="Ballerina"
                    />
                    <p>IDE</p>
                    <p css={style.stepColor}>{props.context.rawEditor.step.kind}...</p>
                </div>
                <div css={{ flex: 1 }} />
                <div css={style.topTabs}>
                    <HorizontalButtonContainer>
                        <Tab name="tab 1" active={true} />
                        <Tab name="tab 2" />
                    </HorizontalButtonContainer>
                </div>
            </div>}
        right={<>
            <ActionsAndMessages
                onRun={() => props.setState(IDE.Updaters.Core.shouldRun(replaceWith(true))) }
                onSave={() => alert("Saved!")}
   
                clientErrors={props.context.rawEditor.errors.kind == "r" ? [props.context.rawEditor.errors.value]:[]}
                serverErrors={[]}
                clientSuccess={[]}
                serverSuccess={[]}
            />
            <FormDisplayTemplate step={props.context.rawEditor.step} spec={props.context.rawEditor.validatedSpec} />
            </>} />
);