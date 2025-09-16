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
import {Seeds} from "./domains/seeds/state";
import {Bootstrap} from "./domains/bootstrap/state";
import {LockedSpec, LockedStep} from "./domains/locked/state";
import {Spec} from "./domains/spec/state";

export type CommonUI = {
    specOrigin: 'existing' | 'create';
    existing: { specs: string[]; selected: Option<string> };
    create: { name: Value<string> };
    
    // these errors will have different UI representations: toasts, dock, etc
    bootstrappingError: Option<List<string>>,
    choosingError: Option<List<string>>,
    lockingError: Option<List<string>>,
};

const CommonUI = {
    Default: (): CommonUI => ({
        specOrigin: 'create',
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
    | ({ phase: 'locked'; source: 'existing' | 'create'; locked: LockedSpec } & LockedStep)
    );

export const Ide = {
    Default: (): Ide => 
        ({ ...CommonUI.Default(), 
            phase: 'bootstrap',
            bootstrap: { kind: 'kickedOff' }
        }),
    Updaters: {
        bootstrap: (b: Updater<Ide>): Updater<Ide> => Updater(ide => b(ide)),
        lockedSpec: (b: Updater<Ide>): Updater<Ide> => Updater(ide => b(ide)),
        
        toChoose: (): BasicUpdater<Ide>  => (ide: Ide): Ide => ({...ide, phase: 'choose'}),
        chooseNew: (): BasicUpdater<Ide>  => (ide: Ide): Ide => ({...ide, phase: 'choose', specOrigin: 'create'}),
        lock: (origin:  'existing' | 'create', name: string, spec: Spec, workspace: VfsWorkspace): BasicUpdater<Ide> => {
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
                ide.phase == "choose" && ide.specOrigin == "create" ? 
                    ({ ...ide, create: { name: Value.Default(name) } })
                    : ({ ...ide })
    },
    Operations: {
        loadWorkspace: async (origin: 'existing' | 'create', name: string): Promise<VfsWorkspace> => {
            let workspace: VfsWorkspace;

            switch (origin) {
                case "create":
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
        toLockedSpec: async (origin:  'existing' | 'create', name: string): Promise<BasicUpdater<Ide>> => {
            const spec: ValueOrErrors<Spec, string> = 
                origin == 'existing' ? await getSpec(name) : ValueOrErrors.Default.return(Spec.Default());
            
            
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