/** @jsxImportSource @emotion/react */
import {EditorStep, SpecEditor, RawJsonEditorView, SpecRunner, IDE, SpecRunnerIndicator} from "playground-core";
import {replaceWith} from "ballerina-core";
import { JsonEditor, githubLightTheme } from 'json-edit-react';
import {style} from "./json-editor.styled.ts";
import React from "react";
import {Actions} from "../layout/actions.tsx";
import {IDEApi} from "playground-core/ide/apis/spec.ts";

export const TmpJsonEditor: RawJsonEditorView = (props) => (
    <>

        <div css={style.editor.container}>
          <div css={style.editor.left}>
            <JsonEditor
              rootName="spec"
              data={JSON.parse(props.context.input.value)}
              theme={githubLightTheme}
              onDelete={ x =>
              {
                const value = JSON.stringify(x.newData)
                props.setState(SpecEditor.Updaters.Template.inputString(replaceWith(value)));
              }}
              //onEditEvent={ x => props.setState(SpecEditor.Updaters.Core.step(replaceWith(EditorStep.editing())))}
              onChange={x => x.newValue}
              onUpdate={x =>
              {   const value = JSON.stringify(x.newData)
                props.setState(SpecEditor.Updaters.Template.inputString(replaceWith(value)));
              }}
            />
          </div>
          <div>{ props.context.input.sync.kind == "loaded" &&
              <JsonEditor
                  rootName="example"
                  data={props.context.input.sync.value.payload}
                  theme={githubLightTheme}
              />}
          </div>
        </div>
        
      
    </>
);