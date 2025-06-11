import {IDE, IDEForeignMutationsExposed} from "playground-core";
import {SimpleCallback} from "ballerina-core";

export const RawEditorArea = (props: {
    inputText: string,
    onChangeInputText: SimpleCallback<string>
}) => (
    <div className="editor-wrapper">
        <pre className="editor-highlighted" id="highlighted"></pre>
        <textarea
            className="editor-input"
            value={props.inputText}
            onChange={(e) => props.onChangeInputText(e.target.value)}
            id="editor">
        </textarea>
    </div>
);
