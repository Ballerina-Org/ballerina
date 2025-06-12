import {Template} from "ballerina-core";
import {
    RawJsonEditorForeignMutationsExpected,
    RawJsonEditorReadonlyContext,
    RawJsonEditorView,
    RawJsonEditorWritableState,
    RawJsonEditor
} from "./state";

import {RawJsonEditorDebouncerRunner, RawJsonEditorDebouncerRunnerBackend} from "./coroutines/runner";

export const RawJsonEditorTemplate = Template.Default<
    RawJsonEditorReadonlyContext,
    RawJsonEditorWritableState,
    RawJsonEditorForeignMutationsExpected,
    RawJsonEditorView
>((props) => <props.view {...props} />).any([
    RawJsonEditorDebouncerRunner, //.mapContext((_) => ({ ..._, events: [] })),
    RawJsonEditorDebouncerRunnerBackend, //.mapContext((_) => ({ ..._, events: [] })),
]);