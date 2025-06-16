import {
    ForeignMutationsInput,
    id,
    Option,
    replaceWith,
    simpleUpdater, Sum,
    Updater,
    Value,
    ValueOrErrors
} from "ballerina-core";
import {IDE, IDEReadonlyContext, IDEWritableState} from "../../state";
import {RawJsonEditorForeignMutationsExpected, SpecValidationResult} from "../spec-editor/state";
import {Co} from "../../coroutines/builder";
import {IDEApi} from "../../apis/spec";

export type SpecRunnerIndicator =
    | { kind : "idle" }
    | { kind : "validating" }
    | { kind : "running" }

export const SpecRunnerIndicator = {
    Default: {
        idle: (): SpecRunnerIndicator => ({ kind: "idle" }),
        validating: (): SpecRunnerIndicator => ({ kind: "validating" }),
        running: (): SpecRunnerIndicator => ({ kind: "running" }),
    }
}

export type SpecRunner = {
    validation: Option<ValueOrErrors<string, string>>
    lockedSpec: Option<string>,
    indicator: SpecRunnerIndicator,
};

const CoreUpdaters = {
    ...simpleUpdater<SpecRunner>()("validation"),
    ...simpleUpdater<SpecRunner>()("lockedSpec"),
    ...simpleUpdater<SpecRunner>()("indicator"),
}

export const SpecRunner = ({
    Default:(): SpecRunner => ({
        validation: Option.Default.none(), // ValueOrErrors.Default.return(""),
        lockedSpec: Option.Default.none(),
        indicator: { kind: "idle" },
    }),
    Updaters: {
        Core: CoreUpdaters,
    },
    Operations: {
    },
})

export type SpecRunnerReadonlyContext = {};
export type SpecRunnerWritableState = IDE;


