import { replaceWith, Updater, simpleUpdater, BasicFun, BasicUpdater, Fun } from "ballerina-core";
import { Template, View, Value } from "ballerina-core";
import { Unit, Debounced, Synchronized, ValidationResult, ForeignMutationsInput } from "ballerina-core";
import {
    RawJsonEditor,
    RawJsonEditorForeignMutationsExpected,
    JsonParseState,
    RawJsonEditorView
} from "./domains/raw-json-editor/state";
import {Child1ForeignMutationsExpected, Child1View} from "../parent/domains/child1/state";
import {ParentReadonlyContext, ParentWritableState} from "../parent/state";

export type IDE = {
    rawEditor: RawJsonEditor,
    tabs: string [],
    availableSpecs: Debounced<Synchronized<Value<JsonParseState []>, ValidationResult>>,
    //status: DocumentStatus
};

const CoreUpdaters = {
    ...simpleUpdater<IDE>()("rawEditor"),
    ...simpleUpdater<IDE>()("availableSpecs"),
    ...simpleUpdater<IDE>()("tabs"),
};

export const IDE = {
    Default: (specs: JsonParseState []): IDE => ({
        rawEditor: RawJsonEditor.Default(specs[0] ?? { kind: "unparsed", raw: `{ name: "Papi"}`}),
        availableSpecs:Debounced.Default(Synchronized.Default(Value.Default(specs))),
        tabs: ["Editor", "Specs", "Runner"],
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

export type IDEForeignMutationsExposed = ReturnType<
    typeof IDE.ForeignMutations
>;

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