import {
    simpleUpdater,
    Option,
    replaceWith,
    BasicUpdater,
    Debounced,
    Synchronized,
    ValidationResult, Updater, Fun
} from "ballerina-core";

import { View } from "ballerina-core";
import { Unit, Value, ForeignMutationsInput } from "ballerina-core";
import {IDE,} from "../../state";
import {IDEApi} from "../../apis/spec";

export type ValidationResultWithPayload<T> =
  Extract<ValidationResult, "valid"> & { payload: T };

export type SpecEditorIndicator =
    | { kind : "idle" }
    | { kind : "editing" }
    | { kind : "locked" };

export const SpecEditorIndicator = {
    Default: {
        idle: (): SpecEditorIndicator => ({ kind: "idle" }),
        editing: (): SpecEditorIndicator => ({ kind: "editing" }),
        locked: (): SpecEditorIndicator => ({ kind: "locked" }),
    }
}
export type SpecValidationResult = { isValid: boolean; errors: string }

export type SpecEditor = {
    input: Debounced<Synchronized<Value<string>, ValidationResultWithPayload<string>>>, 
    indicator: SpecEditorIndicator,
    name: Value<string>,
};

const CoreUpdaters = {
    ...simpleUpdater<SpecEditor>()("input"),
    ...simpleUpdater<SpecEditor>()("indicator"),
    ...simpleUpdater<SpecEditor>()("name"),
};

export const SpecEditor = {
    Default: (json: Option<any>): SpecEditor => {
        let inputString = `{}`;

        switch (json.kind) {
            case "l": break;
            case "r":
                inputString = JSON.stringify(json.value.value);
                break;
        }

        return {
            input: Debounced.Default(Synchronized.Default(Value.Default(inputString))), 
            indicator: SpecEditorIndicator.Default.idle(),
            name: Value.Default("Spec Name"),
        }},
    Updaters: {
        Core: CoreUpdaters,
        Template: {
            inputString:  Fun(Value.Updaters.value<string>).then(
              Fun(Synchronized.Updaters.value<Value<string>, ValidationResultWithPayload<string>>).then(
                Fun(
                  Debounced.Updaters.Template.value<
                    Synchronized<Value<string>, ValidationResultWithPayload<string>>
                  >,
                ).then(CoreUpdaters.input),
              ),
            ),
            //inputString: CoreUpdaters.input,
            name: CoreUpdaters.name,
        },
        Coroutine: {
        },
    },
    Operations: {
    },
    ForeignMutations: (
        _: ForeignMutationsInput<RawJsonEditorReadonlyContext, RawJsonEditorWritableState>,
    ) => ({
    }),
};

export type RawJsonEditorReadonlyContext = Unit;
export type RawJsonEditorWritableState = SpecEditor;

export type RawJsonEditorForeignMutationsExpected = Unit

export type RawJsonEditorView = View<
    RawJsonEditorReadonlyContext & RawJsonEditorWritableState,
    RawJsonEditorWritableState,
    RawJsonEditorForeignMutationsExpected,
    {
    }
>;