import {
    replaceWith,
    simpleUpdater,
    BasicFun,
    BasicUpdater,
    Fun,
    Option,
    FormParsingResult,
    AsyncState
} from "ballerina-core";

import { View } from "ballerina-core";
import { Unit, Debounced, Value, Synchronized, ForeignMutationsInput } from "ballerina-core";

export type JsonParseState<T = unknown> =
    | { kind: "unparsed"; raw: string }
    | { kind: "parsed"; value: T }
    | { kind: "unknown"; value: any };

//
// type JsonParseStatus =
//     | { kind: "Ok" }
//     | { kind: "Error"; errors: string[] }
//
// type JsonValidationStatus =
//     | { kind: "Valid" }
//     | { kind: "Invalid"; errors: string[] }
//
// type SpecDocumentLoaded = { kind: "SpecLoaded" }
// type SpecDocumentDirty = { kind: "SpecLoaded"; reason?: string }
//
// type DocumentLifecycle =
//     | { kind : "EditorEmpty" }
//     | SpecDocumentLoaded
//     | SpecDocumentDirty
//
// type DocumentStatus = {
//     lifecycle: DocumentLifecycle
//     lastModified: Option<Date>
//     jsonParse: JsonParseStatus
//     jsonValidation: JsonValidationStatus
// }

export type ParsingError = { success: true; value: any } | { success: false; error: string }

export type RawJsonEditor<T = unknown> = {
    inputString: Debounced<Synchronized<Value<string>, boolean>>,
    inputJSON: Debounced<Synchronized<Value<JsonParseState<T>>, boolean>>,
    specName: Synchronized<Value<string>, boolean>,
    messages: string []
    //status: DocumentStatus
};

const CoreUpdaters = {
    ...simpleUpdater<RawJsonEditor>()("inputString"),
    ...simpleUpdater<RawJsonEditor>()("messages"),
    ...simpleUpdater<RawJsonEditor>()("specName"),

};

export const RawJsonEditor = {
    Default: <T>(json: JsonParseState<T>): RawJsonEditor<T> => {
        let inputString: string;
            switch (json.kind) {
                case "unparsed":
                    inputString = json.raw;
                    break;
    
                case "parsed":
                    inputString = JSON.stringify(json.value);
                    break;
    
                case "unknown":
                    inputString = json.value;
                    break;
            }
        return {
            inputString: Debounced.Default(Synchronized.Default(Value.Default(inputString))),
            inputJSON: Debounced.Default(Synchronized.Default(Value.Default(json))),
            specName: Synchronized.Default (Value.Default("defaultSpecName")),

            messages: [],
            //status: DocumentStatus.Default(),
        }},
    Updaters: {
        Core: CoreUpdaters,
        Template: {
            inputString: Fun(Value.Updaters.value<string>).then(
                Fun(Synchronized.Updaters.value<Value<string>, boolean>).then(
                    Fun(
                        Debounced.Updaters.Template.value<
                            Synchronized<Value<string>, boolean>
                        >,
                    ).then(CoreUpdaters.inputString),
                ),
            )
        },
        Coroutine: {
        },
    },
    Operations: {
        tryParseJson: (input: Value<string>): { success: true; value: any } | { success: false; error: string } => {
            try {
                return { success: true, value: JSON.parse(input.value) };
            } catch (e: any) {
                return {
                    success: false,
                    error: e && e.message
                        ? `Wrong JSON: ${e.message}`
                        : "Unknown JSON error."
                };
            }
        },
        tryParseJsonAsPromise: (input: Value<string>): Promise<boolean> => {
                console.log("frontend validation")
                return new Promise((resolve, reject) => {
                    const result = RawJsonEditor.Operations.tryParseJson(input);
                    switch(result.success) {
                        case true:
                            resolve(result.value);
                            break;
                        case false:
                            reject(result.error);
                            break
                    }
                });
        }
    },
    ForeignMutations: (
        _: ForeignMutationsInput<RawJsonEditorReadonlyContext, RawJsonEditorWritableState>,
    ) => ({
    }),
};

export type RawJsonEditorReadonlyContext = Unit;
export type RawJsonEditorWritableState = RawJsonEditor;

export type RawJsonEditorForeignMutationsExpected = Unit

export type RawJsonEditorForeignMutationsExposed = ReturnType<
    typeof RawJsonEditor.ForeignMutations
>;

export type RawJsonEditorView = View<
    RawJsonEditorReadonlyContext & RawJsonEditorWritableState,
    RawJsonEditorWritableState,
    RawJsonEditorForeignMutationsExpected,
    {
    }
>;