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
import {DeltaDrain, IdeDeltaTransfer} from "./domains/forms/domains/delta/state"
import {INode, Meta} from "./domains/folders/node";
import {Ide} from "../../../state";
import {LockedDisplay, UI, UIFramework} from "./domains/forms/state";
import {CustomEntity} from "../custom-fields/state";

export type LockedStep =
    | { kind: 'design' }
    | { kind: 'display', display: LockedDisplay };

export type LockedPhase = {
    readonly name: string;
    
    workspace: WorkspaceState,
    settings: Visibility;
    step: LockedStep,
    validatedSpec: Option<KnownSections>,
    errors: List<string>
};

export const LockedPhase = {
    Default: (name: string, variant: WorkspaceVariant, node: INode<Meta>): LockedPhase => ({
            step: { kind: 'design' },
            name: name,
            workspace: WorkspaceState.Default(variant, node),
            validatedSpec: Option.Default.none(),
            settings: 'fully-invisible',
            errors: List<string>()
        }),
    Updaters: {
        Core :{
            ...caseUpdater<LockedPhase>()("step")("display"),
            ...caseUpdater<LockedPhase>()("step")("design"),
            ...caseUpdater<LockedPhase>()("workspace")("selected"),
            ...caseUpdater<LockedPhase>()("workspace")("view"),
            ...simpleUpdater<LockedPhase>()("errors"),
            toggleSettings: (): Updater<LockedPhase> => 
                 Updater(current => ({ 
                     ...current, 
                     settings: current.settings == 'fully-invisible' ? 'fully-visible' : 'fully-invisible'}) satisfies LockedPhase),
            toDisplay: (launchers: string[], spec: KnownSections, name: string, workspace: Extract<WorkspaceState, { kind: 'selected' }>): Updater<LockedPhase> =>
                    Updater(current => ({ 
                        ...current,
                        step: { 
                            kind: 'display', 
                            display: {
                                ui: {
                                    kind: 'ui-kit',
                                    theme: 'blp'
                                },
                                workspace: {
                                  nodes: workspace.nodes,
                                  selected: workspace.file  
                                },
                                launchers: { 
                                    names: launchers, 
                                    selected: Option.Default.none()
                                },
                                show: {
                                    deltas: "fully-invisible",
                                    customEntities: 'fully-invisible',
                                },
                                deltas: Option.Default.none(),
                                customEntity: Option.Default.none(),
                                spec: {
                                    specName: name,
                                    specDefinition: spec,
                                    specPath: current.workspace.nodes.metadata.path.split('/')
                                }
                            }
                        }
                    })),
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
                                Product.Updaters.left<List<IdeDeltaTransfer>, List<IdeDeltaTransfer>>(current =>
                                    current.push(delta)
                                )
                            )(d.deltas)
                    }
                }
                )),
            drainDeltas: (): Updater<LockedPhase> =>
                LockedPhase.Updaters.Core.display(
                    LockedDisplay.Updaters.Core.deltas(d => ({
                            ...d, 
                            drain:
                                Option.Updaters.some(
                                    Product.Updaters.right<List<IdeDeltaTransfer>, List<IdeDeltaTransfer>>(right =>
                                        right.concat(d.kind == "l" ? List([]) : d.value.left)
                                    )
                                    .then(
                                        Product.Updaters.left<List<IdeDeltaTransfer>, List<IdeDeltaTransfer>>(
                                            replaceWith(
                                                List()
                                            )
                                        )
                                    )
                                )(d)
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
