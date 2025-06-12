import {Template} from "ballerina-core";
import {
    RawJsonEditorForeignMutationsExpected,
    RawJsonEditorReadonlyContext,
    RawJsonEditorView,
    RawJsonEditorWritableState,
} from "./state";

import {ValidateSpecification} from "./coroutines/runner";

export const RawJsonEditorTemplate = Template.Default<
    RawJsonEditorReadonlyContext,
    RawJsonEditorWritableState,
    RawJsonEditorForeignMutationsExpected,
    RawJsonEditorView
>((props) => <props.view {...props} />).any([
    //RawJsonEditorDebouncerRunner, //.mapContext((_) => ({ ..._, events: [] })),
    //RawJsonEditorDebouncerRunnerBackend, //.mapContext((_) => ({ ..._, events: [] })),
    ValidateSpecification,
]);