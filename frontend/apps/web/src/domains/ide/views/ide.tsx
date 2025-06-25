/** @jsxImportSource @emotion/react */
import {style} from "./ide.styled.ts";
import { style as editorStyle } from "../domains/spec-editor/json-editor.styled.ts";
import {IDE, IDEView, SpecEditor, SpecEditorIndicator, SpecRunner, SpecRunnerIndicator} from "playground-core";
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
import RadioButtons from "../domains/layout/localisation.tsx";
import {HorizontalDropdown} from "../domains/layout/spec-selector.tsx";

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
                    //TODO: this Run does too much by sharing state internals on a view level
                    onRun={ async () => {
                      const _ = props.setState(IDE.Updaters.Core.runner(SpecRunner.Operations.updateStep(SpecRunnerIndicator.Default.validating())));
                      await IDEApi.validateSpec({value: props.context.editor.input.value})
                        .then(res =>
                          props.setState(
                            IDE.Updaters.Core.runner(SpecRunner.Operations.runEditor(props.context.editor.input.value, res))
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
                  <div css={editorStyle.dividerStyle} />
                  <input
                    type="text"
                    placeholder="instance"
                    css={editorStyle.input}
                    value={props.context.editor.name.value}
                    onChange={(e) => props.setState(IDE.Updaters.Core.editor(SpecEditor.Updaters.Template.name(replaceWith({ value: e.target.value }))))}
                  />
                  <Actions onSave={async () => {}} 
                  
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
                  {/*{props.context.specNames.sync.kind == "loaded" && <p>{props.context.specNames.sync.value.payload.join((", "))}</p>}*/}
                  <HorizontalDropdown 
                          label={"Select a spec"} 
                          onChange={async value =>{
                       
                            const t = await IDEApi.load(value)
              
                            props.setState(IDE.Operations.changeSpec(t.payload));
                          }}
                          options={props.context.specNames.sync.kind != "loaded" ? [] : props.context.specNames.sync.value.payload}/>
                  {props.context.specNames.sync.kind == "reloading" && <p>Checking the database for new spec files ...</p>}
                    {/*<p css={style.stepColor}>{props.context.layout.indicators.step.kind}...</p>*/}
                </div>
                <div css={{ flex: 1 }} />
              <RadioButtons />
            </div>}
        right={
            <>
                <HorizontalButtonContainer>
                  <Tab name="create-person" active={true} />
                  <Tab name="edit-person" />
                  <Tab name="person-transparent"  />
                  <Tab name="person-config" />
                </HorizontalButtonContainer>
                <Messages
                    editorIndicator={props.context.editor.indicator}
                    runnerIndicator={props.context.runner.indicator}
                    clientErrors={[]
                        //props.context.runner.validation.kind == "errors" ? props.context.runner.validation.errors.toArray() : []}
                    }
                    serverErrors={[]}
                    clientSuccess={[]}
                    serverSuccess={[]}
                />
              
              {props.context.runner.validation.kind == "r" && props.context.runner.validation.value.kind == "value" && <FormDisplayTemplate 
                    step={props.context.runner.indicator} 
                    spec={props.context.runner.validation.value.value} />}
                </>
        } />
);