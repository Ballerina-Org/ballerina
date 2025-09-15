import {
    simpleUpdater,
    Option,
    Value, replaceWith, Updater, CollectionReference, BasicUpdater, ParsedFormJSON, ValueOrErrors, TypeName, ParsedType,
    Product
} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { ForeignMutationsInput } from "ballerina-core";
import { Bridge, BridgeState } from "./domains/bridge/state"
import { JsonEditorForeignMutationsExpected, JsonEditorView } from "./domains/editor/state";
import {List} from "immutable";
import {getSpec} from "./api/specs";
import {VfsWorkspace, VirtualFolders} from "./domains/vfs/state";
import {getVirtualFolders} from "./api/vfs";

export type V1 = ParsedFormJSON<any>
export type V2 = any;

export type FullSpec = {
    v1: ParsedFormJSON<any>,
    v2: any,
    seeds: any,
    config: any
}

export type VSpec = {
    v1: ParsedFormJSON<any>,
    v2: any
}
export const FullSpec = {
    Default: (): FullSpec => ({
        v1: {} as ParsedFormJSON<any>,
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
    virtualFolders: VfsWorkspace
}; 

export const LockedSpec = {
    Updaters: {
        Core: {
            Default: (spec: FullSpec, workspace: VfsWorkspace): LockedSpec => ({
                launchers: spec.v1.launchers ? Array.from(Object.keys(spec.v1.launchers)): [],
                selectedLauncher: Option.Default.none(),
                bridge: Bridge.Default(spec),
                virtualFolders: workspace
            })
        }
    }
}

export type CommonUI = {
    activeTab: 'existing' | 'new';
    existing: { specs: string[]; selected: Option<string> };
    create: { name: Value<string> };
    
    bootstrappingError: Option<List<string>>,
    choosingError: Option<List<string>>,
    lockingError: Option<List<string>>,
};

const CommonUI = {
    Default: (): CommonUI => ({
        activeTab: 'new',
        existing: { specs: [], selected : Option.Default.none() },
        create:   { name: Value.Default("") },
        bootstrappingError: Option.Default.none(),
        choosingError: Option.Default.none(),
        lockingError: Option.Default.none(),
    })
}
type LockedStep =
    | { step: "design" }
    | { step: "outcome" };

export type Ide =
    CommonUI & (
    | { phase: 'bootstrap', bootstrap: Bootstrap }
    | { phase: 'choose' }
    | ({ phase: 'locked'; source: 'existing' | 'new'; locked: LockedSpec } & LockedStep)
    );

export const Ide = {
    Default: (): Ide => 
        ({ ...CommonUI.Default(), 
            phase: 'bootstrap',
            bootstrap: { kind: 'kickingOff' }
        }),
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
                        ? { ...ide, activeTab: specNames.length > 0 ? 'existing' : 'new', bootstrap: { kind: 'ready' }, existing: { specs: specNames, selected: Option.Default.none() } }
                        : ide;
            },
            error(txt: List<string>): BasicUpdater<Ide> {
                return (ide: Ide): Ide =>
                    ide.phase === 'bootstrap'
                        ? { ...ide, bootstrappingError: Option.Default.some(txt) } 
                        : ide;
            },
        },
        lockedSpec: {
          seed: (seeds: any): BasicUpdater<Ide>  => (ide: Ide): Ide =>
              ide.phase !== "locked" ? ide : ({...ide, locked: {...ide.locked, bridge: {...ide.locked.bridge, seeds: seeds}}}), 
          selectLauncher: (name: string): BasicUpdater<Ide> => 
            (ide: Ide): Ide =>
                ide.phase == 'locked'
                    ? ({...ide, locked: {...ide.locked, selectedLauncher: Option.Default.some(Value.Default(name))}})
                    : ({...ide}),
          bridge: {
              v1: (value: string): BasicUpdater<Ide> => 
                  (ide: Ide): Ide =>
                      ide.phase == 'locked'
                          ? ({...ide, locked: {...ide.locked, bridge: Bridge.Updaters.Template.setV1Body(Value.Default(value))(ide.locked.bridge)}})
                          : ({...ide}),
          },
            vfs: {
              selectedFolder: (vfs: Updater<VfsWorkspace>): BasicUpdater<Ide> => (ide: Ide): Ide =>
                  ide.phase == 'locked'
                      ? ({...ide, locked: {...ide.locked, virtualFolders: vfs(ide.locked.virtualFolders) }})
                      : ({...ide}),
            },
            
                
        },
        toChoose: (): BasicUpdater<Ide>  => (ide: Ide): Ide => ({...ide, phase: 'choose'}),
        chooseNew: (): BasicUpdater<Ide>  => (ide: Ide): Ide => ({...ide, phase: 'choose', activeTab: 'new'}),
        lock: (origin:  'existing' | 'new', name: string,  spec: FullSpec, workspace: VfsWorkspace): BasicUpdater<Ide> => {
                return (ide: Ide): Ide =>
            ide.phase === 'choose'
                ? { ...ide, phase: 'locked', step: 'design', create : { name: Value.Default(name)}, source: origin, locked: LockedSpec.Updaters.Core.Default( spec, workspace) }
                : { ...ide, lockingError: Option.Default.some(List([]))};
            },
        runForms: () : BasicUpdater<Ide> => {
            return (ide: Ide): Ide =>
                ide.phase === 'locked'
                    ? {
                        ...ide,
                        phase: 'locked',
                        step: 'outcome'
                    }
                    : ide;
        },
        specName: (name: string): BasicUpdater<Ide> =>
            (ide: Ide): Ide => 
                ide.phase == "choose" && ide.activeTab == "new" ? 
                    ({ ...ide, create: { name: Value.Default(name) } })
                    : ({ ...ide })
    },
    Operations: {
        loadWorkspace: async (origin: 'existing' | 'new', name: string): Promise<VfsWorkspace> => {
            let workspace: VfsWorkspace;

            switch (origin) {
                case "new":
                    workspace = VirtualFolders.Operations.createEmptySpec(name);
                    break;

                case "existing": {
                    const vfs = await getVirtualFolders(name);
                    workspace = VirtualFolders.Operations.buildWorkspaceFromRoot(vfs);
                    break;
                }

                default:
                    throw new Error(`Unknown origin: ${origin satisfies never}`);
            }

            return workspace;
        },
        toLockedSpec: async (origin:  'existing' | 'new', name: string): Promise<BasicUpdater<Ide>> => {
            const spec: ValueOrErrors<FullSpec, string> = 
                origin == 'existing' ? await getSpec(name) : ValueOrErrors.Default.return(FullSpec.Default());
            
            
            const workspace = await Ide.Operations.loadWorkspace(origin, name) 
          

            return spec.kind == "value" ?
                Ide.Updaters.lock(origin, name, spec.value, workspace)
                : (ide: Ide): Ide => ({...ide, lockingError: Option.Default.some(spec.errors)})
            
        }
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