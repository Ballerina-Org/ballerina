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

import {Bridge, BridgeState, SpecV1} from "./domains/bridge/state"

import {V1, V2} from "./api/specs";
import {JsonEditorForeignMutationsExpected, JsonEditorView} from "./domains/editor/state";
import {validateV1} from "./api/bridge";

export type Ide = {
    bridge: BridgeState,
    specNames: string[],
    specName: Value<string>,
    launchers: string [],
    liveUpdates: Option<number>,
    launcherName: Option<Value<string>>,
};

const CoreUpdaters = {
    ...simpleUpdater<Ide>()("specNames"),
    ...simpleUpdater<Ide>()("specName"),
    ...simpleUpdater<Ide>()("launchers"),
    ...simpleUpdater<Ide>()("launcherName"),
    ...simpleUpdater<Ide>()("bridge"),
    ...simpleUpdater<Ide>()("liveUpdates"),
};

const Updaters = {
}

export const Ide = {
    Default: (): Ide => ({
        bridge: Bridge.Default(),
        specNames: [],
        specName: { value: "Provide spec name"},
        launchers: [],
        liveUpdates: Option.Default.none(),
        launcherName: Option.Default.none(),
    }),
    Updaters: {
        Core: CoreUpdaters,
        Custom: Updaters,
    },
    Operations: {
        validateV1 : async (ide: Ide) => {
            const res = await validateV1(ide.specName.value, ide.bridge.bridge.left.left);
            return res.kind == "errors" ?
                ((x: Ide) => Ide.Updaters.Core.bridge(Bridge.Operations.errors(res.errors as unknown as string []))(x))
                
                :


                ((x:Ide) =>Ide.Updaters.Core.bridge(Bridge.Operations.setV())
                    .then(Ide.Updaters.Core.launchers(replaceWith(res.value)))(x));
              
            }
        
        // changeSpec: (name: string, spec: string): Updater<Ide> =>{
        //     const m = Bridge.Operations.load(spec);
        //     return CoreUpdaters.specName(
        //         replaceWith(Value.Default(name))
        //     ).then(CoreUpdaters.bridge(replaceWith(m)))},
       
    },
    ForeignMutations: (
        _: ForeignMutationsInput<IdeReadonlyContext, IdeWritableState>,
    ) => ({
    }),
};

export type IdeReadonlyContext = {};
export type IdeWritableState = Ide;

export type IdeForeignMutationsExpected = JsonEditorForeignMutationsExpected

export type IdeView = View<
    IdeReadonlyContext & IdeWritableState,
    IdeWritableState,
    IdeForeignMutationsExpected,
    {
        JsonEditor: Template<
            IdeReadonlyContext & IdeWritableState,
            IdeWritableState,
            JsonEditorForeignMutationsExpected,
            JsonEditorView
        >;
    }
>;