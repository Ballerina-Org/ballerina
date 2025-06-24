/** @jsxImportSource @emotion/react */
import {style} from "./ide.styled.ts";
import { style as editorStyle } from "../domains/spec-editor/json-editor.styled.ts";
import {IDE, IDEView, SpecEditor, SpecRunner, SpecRunnerIndicator} from "playground-core";
import {TmpJsonEditor} from "../domains/spec-editor/json-editor.tsx";
import "react-grid-layout/css/styles.css";
import React from "react";
import {HorizontalButtonContainer, Tab} from "../domains/spec-editor/tabs.tsx";
import {Actions} from "../domains/layout/actions"
import {Messages} from "../domains/spec-runner/messages";
import {FormDisplayTemplate} from "../domains/forms/form-display.tsx";
import {replaceWith, ValueOrErrors} from "ballerina-core";
import {Grid} from "./grid.tsx";
import {IDEApi} from "playground-core/ide/apis/spec.ts";
import {Option} from "ballerina-core";

export const IDELayout: IDEView = (props) => (
    <Grid 
        left={
        <>
            <div css={editorStyle.container}>
                <div css={editorStyle.row}>
                    <input
                      type="text"
                      placeholder="spec name"
                      css={editorStyle.input}
                      value={props.context.editor.name.value}
                      onChange={(e) => props.setState(IDE.Updaters.Core.editor(SpecEditor.Updaters.Template.name(replaceWith({ value: e.target.value }))))}
                    />
                    <label css={editorStyle.switchWrapper}>
                        <input
                          type="checkbox"
                          checked={true}
                          //onChange={(e) => setIsChecked(e.target.checked)}
                          css={editorStyle.visuallyHidden}
                        />
                        <span css={[editorStyle.switchTrack, true && editorStyle.switchTrackChecked]}>
                          <span css={[editorStyle.switchThumb, true && editorStyle.switchThumbChecked]} />
                        </span>
                        <span css={editorStyle.labelText}>Preview</span>
                    </label>
                  <Actions
                    onRun={ async () => {
                      await IDEApi.validateSpec({value: props.context.editor.input.value})
                        .then(res =>
                          props.setState(
                            SpecRunner.Operations.runEditor(props.context.editor.input.value, res)
                          )
                        )
                    }}
                    onSave={ async () =>
                    {
                      //const res = await IDEApi.lock(props.context.editor.input);
                      const entity = await IDEApi.save(props.context.editor.name.value, props.context.editor.input.value)
                      props.setState(
                        IDE.Updaters.Core.runner(
                          SpecRunner.Updaters.Core.indicator(
                            replaceWith(SpecRunnerIndicator.Default.locked())
                          )
                        ).then (x =>{
                          alert(entity);
                          return x
                        })
                      )
                    }
                    }
                  />
                </div>
              
            </div>

            <props.RawJsonEditor{...props} view={TmpJsonEditor} /></>}
        header={
            <div css={style.headerParent}>
                <div css={style.logoParent}>
                    <img
                        style={{ height: 80 }}
                        src="https://github.com/Ballerina-Org/ballerina/raw/main/docs/pics/Ballerina_logo-04.svg"
                        alt="Ballerina"
                    />
                    <p>IDE</p>
                    {/*<p css={style.stepColor}>{props.context.layout.indicators.step.kind}...</p>*/}
                </div>
                <div css={{ flex: 1 }} />
                <div css={style.topTabs}>
                    <HorizontalButtonContainer>
                        <Tab name="tab 1" active={true} />
                        <Tab name="tab 2" />
                    </HorizontalButtonContainer>
                </div>
            </div>}
        right={
            <>

                <Messages
                    indicator={props.context.runner.indicator}
                    clientErrors={[]
                        //props.context.runner.validation.kind == "errors" ? props.context.runner.validation.errors.toArray() : []}
                    }
                    serverErrors={[]}
                    clientSuccess={[]}
                    serverSuccess={[]}
                />
                <FormDisplayTemplate 
                    step={props.context.layout.indicators.step} 
                    spec={props.context.runner.validation} />
                </>
        } />
);