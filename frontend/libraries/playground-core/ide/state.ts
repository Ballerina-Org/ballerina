import {
    simpleUpdater,
    Option,
    SmallIdentifiable,
    Vector2,
    Debounced,
    Synchronized,
    Unit,
    Fun,
    Value, replaceWith, Updater
} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { ForeignMutationsInput } from "ballerina-core";
import {
    SpecEditor,
    RawJsonEditorForeignMutationsExpected,
    RawJsonEditorView, ValidationResultWithPayload, SpecEditorIndicator
} from "./domains/spec-editor/state";

import {Layout} from "./domains/layout/state";
import {SpecRunner} from "./domains/spec-runner/state";
import {IDEApi} from "./apis/spec";


export type IDE = {
    editor: SpecEditor,
    runner: SpecRunner,
    layout: Layout, 
    specNames: Synchronized<Unit, ValidationResultWithPayload<string[]>>,
    spec: Option<Value<string>>,
};

const CoreUpdaters = {
    ...simpleUpdater<IDE>()("editor"),
    ...simpleUpdater<IDE>()("layout"),
    ...simpleUpdater<IDE>()("runner"),
    ...simpleUpdater<IDE>()("specNames"),
    ...simpleUpdater<IDE>()("spec"),
};

export const IDE = {
    Default: (_specs: any []): IDE => ({
        editor: SpecEditor.Default(Option.Default.none()),
        layout: Layout.Default(),
        runner: SpecRunner.Default(),
        specNames: Synchronized.Default({}),
        spec: Option.Default.none()
    }),
    Updaters: {
        Core: CoreUpdaters,
        Template: {

            
            //   Fun(Value.Updaters.value<string>).then(
            //   Fun(Synchronized.Updaters.value<Value<string>, ValidationResultWithPayload<string>>).then(
            //     Fun(
            //       Debounced.Updaters.Template.value<
            //         Synchronized<Value<string>, ValidationResultWithPayload<string>>
            //       >,
            //     ).then(CoreUpdaters.input),
            //   ),
            // ),
        },
        Coroutine: {
        },
    },
    Operations: {
        changeSpec: (spec: string): Updater<IDE> =>


          CoreUpdaters.spec(
            replaceWith(Option.Default.some(Value.Default(spec)))
          )
            .then(CoreUpdaters.editor(
              SpecEditor.Updaters.Core.input(
                replaceWith(
                  Debounced.Default(
                    Synchronized.Default(Value.Default(spec)),
                  ),
                ),
              )))
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