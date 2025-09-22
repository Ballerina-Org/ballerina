import {Template} from "ballerina-core";
import {
    JsonEditorForeignMutationsExpected,
    JsonEditorReadonlyContext,
    JsonEditorView,
    JsonEditorWritableState,
} from "./state";

export const JsonEditorTemplate = Template.Default<
    JsonEditorReadonlyContext,
    JsonEditorWritableState,
    JsonEditorForeignMutationsExpected,
    JsonEditorView
>((props) => <props.view {...props} />).any([
]);