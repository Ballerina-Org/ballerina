import { Debounced } from "ballerina-core";
import { RawJsonEditorForeignMutationsExpected } from "../state";
import { Co } from "./builder";
import { debouncedInputValidator, debouncedInputBackendValidator } from "./debounced-input-synchronizer";
import { validateSpecification } from "./backend-spec-validator";

export const RawJsonEditorDebouncerRunner =
    Co.Template<RawJsonEditorForeignMutationsExpected>( debouncedInputValidator, {
        runFilter: (props) =>
            Debounced.Operations.shouldCoroutineRun(props.context.inputString),
    });

export const RawJsonEditorDebouncerRunnerBackend =
    Co.Template<RawJsonEditorForeignMutationsExpected>( debouncedInputBackendValidator, {
        runFilter: (props) =>
            Debounced.Operations.shouldCoroutineRun(props.context.inputString),
    });

export const ValidateSpecification =
    Co.Template<RawJsonEditorForeignMutationsExpected>( validateSpecification, {
        // runFilter: (props) =>
        //     Debounced.Operations.shouldCoroutineRun(props.context.inputString),
    });