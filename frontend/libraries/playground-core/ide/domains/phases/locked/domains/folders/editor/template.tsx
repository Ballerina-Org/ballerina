import {Template} from "ballerina-core";
import {
    JsonEditorForeignMutationsExpected,
    JsonEditorView,
    JsonEditorWritableState,
} from "./state";

export const JsonEditorTemplate = Template.Default<
    JsonEditorWritableState,
    JsonEditorWritableState,
    JsonEditorForeignMutationsExpected,
    JsonEditorView
>((props) =>
    <props.view
        {...props}
    />).any([]);