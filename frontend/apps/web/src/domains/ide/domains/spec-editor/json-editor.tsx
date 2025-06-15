import {EditorStep, RawJsonEditor, RawJsonEditorView} from "playground-core";
import {replaceWith} from "ballerina-core";
import { JsonEditor, githubLightTheme } from 'json-edit-react'
import React from "react";

export const TmpJsonEditor: RawJsonEditorView = (props) => (
    <>
        <JsonEditor
            data={JSON.parse(props.context.inputString.value)}
            theme={githubLightTheme}
            onDelete={ x =>
            {   
                const value = JSON.stringify(x.newData)
                props.setState(RawJsonEditor.Updaters.Template.inputString(replaceWith({ value: value })));
            }}
            onEditEvent={ x => props.setState(RawJsonEditor.Updaters.Core.step(replaceWith(EditorStep.editing())))}
            onChange={x => x.newValue}
            onUpdate={x =>
            {   const value = JSON.stringify(x.newData)
                props.setState(RawJsonEditor.Updaters.Template.inputString(replaceWith({ value: value })));
            }}
        />
    </>
);