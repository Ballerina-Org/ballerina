import {IDEView, Parent, RawJsonEditor} from "playground-core";
import { IDE  } from "playground-core";
import {replaceWith, Updaters } from "ballerina-core";
import {RawEditorArea} from "./raw-editor.tsx";

export const IDELayout: IDEView = (props) => (
    <>
        <h1>Ballerina IDE</h1>
        <RawEditorArea 
            onChangeInputText = 
                {(_) => 
                    props.setState(
                        IDE.Updaters.Core.rawEditor(RawJsonEditor.Updaters.Template.inputString(replaceWith(_))))}
            inputText = {props.context.rawEditor.inputString.value}
        />
    </>
);

