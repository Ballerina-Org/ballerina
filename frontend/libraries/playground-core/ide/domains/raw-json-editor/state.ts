import {replaceWith, Updater, simpleUpdater, BasicFun, BasicUpdater, Fun, Option} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { Unit, Debounced, Value, Synchronized, ValidationResult, ForeignMutationsInput } from "ballerina-core";

export type JsonParseState<T = unknown> =
    | { kind: "unparsed"; raw: string }
    | { kind: "parsed"; value: T }
    | { kind: "unknown"; value: any };

type JsonParseStatus =
    | { kind: "Ok" }
    | { kind: "Error"; errors: string[] }

type JsonValidationStatus =
    | { kind: "Valid" }
    | { kind: "Invalid"; errors: string[] }

type SpecDocumentLoaded = { kind: "SpecLoaded" }
type SpecDocumentDirty = { kind: "SpecLoaded"; reason?: string }

type DocumentLifecycle =
    | { kind : "EditorEmpty" }
    | SpecDocumentLoaded
    | SpecDocumentDirty

type DocumentStatus = {
    lifecycle: DocumentLifecycle
    lastModified: Option<Date>
    jsonParse: JsonParseStatus
    jsonValidation: JsonValidationStatus
}

export type ParsingError = { success: true; value: any } | { success: false; error: string }

export type RawJsonEditor<T = unknown> = {
    inputString: Debounced<Synchronized<Value<string>, ParsingError>>,
    inputJSON: Debounced<Synchronized<Value<JsonParseState<T>>, ParsingError>>,
    //status: DocumentStatus
};

const CoreUpdaters = {
    ...simpleUpdater<RawJsonEditor>()("inputString"),
};
const DocumentStatus = {
    Default: (): DocumentStatus => ({
        lifecycle: { kind: "EditorEmpty" },
        lastModified: Option.Default.none(),
        jsonParse: { kind: "Ok" },
        jsonValidation: { kind: "Valid" },
    }),
    Updaters: {
        Core: {
            ...simpleUpdater<DocumentStatus>()("lifecycle"),
            ...simpleUpdater<DocumentStatus>()("lastModified"),
            ...simpleUpdater<DocumentStatus>()("jsonParse"),
            ...simpleUpdater<DocumentStatus>()("jsonValidation"),
        },
    }
}

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
            //status: DocumentStatus.Default(),
        }},
    Updaters: {
        Core: CoreUpdaters,
        Template: {
            inputString: Fun(Value.Updaters.value<string>).then(
                Fun(Synchronized.Updaters.value<Value<string>, ParsingError>).then(
                    Fun(
                        Debounced.Updaters.Template.value<
                            Synchronized<Value<string>, ParsingError>
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
        tryParseJsonAsPromise: (input: Value<string>): Promise<{ success: true; value: any } | { success: false; error: string }> => {
            
                return new Promise((resolve, reject) => {
                    const result = RawJsonEditor.Operations.tryParseJson(input);
                    result.success ? resolve(result.value) : reject(result.error)
                });
        }
    },
    ForeignMutations: (
        _: ForeignMutationsInput<RawJsonEditorReadonlyContext, RawJsonEditorWritableState>,
    ) => ({
        loadSpec: (spec: string) => 
            //todo: check if current spec is saved
            _.setState(RawJsonEditor.Updaters.Template.inputString(replaceWith(spec)))
    }),
};

export type RawJsonEditorReadonlyContext = Unit;
export type RawJsonEditorWritableState = RawJsonEditor;

export type RawJsonEditorViewProps = {
    context: RawJsonEditorReadonlyContext & RawJsonEditorWritableState;
    setState: BasicFun<BasicUpdater<RawJsonEditor>, void>;
    foreignMutations: RawJsonEditorForeignMutationsExpected;
};

export type RawJsonEditorForeignMutationsExpected = Unit

export type RawJsonEditorForeignMutationsExposed = ReturnType<
    typeof RawJsonEditor.ForeignMutations
>;

export type RawJsonEditorView = View<
    RawJsonEditorReadonlyContext,
    RawJsonEditorWritableState,
    RawJsonEditorForeignMutationsExpected,
    {
    }
>;