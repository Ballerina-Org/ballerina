import {simpleUpdater, Option, replaceWith, BasicUpdater} from "ballerina-core";

import { View } from "ballerina-core";
import { Unit, Value, ForeignMutationsInput } from "ballerina-core";
import {IDE,} from "../../state";
import {IDEApi} from "../../apis/spec";

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

export type JsonValue<T = unknown> = 
    | { kind: "unparsed"; raw: string }
    | { kind: "parsed"; value: T }
    | { kind: "unknown"; value: any };

export const JsonValue = {
    Default: {
        unparsed: (raw: string): JsonValue => ({ kind: "unparsed", raw }),
        parsed: <T>(value: T): JsonValue<T> => ({ kind: "parsed", value }),
        unknown: (value: any): JsonValue => ({ kind: "unknown", value }),
    }
}

export type SpecEditor<T = unknown> = {
    input: Value<string>, 
    indicator: SpecEditorIndicator,
};

const CoreUpdaters = {
    ...simpleUpdater<SpecEditor>()("input"),
    ...simpleUpdater<SpecEditor>()("indicator"),
};

export const SpecEditor = {
    Default: <T>(json: Option<JsonValue<T>>): SpecEditor<T> => {
        let inputString = `{}`;

        switch (json.kind) {
            case "l": break;
            case "r":
                switch (json.value.kind) {
                    case "unparsed":
                        inputString = json.value.raw;
                        break;

                    case "parsed":
                        inputString = JSON.stringify(json.value.value);
                        break;

                    case "unknown":
                        inputString = json.value.value;
                        break;
                }
                break;
        }

        return {
            input: Value.Default(inputString), 
            indicator: SpecEditorIndicator.Default.idle()
        }},
    Updaters: {
        Core: CoreUpdaters,
        Template: {
            inputString: CoreUpdaters.input,
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