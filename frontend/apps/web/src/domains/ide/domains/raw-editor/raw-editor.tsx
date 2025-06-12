import {RawJsonEditor, RawJsonEditorView} from "playground-core";
import {replaceWith, AsyncState} from "ballerina-core";

export const RawEditorArea: RawJsonEditorView = (props) => (
    <>
        <p>Messages:</p>
        <>{props.context.messages.map( msg => <p>{msg}</p>)}</>
        <p>Backend Spec validation in progress {AsyncState.Operations.isLoading(props.context.specName.sync)}:</p>
        <div>
            {(() => {
                switch(props.context.specName.sync.kind) {
                    case "loading":
                        return <p>Validating backend specification...</p>;
                    case "error":
                        return <p>Validation failed!: </p>;
                    case "loaded":
                        return <p>All good! value: {props.context.specName.sync.value.toString()}</p>;
                    default:
                        return null;
                }
            })()}
        </div>
   
        <div className="editor-wrapper">
            <pre className="editor-highlighted" id="highlighted"></pre>
            <textarea
                className="editor-input"
                value={props.context.inputString.value}
                onChange={(e) => {
                    console.log("raw-editor.tsx: RawEditorArea: onChange");
                    props.setState(RawJsonEditor.Updaters.Template.inputString(replaceWith(e.target.value)))
                }}
                id="editor">
        </textarea>
        </div>
    </>
);