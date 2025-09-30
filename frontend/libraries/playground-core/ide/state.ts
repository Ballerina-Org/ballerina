import {Option, Value, Updater} from "ballerina-core";
import { Template, View } from "ballerina-core";
import { ForeignMutationsInput } from "ballerina-core";
import { JsonEditorForeignMutationsExpected, JsonEditorView } from "./domains/editor/state";
import {List} from "immutable";
import {ProgressiveWorkspace} from "./domains/locked/vfs/state";
import {Bootstrap} from "./domains/bootstrap/state";
import { LockedSpec, LockedStep} from "./domains/locked/state";
import {CommonUI} from "./domains/ui/state";
import {ChooseStep} from "./domains/choose/state";
import {DataEntry, SpecMode, SpecOrigin} from "./domains/spec/state";
import {FlatNode} from "./domains/locked/vfs/upload/model";

export type Ide =
    CommonUI & (
    |  { phase: 'bootstrap', bootstrap: Bootstrap }
    |  { phase: 'choose', specOrigin: SpecOrigin, entry: DataEntry, progressIndicator: ChooseStep }
    | ({ phase: 'locked', locked: LockedSpec } & LockedStep)
    )

export const Ide = {
    Default: (): Ide => 
        ({ ...CommonUI.Default(), 
            phase: 'bootstrap',
            bootstrap: { kind: 'kickedOff' }
        }),
    
    Updaters: {
        CommonUI: {
            specName: (name: Value<string>): Updater<Ide> => Updater(ide => ({...ide, name: name })),
            lockingErrors: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, lockingError: e})),
            bootstrapErrors: (e: List<string>): Updater<Ide> =>  Updater(ide => ({...ide, bootstrappingError: e})),
            chooseErrors: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, choosingError: e})),
            formsError: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, formsError: e})),
        },
        Phases: {
            bootstrapping: {
                toChoosePhase: (): Updater<Ide> => Updater((ide: Ide): Ide =>
                    ({...ide, phase: 'choose', entry:'upload-manual', progressIndicator: 'default',  specOrigin: {origin: 'creating' }})),
            },
            choosing: {
                startUpload: (entry: DataEntry): Updater<Ide> => Updater(ide =>
                    ide.phase === 'choose' ?
                        ({...ide, entry: entry, progressIndicator: 'upload-started'}): ide),
                progressUpload: (): Updater<Ide> => Updater(ide =>
                    ide.phase === 'choose' ?
                        ({...ide, progressIndicator: 'upload-in-progress'}): ide),
                finishUpload: (): Updater<Ide> => Updater(ide =>
                    ide.phase === 'choose' ?
                        ({...ide, progressIndicator: 'upload-finished'}): ide),
                toLocked: (name: string, node: FlatNode, specOrigin: SpecOrigin, formsMode: SpecMode): Updater<Ide> =>
                    Updater(ide =>
                        ({
                            ...ide, phase: 'locked', step: 'design',
                            name: Value.Default(name),
                            locked: { workspace: ProgressiveWorkspace.Default(node, formsMode, specOrigin), validatedSpec: Option.Default.none() },
                        })),
            },
            locking: {
                toOutcome: (): Updater<Ide> =>
                    Updater(ide => ide.phase != 'locked' ? ide : ({
                        ...ide,
                        locked: {...ide.locked, step: 'preDisplay'}
                    }))
            },
        }
    },
    Operations: {

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
type ExtractPhase<TUnion, TPhase extends string> =
    Extract<TUnion, { phase: TPhase }>;
export function PhaseUpdater<
    TUnion extends { phase: string },
    TPhase extends TUnion["phase"],
    TKey extends keyof ExtractPhase<TUnion, TPhase>
>(
    phase: TPhase,
    key: TKey,
    updater: (
        value: ExtractPhase<TUnion, TPhase>[TKey]
    ) => ExtractPhase<TUnion, TPhase>[TKey]
) {
    return (ide: TUnion): TUnion => {
        if (ide.phase === phase) {
            return {
                ...ide,
                [key]: updater((ide as ExtractPhase<TUnion, TPhase>)[key]),
            } as TUnion;
        }
        return ide;
    };
}