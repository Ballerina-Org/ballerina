import {Option, Value, Updater} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { ForeignMutationsInput } from "ballerina-core";
import { JsonEditorForeignMutationsExpected, JsonEditorView } from "./domains/editor/state";
import {List} from "immutable";
import {WorkspaceState} from "./domains/locked/vfs/state";
import {Bootstrap} from "./domains/bootstrap/state";
import { LockedSpec, LockedStep} from "./domains/locked/state";
import {CommonUI} from "./domains/ui/state";
import {ChoosePhase, ChooseStep} from "./domains/choose/state";
import {DataEntry, SpecMode, SpecOrigin} from "./domains/spec/state";
import {Node} from "./domains/locked/vfs/upload/model";

export type Ide =
    CommonUI & (
    |  { phase: 'hero' }
    |  { phase: 'bootstrap', bootstrap: Bootstrap }
    |  { phase: 'choose', choose: ChoosePhase }
    |  { phase: 'locked', locked: LockedSpec }
    )

export const Ide = {
    Default: (): Ide => 
        ({ ...CommonUI.Default(), 
            phase: 'hero'
        }),
    
    Updaters: {
        CommonUI: {
            specName: (name: Value<string>): Updater<Ide> => Updater(ide => ({...ide, name: name })),
            lockingErrors: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, lockingError: e})),
            bootstrapErrors: (e: List<string>): Updater<Ide> =>  Updater(ide => ({...ide, bootstrappingError: e})),
            chooseErrors: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, choosingError: e})),
            formsError: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, formsError: e})),
            toggleSettings: (): Updater<Ide> => Updater(ide => ({...ide, settingsVisible: !ide.settingsVisible})),
            toggleHero: (): Updater<Ide> => Updater(ide => ({...ide, heroVisible: !ide.heroVisible})),
            clearAllErrors : (): Updater<Ide> =>
                Updater(ide => ({
                    ...ide,
                    lockingError: List(),
                    choosingError: List(),
                    bootstrappingError: List(),
                })),
        },
        Phases: {
            hero: {
                toBootstrap: (): Updater<Ide> => 
                    Updater((ide:Ide): Ide =>
                        ({...ide, phase: 'bootstrap', bootstrap: { kind: 'kickedOff' }})
                    )
            },
            bootstrapping: {
                toChoosePhase: (): Updater<Ide> => Updater((ide: Ide): Ide =>
                    ({...ide, phase: 'choose', choose: {entry:'upload-manual', progressIndicator: 'default',  specOrigin: {origin: 'creating' }}})),
            },
            choosing: {
                startUpload: (entry: DataEntry): Updater<Ide> => Updater(ide =>
                    ide.phase === 'choose' ?
                        ({...ide, choose: { ...ide.choose, entry: entry, progressIndicator: 'upload-started'}}): ide),
                progressUpload: (): Updater<Ide> => Updater(ide =>
                    ide.phase === 'choose' ?
                        ({...ide, choose: { ...ide.choose, progressIndicator: 'upload-in-progress'}}): ide),
                finishUpload: (): Updater<Ide> => Updater(ide =>
                    ide.phase === 'choose' ?
                        ({...ide, choose: { ...ide.choose, progressIndicator: 'upload-finished'}}): ide),
                toLocked: (name: string, node: Node, specOrigin: SpecOrigin, formsMode: SpecMode): Updater<Ide> =>
                    Updater(ide =>
                        ({
                            ...ide, phase: 'locked',
                            name: Value.Default(name),
                            locked: { progress: {kind: 'design'}, workspace: WorkspaceState.Default(node, formsMode, specOrigin), validatedSpec: Option.Default.none() },
                        })),
            },
            locking: {
                toOutcome: (): Updater<Ide> =>
                    Updater(ide => ide.phase != 'locked' ? ide : ({
                        ...ide,
                        locked: {...ide.locked, step: 'preDisplay'}
                    })),
                refreshVfs: (node: Node): Updater<Ide> => 
                    Updater(ide => {
                        
                        if (ide.phase != 'locked') return ide;
                        return ({
                            ...ide,
                            locked: {...ide.locked, workspace: WorkspaceState.Updater.reloadContent(node)(ide.locked.workspace)},
                        })
                    }),
                vfs: (vfs: Updater<WorkspaceState>): Updater<Ide> =>
                    Updater(ide => {

                        if (ide.phase != 'locked') return ide;
                        return ({
                            ...ide,
                            locked: {...ide.locked, workspace: vfs(ide.locked.workspace)},
                        })
                    })
            },
        }
    },
    Operations: {
        clearErrors: () => Ide.Updaters.CommonUI.lockingErrors(List([])).then(
            Ide.Updaters.CommonUI.chooseErrors(List([])).then(Ide.Updaters.CommonUI.bootstrapErrors(List([])))
        )
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
