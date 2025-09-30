import {Option, Updater} from "ballerina-core";
import {Ide, PhaseUpdater} from "../../state";
import {KnownSections, ProgressiveWorkspace} from "./vfs/state";
import {FlatNode} from "./vfs/upload/model";
import {IdeEntity, IdeLauncher} from "../spec/state";

export type ProgressiveUI<A, B> =
    | { kind: "selectA", options: A[] }
    | { kind: "selectB", a: A, options: B[] }
    | { kind: "done", a: A, b: B };

export const PreForms ={
    fromLaunchers: 
        (launchers: IdeLauncher[])
            : ProgressiveUI<IdeLauncher, IdeEntity> => 
            ({ kind: "selectA", options: launchers }),
    fromLauncherAndEntities: 
        (launcher: IdeLauncher, entities: IdeEntity[])
            : ProgressiveUI<IdeLauncher, IdeEntity> => 
            ({ kind: "selectB", a: launcher, options: entities}),
    selectEntity:
        (launcher: IdeLauncher, entity: IdeEntity)
            : ProgressiveUI<IdeLauncher, IdeEntity> => 
            ({ kind: "done", a: launcher, b: entity}),
}

export type LockedStep =
    | { step: 'design' }
    | { step: 'preDisplay', selectEntityFromLauncher: ProgressiveUI<IdeLauncher, IdeEntity> };

export const LockedStep = {
    Updaters: {
        Core: {
            fromLaunchers: (launchers: IdeLauncher []) => 
                ({ step: 'preDisplay', selectEntityFromLauncher: PreForms.fromLaunchers(launchers) }),
            fromLauncherAndEntities:
                (launcher: IdeLauncher, entities: IdeEntity[]) => 
                    ({ step: 'preDisplay', selectEntityFromLauncher: PreForms.fromLauncherAndEntities(launcher, entities) }),
            selectEntity:
                (launcher: IdeLauncher, entity: IdeEntity)=>
                    ({ step: 'preDisplay', selectEntityFromLauncher: PreForms.selectEntity(launcher, entity)})
        }
    }
}

export type LockedSpec = {
    workspace: ProgressiveWorkspace,
    validatedSpec: Option<KnownSections> // merged or full single
};

const LockedUpdater = (
    updater: (locked: LockedSpec) => LockedSpec
) => PhaseUpdater<Ide, "locked", "locked">("locked", "locked", updater);

function LockedUpdaterFull(
    updater: (locked: LockedSpec, ide: Ide) => LockedSpec | Ide
) {
    return (ide: Ide): Ide => {
        if (ide.phase === "locked") {
            const result = updater(ide.locked, ide);
            if ("phase" in result) {
                return result;
            } else {
                return { ...ide, locked: result };
            }
        }
        return ide;
    };
}

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

                        return ({
                        ...locked,
                        ...step
                        } satisfies LockedSpec)
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
            workspace: (workspace: Updater<ProgressiveWorkspace>): Updater<Ide> =>
                Updater(ide =>
                    ide.phase == 'locked'
                        ? ({...ide, locked: {...ide.locked, workspace: workspace(ide.locked.workspace)}})
                        : ({...ide})),
        }
    },
    Operations: {
        enableRun: (): Updater<Ide> =>
            Updater((ide: Ide) => {
                    if (ide.phase != 'locked' || ide.locked.validatedSpec.kind == "l") return ide;

                    const launchers = Object.entries(ide.locked.validatedSpec.value.launchers!);
                    const ls = 
                        launchers.filter(([, value]) => typeof value === "object" && value !== null)
                        .map(([key, value]) => ({
                            key,
                            ...(value as object),
                        })) as any
                    const step = LockedStep.Updaters.Core.fromLaunchers(ls);
                    return ({...ide, locked: {...ide.locked, ...step}})
                })
           
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
