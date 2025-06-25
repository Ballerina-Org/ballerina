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
    | { kind : "editor-dirty" }
    | { kind : "ready-for-UI" }

export const SpecRunnerIndicator = {
    Default: {
        idle: (): SpecRunnerIndicator => ({ kind: "idle" }),
        validating: (): SpecRunnerIndicator => ({ kind: "validating" }),
        running: (): SpecRunnerIndicator => ({ kind: "running" }),
        locked: (): SpecRunnerIndicator => ({ kind: "locked" }),
        readyForUI: (): SpecRunnerIndicator => ({ kind: "ready-for-UI" }),
        editorDirty: (): SpecRunnerIndicator => ({ kind: "editor-dirty" }),
    }
}

export type SpecRunner = {
    validation: Option<ValueOrErrors<string, string>>
    lockedSpec: Option<string>,
    indicator: SpecRunnerIndicator,
    launchers: string [],
};

const CoreUpdaters = {
    ...simpleUpdater<SpecRunner>()("validation"),
    ...simpleUpdater<SpecRunner>()("lockedSpec"),
    ...simpleUpdater<SpecRunner>()("indicator"),
    ...simpleUpdater<SpecRunner>()("launchers"),
}

export const SpecRunner = ({
    Default:(): SpecRunner => ({
        validation: Option.Default.none(), 
        lockedSpec: Option.Default.none(),
        indicator: SpecRunnerIndicator.Default.idle(),
        launchers: [],
    }),
    Updaters: {
        Core: CoreUpdaters,
    },
    Operations: {
        updateStep: (step: SpecRunnerIndicator): Updater<SpecRunner>  => 
          
        SpecRunner.Updaters.Core.indicator(
            replaceWith(step)
        ),
        runEditor: (spec: string, res: SpecValidationResult): Updater<SpecRunner> =>
    
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
                    )).then(SpecRunner.Updaters.Core.indicator(
                      replaceWith(SpecRunnerIndicator.Default.readyForUI())
                    ))
                )
            
    },
})

export type SpecRunnerReadonlyContext = {};
export type SpecRunnerWritableState = IDE;
