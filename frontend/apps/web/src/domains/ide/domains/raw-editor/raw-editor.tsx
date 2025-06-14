import {RawJsonEditor, RawJsonEditorView} from "playground-core";
import {replaceWith} from "ballerina-core";
import React from "react";

export const RawEditor: RawJsonEditorView = (props) => (
    <>
        <div className="editor-wrapper">
            <pre className="editor-highlighted" id="highlighted"></pre>
            <textarea
                spellCheck={false}
                className="editor-input"
                value={props.context.inputString.value}
                onChange={(e) => {
                    props.setState(RawJsonEditor.Updaters.Template.inputString(replaceWith({ value: e.target.value})))
                }}
                id="editor">
        </textarea>
        </div>
    </>
);