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

//TODO: split state further between spec-editor and spec-runner

export type IDE = {

    specNames: string[],
    
    specBody: Value<string>,
    specName: Value<string>,
    entityBody: Value<string>,
    entityName: Option<Value<string>>,
    launchers: string [],
    launcherName: Option<Value<string>>,
    entityNames: string [],
    
    //domains
    editor: SpecEditor,
    runner: SpecRunner,
    layout: Layout,
    step: IdeStep,
    loading: boolean,
};

export type IdeStep = 
    | { "kind": "start"}
    | { "kind": "entity" }
    | { "kind": "launcher" }
    | { "kind": "actions" }
    | { "kind": "deploy" }

export const IdeStep = {
    Default: {
        start: (): IdeStep => ({ kind: "start" }),
        launcher: (): IdeStep => ({ kind: "launcher" }),
        entity: (): IdeStep => ({ kind: "entity" }),
        actions: (): IdeStep => ({ kind: "actions" }),
        deploy: (): IdeStep => ({ kind: "deploy" }),
    }
}

const CoreUpdaters = {
    ...simpleUpdater<IDE>()("editor"),
    ...simpleUpdater<IDE>()("layout"),
    ...simpleUpdater<IDE>()("runner"),
    ...simpleUpdater<IDE>()("specNames"),
    ...simpleUpdater<IDE>()("specBody"),
    ...simpleUpdater<IDE>()("specName"),
    ...simpleUpdater<IDE>()("launchers"),
    ...simpleUpdater<IDE>()("entityBody"),
    ...simpleUpdater<IDE>()("entityNames"),
    ...simpleUpdater<IDE>()("entityName"),
    ...simpleUpdater<IDE>()("launcherName"),
    ...simpleUpdater<IDE>()("step"),
    ...simpleUpdater<IDE>()("loading"),
};

export const IDE = {
    Default: (): IDE => ({
        editor: SpecEditor.Default("New Spec"),
        layout: Layout.Default(),
        runner: SpecRunner.Default(),
        specNames: [],
        entityNames: [],
        step: IdeStep.Default.start(),
        specBody: { value: "{}" },
        specName: { value: "New Spec"},
        entityBody: { value: "{}" },
        entityName: Option.Default.none(),
        launchers: [],
        launcherName: Option.Default.none(),
        loading: false
    }),
    Updaters: {
        Core: CoreUpdaters,
        Template: {
        },
        Coroutine: {
        },
    },
    Operations: {
        changeSpec: (name: string, spec: string): Updater<IDE> =>
          CoreUpdaters.specBody(
            replaceWith(Value.Default(spec))
          ).then(CoreUpdaters.specName(
            replaceWith(Value.Default(name))
          ))
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