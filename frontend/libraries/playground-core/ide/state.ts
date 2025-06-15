import { simpleUpdater, Option} from "ballerina-core";
import { Template, View, Value } from "ballerina-core";
import { Debounced, Synchronized, ValidationResult, ForeignMutationsInput } from "ballerina-core";
import {
    RawJsonEditor,
    RawJsonEditorForeignMutationsExpected,
    JsonValue,
    RawJsonEditorView
} from "./domains/spec-editor/state";

export type EditorStep =
    | { kind : "editing" }
    | { kind : "validating" }
    | { kind : "parsing" }
    | { kind : "running" }
    | { kind : "output" };

export const EditorStep = {
    editing: (): EditorStep => ({ kind: "editing" }),
    validating: (): EditorStep => ({ kind: "validating" }),
    parsing: (): EditorStep => ({ kind: "parsing" }),
    running: (): EditorStep => ({ kind: "running" }),
    output: (): EditorStep => ({ kind: "output" }),
}

export type IDE = {
    rawEditor: RawJsonEditor,
    tabs: string [],
    shouldRun: boolean,
    availableSpecs: Debounced<Synchronized<Value<JsonValue []>, ValidationResult>>
};

const CoreUpdaters = {
    ...simpleUpdater<IDE>()("rawEditor"),
    ...simpleUpdater<IDE>()("availableSpecs"),
    ...simpleUpdater<IDE>()("tabs"),
    ...simpleUpdater<IDE>()("shouldRun"),
};

export const IDE = {
    Default: (specs: JsonValue []): IDE => ({
        rawEditor: RawJsonEditor.Default(Option.Default.none()),
        availableSpecs:Debounced.Default(Synchronized.Default(Value.Default([]))),
        shouldRun: false,
        tabs: [ "tab1", "tab2", "tab3" ],   

    }),
    Updaters: {
        Core: CoreUpdaters,
        Template: {
        },
        Coroutine: {
        },
    },
    Operations: {
        
    },
    ForeignMutations: (
        _: ForeignMutationsInput<IDEReadonlyContext, IDEWritableState>,
    ) => ({
    }),
};

export type IDEReadonlyContext = {};
export type IDEWritableState = IDE;

export type IDEForeignMutationsExpected = RawJsonEditorForeignMutationsExpected

// export type IDEForeignMutationsExposed = ReturnType<
//     typeof IDE.ForeignMutations
// >;

export type IDEView = View<
    IDEReadonlyContext & IDEWritableState,
    IDEWritableState,
    IDEForeignMutationsExpected,
    {
        RawJsonEditor: Template<
            IDEReadonlyContext & IDEWritableState,
            IDEWritableState,
            RawJsonEditorForeignMutationsExpected,
            RawJsonEditorView
        >;
    }
>;