import {simpleUpdater, Option, SmallIdentifiable, Vector2} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { ForeignMutationsInput } from "ballerina-core";
import {
    SpecEditor,
    RawJsonEditorForeignMutationsExpected,
    RawJsonEditorView
} from "./domains/spec-editor/state";

import {Layout} from "./domains/layout/state";
import {SpecRunner} from "./domains/spec-runner/state";


export type IDE = {
    editor: SpecEditor,
    runner: SpecRunner,
    layout: Layout
};

const CoreUpdaters = {
    ...simpleUpdater<IDE>()("editor"),
    ...simpleUpdater<IDE>()("layout"),
    ...simpleUpdater<IDE>()("runner"),
};

export const IDE = {
    Default: (_specs: any []): IDE => ({
        editor: SpecEditor.Default(Option.Default.none()),
        layout: Layout.Default(),
        runner: SpecRunner.Default(),
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