import {simpleUpdater, Option, replaceWith, BasicUpdater} from "ballerina-core";

import { View } from "ballerina-core";
import { Unit, Value, ForeignMutationsInput } from "ballerina-core";
import {EditorStep, IDE,} from "../../state";
import {IDEApi} from "../../apis/spec";

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

export type RawJsonEditor<T = unknown> = {
    inputString: Value<string>,
    validatedSpec: Option<string>,
    errors: Option<string>,
    step: EditorStep,
};

const CoreUpdaters = {
    ...simpleUpdater<RawJsonEditor>()("inputString"),
    ...simpleUpdater<RawJsonEditor>()("validatedSpec"),
    ...simpleUpdater<RawJsonEditor>()("errors"),
    ...simpleUpdater<RawJsonEditor>()("step"),
};

export const RawJsonEditor = {
    Default: <T>(json: Option<JsonValue<T>>): RawJsonEditor<T> => {
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
            inputString: Value.Default(inputString), 
            validatedSpec: Option.Default.none(),
            errors: Option.Default.none(),
            step: { kind: "editing" },
        }},
    Updaters: {
        Core: CoreUpdaters,
        Template: {
            inputString: CoreUpdaters.inputString,
        },
        Coroutine: {
        },
    },
    Operations: {
        tryParse: (input: Value<string>): Promise<any> => {
            return new Promise((resolve, reject) => {
                try {
                    resolve(JSON.parse(input.value))
                } catch (e: any) {
                    reject(e);
                }
            });
        },
    },
    ForeignMutations: (
        _: ForeignMutationsInput<RawJsonEditorReadonlyContext, RawJsonEditorWritableState>,
    ) => ({
    }),
};

export type RawJsonEditorReadonlyContext = Unit;
export type RawJsonEditorWritableState = RawJsonEditor;

export type RawJsonEditorForeignMutationsExpected = Unit

// export type RawJsonEditorForeignMutationsExposed = ReturnType<
//     typeof RawJsonEditor.ForeignMutations
// >;

export type RawJsonEditorView = View<
    RawJsonEditorReadonlyContext & RawJsonEditorWritableState,
    RawJsonEditorWritableState,
    RawJsonEditorForeignMutationsExpected,
    {
    }
>;