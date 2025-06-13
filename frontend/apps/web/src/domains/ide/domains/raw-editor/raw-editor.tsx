import {RawJsonEditor, RawJsonEditorView} from "playground-core";
import {replaceWith, AsyncState} from "ballerina-core";
import { Play, AlertTriangle, CheckCircle, Server, User, Loader } from "lucide-react";
export const RawEditorArea: RawJsonEditorView = (props) => (
    <>
        <div>
            {(() => {
                switch(props.context.specName.sync.kind) {
                    case "loading":
                        return <div>
                            <Loader size={10} />
                            <span>Backend validation</span>
                        </div>;
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
                spellCheck={false}
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