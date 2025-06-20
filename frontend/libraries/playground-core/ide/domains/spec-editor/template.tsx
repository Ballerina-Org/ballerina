import {Template} from "ballerina-core";
import {
    RawJsonEditorForeignMutationsExpected,
    RawJsonEditorReadonlyContext,
    RawJsonEditorView,
    RawJsonEditorWritableState,
} from "./state";
import { SpecPreviewDebouncer } from "./coroutines/runner";

export const RawJsonEditorTemplate = Template.Default<
    RawJsonEditorReadonlyContext,
    RawJsonEditorWritableState,
    RawJsonEditorForeignMutationsExpected,
    RawJsonEditorView
>((props) => <props.view {...props} />).any([
  SpecPreviewDebouncer
]);