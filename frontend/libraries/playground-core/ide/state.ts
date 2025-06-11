import { replaceWith, Updater, simpleUpdater, BasicFun, BasicUpdater, Fun } from "ballerina-core";
import { Template, View, Value } from "ballerina-core";
import { Unit, Debounced, Synchronized, ValidationResult, ForeignMutationsInput } from "ballerina-core";
import {RawJsonEditor, RawJsonEditorForeignMutationsExpected, JsonParseState } from "./domains/raw-json-editor/state";

export type IDE = {
    rawEditor: RawJsonEditor,
    availableSpecs: Debounced<Synchronized<Value<JsonParseState []>, ValidationResult>>,
    //status: DocumentStatus
};

const CoreUpdaters = {
    ...simpleUpdater<IDE>()("rawEditor"),
    ...simpleUpdater<IDE>()("availableSpecs"),
};

export const IDE = {
    Default: (specs: JsonParseState []): IDE => ({
        rawEditor: RawJsonEditor.Default(specs[0] ?? { kind: "unparsed", raw: `{ name: "Papi"}`}),
        availableSpecs:Debounced.Default(Synchronized.Default(Value.Default(specs)))
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

export type IDEReadonlyContext = IDE;
export type IDEWritableState = IDE;

// export type _IDEViewProps = {
//     context: IDEReadonlyContext;
//     setState: BasicFun<BasicUpdater<IDE>, void>;
//     foreignMutations: IDEForeignMutationsExpected;
// };

export type IDEForeignMutationsExpected = RawJsonEditorForeignMutationsExpected

export type IDEForeignMutationsExposed = ReturnType<
    typeof IDE.ForeignMutations
>;

export type IDEView = View<
    IDEReadonlyContext,
    IDEWritableState,
    IDEForeignMutationsExpected,
    {
    }
>;