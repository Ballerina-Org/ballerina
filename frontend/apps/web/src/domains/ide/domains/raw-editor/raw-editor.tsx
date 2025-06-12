import {RawJsonEditor, RawJsonEditorView} from "playground-core";
import {replaceWith} from "ballerina-core";

export const RawEditorArea: RawJsonEditorView = (props) => (
    <>
        <p>Messages:</p>
        <>{props.context.messages.map( msg => <p>{msg}</p>)}</>
        
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
