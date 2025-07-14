/** @jsxImportSource @emotion/react */
import {
  SpecEditor,
  RawJsonEditorView,
  SpecEditorIndicator
} from "playground-core";
import {replaceWith} from "ballerina-core";
import { JsonEditor, githubLightTheme,githubDarkTheme,psychedelicTheme,monoLightTheme } from 'json-edit-react';
import {style} from "./json-editor.styled.ts";
import React from "react";
import {CheckCircle, Edit, Delete, Check, SquareX, ArrowDown} from "lucide-react";

export const TmpJsonEditor: RawJsonEditorView = (props) => (
    <>

        <div css={style.editor.container}>
          {/*<p>{props.context.indicator.kind}</p>*/}
          <div css={style.editor.left}>
            <JsonEditor
              icons={{
                delete: <Delete size={20}/>, 
                edit:<Edit size={20}/>, 
                ok: <Check size={20}/>, 
                cancel: <SquareX  size={20}/>, 
                add:<CheckCircle size={20}/>,
                chevron:<ArrowDown size={20}/>,
                copy:<CheckCircle size={20}/>,
              }}

              collapse={1}
              rootName="spec"
              data={props.context.input.value ? JSON.parse(props.context.input.value) : {}}
              theme={monoLightTheme}
              onDelete={ x =>
              {
                const value = JSON.stringify(x.newData)
                props.setState(SpecEditor.Updaters.Template.inputString(replaceWith(value)));
              }}
              
              onEditEvent={ x => {
                if (x == null && props.context.indicator.kind == "editing") props.setState(SpecEditor.Updaters.Core.indicator(replaceWith(SpecEditorIndicator.Default.idle())));
                else if (x != null) props.setState(SpecEditor.Updaters.Core.indicator(replaceWith(SpecEditorIndicator.Default.editing())))}}
              onChange={x => {
                props.setState(SpecEditor.Updaters.Core.indicator(replaceWith(SpecEditorIndicator.Default.idle())))
                return x.newValue}}
              onUpdate={x =>
              {   const value = JSON.stringify(x.newData)
                props.setState(SpecEditor.Updaters.Template.inputString(replaceWith(value)));
                props.setState(SpecEditor.Updaters.Core.indicator(replaceWith(SpecEditorIndicator.Default.idle())))
              }}
            />
          </div>
        </div>
    </>
);