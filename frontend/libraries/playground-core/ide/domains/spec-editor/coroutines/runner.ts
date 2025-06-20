import { Debounced } from "ballerina-core";
import { Co } from "./builder";
import { specPreviewDebouncer } from "./spec-preview-debouncer";
import { RawJsonEditorForeignMutationsExpected } from "../state";

export const SpecPreviewDebouncer =
  Co.Template<RawJsonEditorForeignMutationsExpected>(specPreviewDebouncer, {
    runFilter: (props) =>
      Debounced.Operations.shouldCoroutineRun(props.context.input),
  });
