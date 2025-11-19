import {
    AggregatedFlags, caseUpdater,
    DispatchDeltaTransfer,
    DispatchDeltaTransferComparand,
    Option,
    Product,
    replaceWith,
    Updater, simpleUpdater, View, Value, Maybe, Template, BasicUpdater, Visibility
} from "ballerina-core";
import {
    SelectedWorkspaceState,
    WorkspaceContext,
    WorkspaceForeignMutationsExpected,
    WorkspaceState,
    WorkspaceVariant,
    WorkspaceView
} from "./domains/folders/state"
import {KnownSections} from "../../types/Json"
import { List} from "immutable";
import {DeltaDrain, Deltas, IdeDeltaTransfer} from "./domains/forms/domains/delta/state"
import {INode, Meta} from "./domains/folders/node";
import {Ide} from "../../../state";
import {LockedDisplay, UI, UIFramework} from "./domains/forms/state";
import {CustomFields} from "../custom-fields/state";

export type LockedStep =
    | { kind: 'design' }
    | { kind: 'display', display: LockedDisplay };

export type LockedPhase = {
    readonly name: string;
    
    workspace: WorkspaceState,
    settings: Visibility;
    step: LockedStep,
    validatedSpec: Option<KnownSections>,
    errors: List<string>,
    customFields: CustomFields,
    automated: boolean // validate, run forms (for current launcher) on every keystroke / seconds elapsed
};

export const LockedPhase = {
    Default: (name: string, variant: WorkspaceVariant, node: INode<Meta>): LockedPhase => ({
            step: { kind: 'design' },
            name: name,
            workspace: WorkspaceState.Default(variant, node),
            validatedSpec: Option.Default.none(),
            automated: false,
            settings: 'fully-invisible',
            customFields: CustomFields.Default(),
            errors: List<string>()
        }),
    Updaters: {
        Core :{
            ...caseUpdater<LockedPhase>()("step")("display"),
            ...caseUpdater<LockedPhase>()("step")("design"),
            ...caseUpdater<LockedPhase>()("workspace")("selected"),
            ...caseUpdater<LockedPhase>()("workspace")("view"),
            ...simpleUpdater<LockedPhase>()("errors"),
            ...simpleUpdater<LockedPhase>()("automated"),
            ...simpleUpdater<LockedPhase>()("customFields"),
            toggleSettings: (): Updater<LockedPhase> => 
                 Updater(ls => ({ 
                     ...ls, 
                     settings: ls.settings == 'fully-invisible' ? 'fully-visible':'fully-invisible'}) satisfies LockedPhase),
            toDisplay: (launchers: string[]): Updater<LockedPhase> => 
               
                    Updater(ld => ({ 
                        ...ld,
                        step: { 
                            kind: 'display', 
                            display: {
                            ui: {
                                kind: 'ui-kit',
                                theme: 'blp'
                            },
                            launchers: { 
                                names: launchers, 
                                selected: Option.Default.none()
                            },
                            deltas: { visibility: 'fully-invisible', drain: Option.Default.none()}

                        
                        }}})),
            startDeltas: (): Updater<LockedPhase> => 
                LockedPhase.Updaters.Core.display(
                    LockedDisplay.Updaters.Core.deltas(d => ({...d, drain: Option.Default.some(
                            Product.Default(List<IdeDeltaTransfer>(), List<IdeDeltaTransfer>())
                        )}))),
            addDelta: (delta:IdeDeltaTransfer): Updater<LockedPhase> =>
                
                LockedPhase.Updaters.Core.display(d => ({
                    ...d, 
                    deltas: {
                        ...d.deltas, 
                        drain: 
                            Option.Updaters.some(
                                Product.Updaters.left<Deltas, Deltas>(current =>
                                    current.push(delta)
                                )
                            )(d.deltas.drain)
                    }
                }
                )),
            drainDeltas: (): Updater<LockedPhase> =>
                LockedPhase.Updaters.Core.display(
                    LockedDisplay.Updaters.Core.deltas(d => ({
                            ...d, 
                            drain:
                                Option.Updaters.some(
                                    Product.Updaters.right<Deltas, Deltas>(right =>
                                        right.concat(d.drain.kind == "l" ? List([]) : d.drain.value.left)
                                    )
                                    .then(
                                        Product.Updaters.left<Deltas, Deltas>(
                                            replaceWith(
                                                List()
                                            )
                                        )
                                    )
                                )(d.drain)
                    }))
                ),
            selectLauncher: (launcher: string): Updater<LockedPhase> =>
                LockedPhase.Updaters.Core.display(
                    LockedDisplay.Updaters.Core.launchers(l => ({
                        ...l, selected: Option.Default.some(launcher)}))),
            selectLauncherByNr: (nr: number): Updater<LockedPhase> =>
                LockedPhase.Updaters.Core.display(
                    LockedDisplay.Updaters.Core.launchers(l => ({
                        ...l, selected: Option.Default.some(l.names.at(nr - 1)!)}))),
            validated: (json: KnownSections): Updater<LockedPhase> => 
                Updater(ide => ({...ide, validatedSpec: Option.Default.some(json)})),
            workspace: (workspace: Updater<WorkspaceState>): Updater<LockedPhase> =>
                Updater(ide =>  ({...ide, workspace: workspace(ide.workspace)})),
                
        }
    },
    Operations: {
        addSuffix: (filename: string, suffix: string): string => {
            const dot = filename.lastIndexOf(".");
            return dot === -1
                ? `${filename}${suffix}`
                : `${filename.slice(0, dot)}${suffix}${filename.slice(dot)}`;
        },
    }
}

export type LockedPhaseForeignMutationsExpected = {
}

export type LockedPhaseView = View<
    Maybe<LockedPhase>,
    Maybe<LockedPhase>,
    LockedPhaseForeignMutationsExpected,
    {
        Workspace: Template<
            Maybe<LockedPhase>,
            Maybe<LockedPhase>,
            WorkspaceForeignMutationsExpected,
            WorkspaceView
        >;
    }
>;
