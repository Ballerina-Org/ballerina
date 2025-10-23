import {DispatchDelta, Option, Product, replaceWith, Updater} from "ballerina-core";
import {Ide} from "../../state";
import {WorkspaceState} from "./vfs/state";
import {FlatNode, Node} from "./vfs/upload/model";
import {IdeEntity, IdeLauncher} from "../spec/state";
import {ProgressiveAB} from "../types/Progresssive";
import {LockedUpdater, LockedUpdaterFull} from "../types/PhaseUpdater";
import {KnownSections} from "../types/Json";
import {Dispatch} from "react";
import { List, Map} from "immutable";
import {DeltaDrain, Deltas} from "../forms/deltas/state";

export const ProgressiveLauncherAndEntities ={
    fromLaunchers: 
        (launchers: IdeLauncher[])
            : ProgressiveAB<IdeLauncher, IdeEntity> => 
            ({ kind: "selectA", options: launchers }),
    fromLauncherAndEntities: 
        (launcher: IdeLauncher, entities: IdeEntity[])
            : ProgressiveAB<IdeLauncher, IdeEntity> => 
            ({ kind: "selectB", a: launcher, options: entities}),
    selectEntity:
        (launcher: IdeLauncher, entity: IdeEntity)
            : ProgressiveAB<IdeLauncher, IdeEntity> => 
            ({ kind: "done", a: launcher, b: entity}),
}

export type FormsDataEntry =
    | { kind: 'launcher', selectEntityFromLauncher: ProgressiveAB<IdeLauncher, IdeEntity> }
    | { kind: 'launchers', launchers: string [], selected: Option<string> }
    | { kind: 'schema-selector', selectEntityAndLookups: ProgressiveAB<string, { name: string, fromEntity: string, toEntity: string }> }

export type LockedStep =
    | { kind: 'design' }
    | { kind: 'preDisplay', dataEntry: FormsDataEntry, deltas: Option<DeltaDrain> };

export const LockedStep = {
    Updaters: {
        Core: {
            fromLaunchers: (launchers: IdeLauncher []) : LockedStep => 
                ({ kind: 'preDisplay',
                        deltas: Option.Default.none(),
                        dataEntry: { kind: 'launcher', selectEntityFromLauncher: ProgressiveLauncherAndEntities.fromLaunchers(launchers) }}),
            fromLauncherAndEntities: (launcher: IdeLauncher, entities: IdeEntity[]) : LockedStep  => 
                ({ kind: 'preDisplay',
                    deltas: Option.Default.none(),
                    dataEntry: { kind: 'launcher', selectEntityFromLauncher: ProgressiveLauncherAndEntities.fromLauncherAndEntities(launcher, entities) }}),
            selectEntity: (launcher: IdeLauncher, entity: IdeEntity) : LockedStep =>
                ({ kind: 'preDisplay',
                    deltas: Option.Default.none(),
                    dataEntry: { kind: 'launcher', selectEntityFromLauncher: ProgressiveLauncherAndEntities.selectEntity(launcher, entity)}}),
            launchers: (launchers: string[]): LockedStep => 
                ({ kind: 'preDisplay',
                    deltas: Option.Default.none(),
                    dataEntry: { kind: 'launchers', launchers: launchers, selected: Option.Default.none()}})
                    ,
            selectLauncher: (launcher: string,launchers: IdeLauncher []): LockedStep =>
                ({ kind: 'preDisplay', deltas: Option.Default.none(),
                    dataEntry: { 
                        kind: 'launchers', 
                        launchers: launchers, 
                        selected: Option.Default.some(launcher)
                    }
                }
                )
        }
    }
}

export type LockedSpec = {
    workspace: WorkspaceState,
    progress: LockedStep,
    validatedSpec: Option<KnownSections> // merged or full single
};

export const LockedSpec = {
    Updaters: {
        Step :{
            addDelta: (delta:DispatchDelta<any>): Updater<Ide> =>
                Updater(
                    LockedUpdaterFull((locked, ide) => {
                        if(locked.progress.kind != 'preDisplay' || locked.progress.deltas.kind == "l") return locked;

                        return {
                            ...locked,
                            progress: { 
                                ...locked.progress, 
                                deltas: Option.Default.some(Product.Updaters.left<Deltas, Deltas>(current => current.push(delta))(locked.progress.deltas.value))
                            },
                        } satisfies LockedSpec;
                    })
                ),
            drainDeltas :() : Updater<Ide> =>
                Updater(
                    LockedUpdater(locked => {
                        if(locked.progress.kind != 'preDisplay' || locked.progress.deltas.kind == "l") return locked;
                        const deltas = locked.progress.deltas;
                        return {
                            ...locked,
                            progress: {
                                ...locked.progress,
                                deltas: Option.Default.some(
                                    Product.Updaters.right<Deltas, Deltas>(right => 
                                        right.concat(deltas.value.left))
                                        .then(Product.Updaters.left<Deltas, Deltas>(replaceWith(List())))(deltas.value))
                            },
                        };
                    })
                ),
            selectLauncher: (launcher: IdeLauncher): Updater<Ide> =>
                Updater(
                    LockedUpdaterFull((locked, ide) => {
                        if(locked.progress.kind != 'preDisplay' || locked.progress.dataEntry.kind != 'launchers') return locked;
                        return {
                            ...locked,
                            progress: LockedStep.Updaters.Core.selectLauncher(launcher, locked.progress.dataEntry.launchers),
                        } satisfies LockedSpec;
                    })
                ),
            selectLauncherByNr: (nr: number): Updater<Ide> =>
                Updater(
                    LockedUpdaterFull((locked, ide) => {
                        if(locked.progress.kind != 'preDisplay' || locked.progress.dataEntry.kind != 'launchers') return locked;
                        const name = locked.progress.dataEntry.launchers.at(nr - 1)!
                        return {
                            ...locked,
                            progress: LockedStep.Updaters.Core.selectLauncher(name, locked.progress.dataEntry.launchers),
                        } satisfies LockedSpec;
                    })
                ),
            selectEntity: (launcher: IdeLauncher, entity: IdeEntity): Updater<Ide> =>
                Updater(ide => {
                    if(ide.phase != 'locked') return ide;
                    const step = LockedStep.Updaters.Core.selectEntity(launcher, entity)
                    return ({...ide, locked: {...ide.locked, progress: step}});
                }),
  
        },
        Core: {
            validated: (json: KnownSections): Updater<Ide> => 
                Updater(ide =>
                    ide.phase != 'locked' ? ide:
                        ({...ide, locked: {...ide.locked, validatedSpec: Option.Default.some(json)}})),
            workspace: (workspace: Updater<WorkspaceState>): Updater<Ide> =>
                Updater(ide =>
                    ide.phase == 'locked'
                        ? ({...ide, locked: {...ide.locked, workspace: workspace(ide.locked.workspace)}})
                        : ({...ide})),
        }
    },
    Operations: {
        addSuffix: (filename: string, suffix: string): string => {
            const dot = filename.lastIndexOf(".");
            return dot === -1
                ? `${filename}${suffix}`
                : `${filename.slice(0, dot)}${suffix}${filename.slice(dot)}`;
        },
        enableRun: (launchers: string []): Updater<Ide> =>
            Updater(
                LockedUpdater(locked => {

                    if (locked.validatedSpec.kind == "l") return locked;
                    const step = LockedStep.Updaters.Core.launchers(launchers)
                    const next = {...locked, progress: step}
                    return next;
                    
                })),
        startDeltas: (): Updater<Ide> =>
            Updater(
                LockedUpdater(locked => {

                    if (locked.validatedSpec.kind == "l") return locked;
                    return {...locked, progress: { ...locked.progress, deltas: Option.Default.some(Product.Default(List(), List()))}}
                })),
    }
}
