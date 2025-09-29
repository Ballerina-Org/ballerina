import {Option, Value, Updater} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { ForeignMutationsInput } from "ballerina-core";
import { JsonEditorForeignMutationsExpected, JsonEditorView } from "./domains/editor/state";
import {List} from "immutable";
import {VfsWorkspace} from "./domains/locked/vfs/state";
import {Bootstrap} from "./domains/bootstrap/state";
import {FormsMode, LockedSpec, LockedStep} from "./domains/locked/state";
import {CommonUI, DataEntry} from "./domains/ui/state";
import {ChooseSource, ChooseStep} from "./domains/choose/state";


export type Ide =
    CommonUI & (
    |  { phase: 'bootstrap', bootstrap: Bootstrap }
    |  { phase: 'choose', progressIndicator: ChooseStep, source: ChooseSource }
    | ({ phase: 'locked', locked: LockedSpec } & LockedStep)
    // more for form-engine tbc
    )

export const Ide = {
    Default: (): Ide => 
        ({ ...CommonUI.Default(), 
            phase: 'bootstrap',
            bootstrap: { kind: 'kickedOff' }
        }),
    
    Updaters: {
        CommonUI: {
            specName: (name: Value<string>): Updater<Ide> => Updater(ide => ({...ide, create: { ...ide.create, name: name } })),
            lockingErrors: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, lockingError: e})),
            bootstrapErrors: (e: List<string>): Updater<Ide> =>  Updater(ide => ({...ide, bootstrappingError: e})),
            chooseErrors: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, choosingError: e})),
            formsError: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, formsError: e})),
        },
        Phases: {
            choosing: {},
            locking: {},
            bootstrapping: {},
            toChoosePhase: (): Updater<Ide> => Updater((ide: Ide): Ide => ({...ide, phase: 'choose', progressIndicator: 'default', source: 'manual'})),
            startUpload: (source: ChooseSource): Updater<Ide> => Updater(ide =>
                ide.phase === 'choose' ?
                ({...ide, progressIndicator: 'upload-started', source: source}): ide),
            finishUpload: (): Updater<Ide> => Updater(ide =>
                ide.phase === 'choose' ?
                ({...ide, progressIndicator: 'upload-finished'}): ide),
            progressUpload: (): Updater<Ide> => Updater(ide =>
                ide.phase === 'choose' ?
                    ({...ide, progressIndicator: 'upload-in-progress'}): ide),
            lockedPhase: (origin: 'existing' | 'create', 
                          how: DataEntry, 
                          name: string, 
                          workspace: VfsWorkspace,
                          formsMode: FormsMode,
            ): Updater<Ide> =>
                Updater(ide =>
                    ({
                        ...ide,
                        phase: 'locked',
                        step: 'design',
                        origin: origin,
                        create: {name: Value.Default(name), doneAs: Option.Default.some(how) },
                        locked: LockedSpec.Updaters.Core.Default(workspace, formsMode)
                    })),
            lockedOutcomePhase: (): Updater<Ide> =>
                Updater(ide => ({
                    ...ide,
                    step: 'outcome'
                }))
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