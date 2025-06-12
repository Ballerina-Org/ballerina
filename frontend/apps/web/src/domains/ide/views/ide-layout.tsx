import {IDEView} from "playground-core";
import {RawEditorArea} from "../domains/raw-editor/raw-editor";

export const IDELayout: IDEView = (props) => (
    <>
        <h2>IDE layout</h2>
        <props.RawJsonEditor{...props} view={RawEditorArea} />
    </>
);