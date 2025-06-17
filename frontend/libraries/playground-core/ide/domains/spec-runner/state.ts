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
    | { kind : "locked" }
    | { kind : "running" }

export const SpecRunnerIndicator = {
    Default: {
        idle: (): SpecRunnerIndicator => ({ kind: "idle" }),
        validating: (): SpecRunnerIndicator => ({ kind: "validating" }),
        running: (): SpecRunnerIndicator => ({ kind: "running" }),
        locked: (): SpecRunnerIndicator => ({ kind: "locked" }),
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
        validation: Option.Default.none(), 
        lockedSpec: Option.Default.none(),
        indicator: SpecRunnerIndicator.Default.idle(),
    }),
    Updaters: {
        Core: CoreUpdaters,
    },
    Operations: {
        runEditor: (spec: string, res: SpecValidationResult): Updater<IDE> =>
            IDE.Updaters.Core.runner(
                SpecRunner.Updaters.Core.indicator(
                    replaceWith(SpecRunnerIndicator.Default.running())
                )
                .then(
                    SpecRunner.Updaters.Core.lockedSpec(
                        replaceWith(res.isValid ? Option.Default.some(spec) : Option.Default.none())
                    )
                    .then(SpecRunner.Updaters.Core.validation(
                        replaceWith(
                            res.isValid ?
                                Option.Default.some(ValueOrErrors.Default.return(spec)):
                                Option.Default.none()
                        )
                    ))
                )
            )
    },
})

export type SpecRunnerReadonlyContext = {};
export type SpecRunnerWritableState = IDE;
