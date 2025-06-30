/** @jsxImportSource @emotion/react */
import {style} from "./ide.styled.ts";
import { style as editorStyle } from "../domains/spec-editor/json-editor.styled.ts";
import {IDE, IDEView, SpecEditor, SpecRunner, SpecRunnerIndicator} from "playground-core";
import {TmpJsonEditor} from "../domains/spec-editor/json-editor.tsx";
import "react-grid-layout/css/styles.css";
import React, {useState} from "react";
import {Actions} from "../domains/layout/actions"
import {Messages} from "../domains/spec-runner/messages";
import {FormDisplayTemplate} from "../domains/forms/form-display.tsx";
import {replaceWith, Value, Option, ValueOrErrors} from "ballerina-core";
import {Grid} from "./grid.tsx";
import {IDEApi} from "playground-core/ide/apis/spec.ts";
import RadioButtons from "../domains/layout/localisation.tsx";
import {HorizontalDropdown} from "../domains/layout/spec-selector.tsx";


export const IDELayout: IDEView = (props) =>{
  const [key, setKey] = useState(0);
return (
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
                      onChange={(e) => 
                        props.setState(
                          IDE.Updaters.Core.editor(
                            SpecEditor.Updaters.Template.name(
                              replaceWith({ value: e.target.value }))))}
                    />

                  <Actions
                    //TODO: this Run does too much by sharing state internals on a view level
                    
                    onReload={()=> setKey(prev => prev + 1)}
                    onNew={()=>{}} //TODO
                    onRun={ async () => {
                      const _ = 
                        props.setState(
                          IDE.Updaters.Core.runner(
                            SpecRunner.Operations.updateStep(
                              SpecRunnerIndicator.Default.validating())));
                      const res = await IDEApi.validateSpec({value: props.context.editor.input.value});
                      const seed = await IDEApi.seed(Value.Default(props.context.editor.input.value));
                      props.setState(
                        IDE.Updaters.Core.entityBody(
                          replaceWith({ value: seed.payload}))
                          .then(
                            IDE.Updaters.Core.runner(
                              SpecRunner.Operations.runEditor(
                                props.context.editor.input.value, props.context.entityBody.value, res
                              )
                            )
                        )
                      )
                    }}
                    onSave={ async () => {
                      const res = await IDEApi.validateSpec({value: props.context.editor.input.value});

                      if (res.isValid) {
                        const entity = await IDEApi.save(props.context.editor.name.value, props.context.editor.input.value)
                        const entityNames = await IDEApi.entity_names(props.context.specName.value);
                        props.setState(
                          IDE.Updaters.Core.runner(ide => {
                            alert("Saved");
                            return ide
                          }).then(IDE.Updaters.Core.entityNames(
                            replaceWith(entityNames.payload)
                          ))
                        )
                      } else {
                        props.setState(IDE.Updaters.Core.runner(SpecRunner.Updaters.Core.validation(
                          replaceWith(
                            Option.Default.some(ValueOrErrors.Default.throwOne(res.errors))
                          )
                        )));
                      }

                    }
                    }
                  />
                </div>
              
            </div>
          
            <props.RawJsonEditor{...props} view={TmpJsonEditor} /><p>{JSON.stringify(props.context.entityBody.value)}</p></>}
        header={
            <div css={style.headerParent}>
                <div css={style.logoParent}>
                    <img
                        style={{ height: 80 }}
                        src="https://github.com/Ballerina-Org/ballerina/raw/main/docs/pics/Ballerina_logo-04.svg"
                        alt="Ballerina"
                    />
                    <p>IDE</p>
                  <HorizontalDropdown 
                      label={"Select spec"} 
                      onChange={async name =>{
                   
                        const t = await IDEApi.load(name)

                        props.setState(
                          IDE.Operations.changeSpec(name, t.payload.spec).then(
                            IDE.Updaters.Core.launchers(replaceWith(t.payload.launchers)))
                            .then(
                              IDE.Updaters.Core.editor(SpecEditor.Updaters.Template.inputString(
                                replaceWith(t.payload.spec)
                                ))
                          )
                        );
                      }}
                      options={props.context.specNames}/>
                </div>
                <div css={{ flex: 1 }} />

            </div>}
        right={
            <>
                <Messages
                    editorIndicator={props.context.editor.indicator}
                    runnerIndicator={props.context.runner.indicator}
                    clientErrors={
                        props.context.runner.validation.kind == "r" 
                        &&  props.context.runner.validation.value.kind == "errors" 
                        ? props.context.runner.validation.value.errors.toArray() : []
                    }
                    serverErrors={[]}
                    clientSuccess={[]}
                    serverSuccess={[]}
                />
              <p>Entities:</p>
              <RadioButtons
                onChange={async (value) => {
                  const launchers = await IDEApi.launcher_names(props.context.specName.value, value)
                  props.setState(
                    IDE.Updaters.Core.entityName(
                      replaceWith(
                        Option.Default.some(
                          Value.Default(value)
                        )
                      )
                    ).then(IDE.Updaters.Core.launchers(
                          replaceWith(
                            launchers.payload
                          )
                      ))
                    
                  )
                }
              }
                options={
                  Array.isArray(props.context.entityNames) ? props.context.entityNames.map( en => ({value: en, label: en})) : []}  />
              <p>Launchers:</p>
              { <RadioButtons 
                onChange={async (value) => {
               
                  props.setState(
                    IDE.Updaters.Core.launcherName(
                      replaceWith(
                        Option.Default.some(
                          Value.Default(value)
                        )
                      )
                    )

                  )
                }}
                options={
                  Array.isArray(props.context.launchers) ? props.context.launchers.map( launcher => ({value: launcher, label: launcher})) : []}  />

              }
              {props.context.runner.validation.kind == "r" 
                && props.context.runner.validation.value.kind == "value" 
                && props.context.entityName.kind == "r"
                && props.context.launcherName.kind == "r"
                && <FormDisplayTemplate key={key} 
                    entityName={props.context.entityName.value.value} 
                    launcherName={props.context.launcherName.value.value}
                    step={props.context.runner.indicator} 
                    example={props.context.entityBody.value}
                    specName={props.context.specName.value}
                    spec={props.context.runner.validation.value.value} />}
                </>
        } />
)};