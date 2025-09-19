import {
    Option,
    Value, Updater, BasicUpdater, ValueOrErrors, simpleUpdater
} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { ForeignMutationsInput } from "ballerina-core";
import { JsonEditorForeignMutationsExpected, JsonEditorView } from "./domains/editor/state";
import {List} from "immutable";
import {getSpec, initSpec} from "./api/specs";
import {VfsWorkspace,  VirtualFolders} from "./domains/locked/vfs/state";
import {Bootstrap} from "./domains/bootstrap/state";
import {LockedSpec, LockedStep} from "./domains/locked/state";
import {ChooseStep, CommonUI, DataOrigin} from "./domains/ui/state";
import {FlatNode} from "./domains/locked/vfs/upload/model";


export type Ide =
    CommonUI & (
    |  { phase: 'bootstrap', bootstrap: Bootstrap }
    |  { phase: 'choose', details: ChooseStep }
    | ({ phase: 'locked', locked: LockedSpec } & LockedStep)
    );

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
            

        },
        Template: {
            choosePhase: (): Updater<Ide> => Updater((ide: Ide): Ide => ({...ide, phase: 'choose', details: 'default'})),
            //chooseCreatePhase: (): Updater<Ide> => Updater((ide: Ide): Ide => ({...ide, phase: 'choose', specOrigin: 'create'})),
            startUpload: (): Updater<Ide> => Updater(ide =>
                ide.phase == 'choose' ?
                ({...ide, details: 'upload-started'}): ({...ide})),
            finishUpload: (): Updater<Ide> => Updater(ide =>
                ide.phase == 'choose' ?
                ({...ide, details: 'upload-finished'}): ({...ide})),
            progressUpload: (): Updater<Ide> => Updater(ide =>
                ide.phase == 'choose' ?
                    ({...ide, details: 'upload-in-progress'}): ({...ide})),
            lockedPhase: (origin: 'existing' | 'create', how: DataOrigin, name: string, workspace: VfsWorkspace): Updater<Ide> =>
                Updater(ide =>
                    ({
                        ...ide,
                        phase: 'locked',
                        step: 'design',
                        origin: origin,
                        create: {name: Value.Default(name), doneAs: Option.Default.some(how) },
                        locked: LockedSpec.Updaters.Core.Default(workspace)
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