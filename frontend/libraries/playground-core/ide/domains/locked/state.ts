import {Option, Updater} from "ballerina-core";
import {Ide} from "../../state";
import {WorkspaceState} from "./vfs/state";
import {FlatNode, Node} from "./vfs/upload/model";
import {IdeEntity, IdeLauncher} from "../spec/state";
import {ProgressiveAB} from "../types/Progresssive";
import {LockedUpdater, LockedUpdaterFull} from "../types/PhaseUpdater";
import {KnownSections} from "../types/Json";

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

export type LockedStep =
    | { kind: 'design' }
    | { kind: 'preDisplay', selectEntityFromLauncher: ProgressiveAB<IdeLauncher, IdeEntity> };

export const LockedStep = {
    Updaters: {
        Core: {
            fromLaunchers: (launchers: IdeLauncher []) : LockedStep => 
                ({ kind: 'preDisplay', 
                    selectEntityFromLauncher: ProgressiveLauncherAndEntities.fromLaunchers(launchers) }),
            fromLauncherAndEntities: (launcher: IdeLauncher, entities: IdeEntity[]) : LockedStep  => 
                ({ kind: 'preDisplay', 
                    selectEntityFromLauncher: ProgressiveLauncherAndEntities.fromLauncherAndEntities(launcher, entities) }),
            selectEntity: (launcher: IdeLauncher, entity: IdeEntity) : LockedStep =>
                ({ kind: 'preDisplay', 
                    selectEntityFromLauncher: ProgressiveLauncherAndEntities.selectEntity(launcher, entity)})
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
            // test2: () => Updater(
            //     LockedUpdater(locked => ({
            //         ...locked,
            //         validatedSpec: Option.Default.none(),
            //         updated: true,  // additional field = error
            //     } satisfies LockedSpec))
            // ),
            // selectLauncher: (launcher: IdeLauncher): Updater<Ide> =>
            //     Updater(ide => {
            //         if(ide.phase != 'locked') return ide;
            //
            //         const schema = FlatNode.Operations.findFileByName(ide.locked.workspace.nodes,"schema.json" );
            //         if(schema.kind == "l") return ({...ide, lockingError: ide.lockingError.push("schema file is missing")});
            //        
            //         const entities = Object.keys(schema.value.metadata.content.schema.entities || {})
            //
            //         if(entities.length == 0) return ({...ide, lockingError: ide.lockingError.push("schema does not have entities")});
            //
            //         const step = LockedStep.Updaters.Core.fromLauncherAndEntities(launcher, entities)
            //         return ({...ide, locked: {...ide.locked, ...step}})
            //     }),
            selectLauncher2: (launcher: IdeLauncher): Updater<Ide> =>
                Updater(
                    LockedUpdater(locked => {
                        const schema = FlatNode.Operations.findFileByName(locked.workspace.nodes,"schema.json" );
                        if(schema.kind == "l") return ({...locked});

                        const entities = Object.keys(schema.value.metadata.content.schema.entities || {})

                        if(entities.length == 0) return ({...locked});

                        const step = LockedStep.Updaters.Core.fromLauncherAndEntities(launcher, entities)
                        return (
                            { ...locked, progress: step } satisfies LockedSpec)
                    }
                )),
            selectLauncher: (launcher: IdeLauncher): Updater<Ide> =>
                Updater(
                    LockedUpdaterFull((locked, ide) => {
                        const schema = FlatNode.Operations.findFileByName(
                            locked.workspace.nodes,
                            "schema.json"
                        );

                        if (schema.kind === "l") {
                            return {
                                ...ide,
                                lockingError: ide.lockingError.push("schema file is missing"),
                            };
                        }

                        const entities = Object.keys(
                            schema.value.metadata.content.schema.entities || {}
                        );

                        if (entities.length === 0) {
                            return {
                                ...ide,
                                lockingError: ide.lockingError.push(
                                    "schema does not have entities"
                                ),
                            };
                        }

                        const step = LockedStep.Updaters.Core.fromLauncherAndEntities(
                            launcher,
                            entities
                        );

                        return {
                            ...locked,
                            ...step,
                        } satisfies LockedSpec;
                    })
                ),
            selectEntity: (launcher: IdeLauncher, entity: IdeEntity): Updater<Ide> =>
                Updater(ide => {
                    if(ide.phase != 'locked') return ide;
                    const step = LockedStep.Updaters.Core.selectEntity(launcher, entity)
                    return ({...ide, locked: {...ide.locked, ...step}})
                }),
  
        },
        Core: {
            validated: (json: KnownSections): Updater<Ide> => 
                Updater(ide =>
                    ide.phase != 'locked' ? ide:
                        ({...ide, validated: Option.Default.some(json)})),
            // selectLauncher: (l: any): Updater<Ide> =>
            //     Updater(ide => {
            //         if(!(ide.phase == 'locked' && ide.step == 'preDisplay')){
            //             window.alert("IDE design bad state, cannot select launcher");
            //             return ide;
            //         }
            //         const launcher = ide.phase == 'locked' && ide.locked.launchers.find(launcher => launcher.key === l.key)
            //         return ide.phase == 'locked' && ide.step == 'preDisplay'
            //             ? ({
            //                 ...ide,
            //                 locked: {
            //                     ...ide.locked,
            //                     step: 'preDisplay', selectEntityFromLauncher: PreForms.
            //                 }
            //             })
            //             : ide
            //     }),
            // selectEntity: (e: string): Updater<Ide> =>
            //     Updater(ide => {
            //         if(ide.phase == 'locked') {
            //             const entity = ide.locked.entities.find(entity => entity === e)
            //     
            //             return ({
            //                 ...ide,
            //                 locked: {
            //                     ...ide.locked,
            //                     selectedEntity: Option.Default.some(entity!)
            //                 }
            //             })
            //         }
            //         return ide
            //     }),
            workspace: (workspace: Updater<WorkspaceState>): Updater<Ide> =>
                Updater(ide =>
                    ide.phase == 'locked'
                        ? ({...ide, locked: {...ide.locked, workspace: workspace(ide.locked.workspace)}})
                        : ({...ide})),
        }
    },
    Operations: {
        enableRun: (): Updater<Ide> =>
            Updater(
                LockedUpdater(locked => {
                    if (locked.validatedSpec.kind == "l") return locked;

                    const launchers = Object.entries(locked.validatedSpec.value.launchers!);
                    const ls =
                        launchers.filter(([, value]) => typeof value === "object" && value !== null)
                            .map(([key, value]) => ({
                                key,
                                ...(value as object),
                            })) as any
                    const step = LockedStep.Updaters.Core.fromLaunchers(ls);
                    return ({...locked, ...step})
                    
                })),
            //Updater((ide: Ide) => {
                //     if (ide.phase != 'locked' || ide.locked.validatedSpec.kind == "l") return ide;
                //
                //     const launchers = Object.entries(ide.locked.validatedSpec.value.launchers!);
                //     const ls = 
                //         launchers.filter(([, value]) => typeof value === "object" && value !== null)
                //         .map(([key, value]) => ({
                //             key,
                //             ...(value as object),
                //         })) as any
                //     const step = LockedStep.Updaters.Core.fromLaunchers(ls);
                //     return ({...ide, locked: {...ide.locked, ...step}})
                // })
           
                // ide.phase == 'locked' 
                // && ide.locked.validatedSpec.kind == "r" ? 
                //     ({...ide, 
                //         locked: { 
                //         ...ide.locked,
                //             launchers: Object.entries(ide.locked.virtualFolders.merged.value.launchers!)
                //                 .filter(([, value]) => typeof value === "object" && value !== null)
                //                 .map(([key, value]) => ({
                //                     key,
                //                     ...(value as object),
                //                 })) as any
                //         }
                //     }) : ide
            // ).then(ide => {
            //     if(ide.phase !== 'locked') { return ide }
            //   
            //     const schema = FlatNode.Operations.findFileByName(ide.locked.virtualFolders.nodes,"schema.json" );
            //     if(schema.kind == "l") return ({...ide, lockingError: ide.lockingError.push("schema file is missing")});
            //     const entitiesObj = Object.keys(schema.value.metadata.content.schema.entities || {})
            //   
            //     if(entitiesObj.length == 0) return ({...ide, lockingError: ide.lockingError.push("schema does not have entities")});
            // 
            //     return ({...ide, 
            //         locked: { 
            //         ...ide.locked, 
            //             virtualFolders: {
            //             ...ide.locked.virtualFolders, 
            //                 schema :schema
            //         }, entities: entitiesObj
            //     }
            //     })
            // })
    }
    
}
