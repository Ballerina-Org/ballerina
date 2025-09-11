import {
    simpleUpdater,
    Option,
    Value, replaceWith, Updater, CollectionReference, BasicUpdater, ParsedFormJSON, ValueOrErrors
} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { ForeignMutationsInput } from "ballerina-core";
import { Bridge, BridgeState } from "./domains/bridge/state"
import { JsonEditorForeignMutationsExpected, JsonEditorView } from "./domains/editor/state";
import {List} from "immutable";
import {getSpec} from "./api/specs";

export type V1 = ParsedFormJSON<any>

export type V2 = any;

export type FullSpec = {
    v1: ParsedFormJSON<any>,
    v2: any,
    seeds: any,
    config: any
}
export const FullSpec = {
    Default: (): FullSpec => ({
        v1: {} as  ParsedFormJSON<any>,
        v2: {},
        seeds: {},
        config: {}
    })
}
export type Bootstrap =
    | { kind: "kickingOff" }
    | { kind: "initializing", message: string }
    | { kind: "ready"}

export type LockedSpec = {
    bridge: BridgeState,
    launchers: string [],
    selectedLauncher: Option<Value<string>>,
}; 

export const LockedSpec = {
    Updaters: {
        Core: {
            Default: (spec: FullSpec) : LockedSpec => ({
                launchers: spec.v1.launchers ? Array.from(Object.keys(spec.v1.launchers)): [],
                selectedLauncher: Option.Default.none(),
                bridge: Bridge.Default(spec)
            })
        }
    }
}

export type CommonUI = {
    activeTab: 'existing' | 'new';
    existing: { specs: string[]; selected: Option<string> };
    create:   { name: Value<string> };
    bootstrappingError: Option<List<string>>,
    choosingError: Option<List<string>>,
    lockingError: Option<List<string>>,
};

const CommonUI = {
    Default: (): CommonUI => ({
        activeTab: 'existing',
        existing: { specs: [], selected : Option.Default.none() },
        create:   { name: Value.Default("") },
        bootstrappingError: Option.Default.none(),
        choosingError: Option.Default.none(),
        lockingError: Option.Default.none(),
    })
}

export type Ide =
    CommonUI & (
    | { phase: 'bootstrap', bootstrap: Bootstrap }
    | { phase: 'choose' }
    | { phase: 'locked'; source: 'existing' | 'new'; locked: LockedSpec }
    );

export function lock(
    s: Extract<Ide, { phase: 'choose' }>,
    buildLocked: (specName: string) => LockedSpec
): Ide {
    const fromExisting = s.activeTab === 'existing' &&  s.existing.selected.kind == "r";
    const specName = fromExisting
        ? s.existing.selected.value
        : s.create.name.value;

    return { ...s, phase: 'locked', source: fromExisting ? 'existing' : 'new', locked: buildLocked(<string>specName) };
}

export const Ide = {
    Default: (): Ide => {
        return ({ ...CommonUI.Default(), 
                    phase: 'bootstrap',
                    bootstrap: { kind: 'kickingOff' } })
    },
    Template: {
        bridge: {
            id: () : BasicUpdater<Ide>=> {
                return (ide: Ide): Ide =>
                    ide
            }
        }
    },
    Updaters: {
        
        bootstrap: {
            initializing(msg: string): BasicUpdater<Ide> {
                return (ide: Ide): Ide =>
                    ide.phase === 'bootstrap'
                        ? { ...ide, bootstrap: { kind: 'initializing', message: msg } }
                        : ide;
            },
            ready(specNames: string []): BasicUpdater<Ide> {                
                return (ide: Ide): Ide =>
                    ide.phase === 'bootstrap'
                        ? { ...ide, bootstrap: { kind: 'ready' }, existing: { specs: specNames, selected: Option.Default.none() } }
                        : ide;
            },
            error(txt: List<string>): BasicUpdater<Ide> {
                return (ide: Ide): Ide =>
                    ide.phase === 'bootstrap'
                        ? { ...ide, bootstrappingError: Option.Default.some(txt) } 
                        : ide;
            },
        },
        toChoose: (): BasicUpdater<Ide>  => (ide: Ide): Ide => ({...ide, phase: 'choose', activeTab: 'existing'}),
        chooseNew: (): BasicUpdater<Ide>  => (ide: Ide): Ide => ({...ide, phase: 'choose', activeTab: 'new'}),
        lock: (origin:  'existing' | 'new',  spec: FullSpec): BasicUpdater<Ide> => {
                return (ide: Ide): Ide =>
            ide.phase === 'choose'
                ? { ...ide, phase: 'locked', source: origin, locked: LockedSpec.Updaters.Core.Default(spec) }
                : { ...ide, lockingError: Option.Default.some(List([]))};
            },
        specName: (name: string): BasicUpdater<Ide> =>
            (ide: Ide): Ide => 
                ide.phase == "choose" && ide.activeTab == "new" ? 
                    ({ ...ide, create: { name: Value.Default(name) } })
                    : ({ ...ide })
                      
            //LockedSpec.Updaters.Core.Default(spec)
    },
    Operations: {
        toLockedSpec: async (origin:  'existing' | 'new', name: string): Promise<BasicUpdater<Ide>> => {
            const spec: ValueOrErrors<FullSpec, string> = 
                origin == "existing" ? await getSpec(name) : ValueOrErrors.Default.return(FullSpec.Default());

            return spec.kind == "value" ?
                Ide.Updaters.lock(origin, spec.value)
                : (ide: Ide): Ide => ({...ide, lockingError: Option.Default.some(spec.errors)})
            
        }
        // validateV1 : async (ide: Ide) => {
        //     const res = await validateV1(ide.specName.value, ide.bridge.spec.left.left);
        //
        //     return res.kind == "errors" ?
        //         ((x: Ide) => Ide.Updaters.Core.bridge(Bridge.Operations.errors(res.errors as unknown as string []))(x))
        //         :
        //         ((x:Ide) =>Ide.Updaters.Core.bridge(Bridge.Operations.setV())
        //             .then(Ide.Updaters.Core.launchers(replaceWith(res.value)))(x));
        //      
        //     }
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