import {Option, Updater, caseUpdater, CaseUpdater} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { ForeignMutationsInput } from "ballerina-core";
import { JsonEditorForeignMutationsExpected, JsonEditorView } from "./domains/editor/state";
import {WorkspaceState} from "./domains/locked/vfs/state";
import {BootstrapPhase} from "./domains/bootstrap/state";
import {LockedPhase, LockedStep} from "./domains/locked/state";
import {CommonUI, Variant} from "./domains/common-ui/state";
import {Origin} from "./domains/choose/state";
import {Node} from "./domains/locked/vfs/upload/model";

// export type Ide =
//     CommonUI & (
//     |  { readonly kind: 'hero' }
//     |  ({ readonly kind: 'bootstrap'} & BootstrapPhase)
//     |  ({ readonly kind: 'selectionOrCreation'} & Origin)
//     |  ({ readonly kind: 'locked'} & LockedPhase)
//     )
//
// type App = {
//     ide: Ide;
//     otherStuff?: any;
// };
//
// export const App = {
//     Default: (): App =>
//         ({ ide : {...CommonUI.Default({ kind: 'scratch'} as Variant), kind: 'hero'}}),
//     Updater: {
//         ...caseUpdater<App>()("ide")("bootstrap"),
//         ...caseUpdater<App>()("ide")("locked"),
//         ...caseUpdater<App>()("ide")("hero"),
//         ...caseUpdater<App>()("ide")("selectionOrCreation"),
//     },
//     Operations: {
//         test: (app: App) => App.Updater.bootstrap( b => ({ ...b, kind2: 'initializing', message:''}))
//     }
// }

export type Ide =
    CommonUI & (
    |  { readonly phase: 'hero' }
    |  { readonly phase: 'bootstrap', bootstrap: BootstrapPhase }
    |  { readonly phase: 'selectionOrCreation', origin: Origin }
    |  { readonly phase: 'locked', locked: LockedPhase }
    )


export const IsBootstrap = (
    ide: Ide
): ide is Extract<Ide, { phase: "bootstrap" }> => ide.phase === "bootstrap";

export const IsHero = (
    ide: Ide
): ide is Extract<Ide, { phase: "hero" }> => ide.phase === "hero";

export const IsChoose= (
    ide: Ide
): ide is Extract<Ide, { phase: "selectionOrCreation" }> => ide.phase === "selectionOrCreation";

export const IsLocked= (
    ide: Ide
): ide is Extract<Ide, { phase: "locked" }> => ide.phase === "locked";

export const Ide = {
    Default: (): Ide => 
        ({ ...CommonUI.Default({ kind: 'scratch'} as Variant), phase: 'hero'}),
    
    Updaters: {

        Phases: {
            hero: {
                toBootstrap: (variant: Variant): Updater<Ide> => 
                    Updater((ide: Ide): Ide => ({
                        ...ide,
                            phase: 'bootstrap',
                        bootstrap: {kind: 'kickedOff'}
                    })
                    ).then(Updater((ide: Ide) => Object.assign(ide, CommonUI.Default(variant))))
                
            },
            bootstrapping: {
                update: (bp: Updater<BootstrapPhase>): Updater<Ide> =>
                    Updater((ide:Ide): Ide =>
                        ide.phase !== 'bootstrap' ? ide :({...ide, phase: 'bootstrap', bootstrap: bp(ide.bootstrap)})),
                toChoosePhase: (): Updater<Ide> => Updater((ide: Ide): Ide =>
                    ({...ide, phase: 'selectionOrCreation', origin: 'creating' })),
            },
            choosing: {
                startUpload: (): Updater<Ide> => 
                    Updater(ide =>
                    ide.phase === 'selectionOrCreation' && ide.variant.kind !== 'scratch'
                        ?
                        ({...ide, variant : {...ide.variant, upload: 'upload-started' }})
                        : ide),
                finishUpload: (): Updater<Ide> => Updater(ide =>
                    ide.phase === 'selectionOrCreation' && ide.variant.kind !== 'scratch'
                        ?
                        ({...ide, variant : {...ide.variant, upload: 'upload-finished' }})
                        : ide),
                toLocked: (node: Node): Updater<Ide> =>
                    Updater(ide => {
                        if (ide.phase != 'selectionOrCreation') return ide;
                        const origin = ide.origin
                        return ({
                            ...ide, phase: 'locked',
                            //name: Value.Default(name),
                            locked: {
                                progress: {kind: 'design'},
                                origin: origin,
                                workspace: WorkspaceState.Default(node),
                                validatedSpec: Option.Default.none()
                            },
                        })
                    }),
            },
            locking: {
                progress: (_: Updater<LockedStep>): Updater<Ide> =>
                    Updater(ide => ide.phase != 'locked' ? ide : ({
                        ...ide,
                        locked:  {...ide.locked, progress : _(ide.locked.progress)}
                    })),
                refreshVfs: (node: Node): Updater<Ide> => 
                    Updater(ide => {
                        
                        if (ide.phase != 'locked') return ide;
                        return ({
                            ...ide,
                            locked: {...ide.locked, workspace: WorkspaceState.Updater.reloadContent(node)(ide.locked.workspace)},
                        })
                    }),
                vfs: (_: Updater<WorkspaceState>): Updater<Ide> =>
                    Updater(ide => {

                        if (ide.phase != 'locked') return ide;
                        return ({
                            ...ide,
                            locked: {...ide.locked, workspace: _(ide.locked.workspace)},
                        })
                    })
            },
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
