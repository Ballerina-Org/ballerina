import {Option, replaceWith, Updater, Value} from "ballerina-core";
import {Ide} from "../../state";
import {KnownSections, VfsWorkspace} from "./vfs/state";
import {Seeds} from "../seeds/state";
import {validate} from "../../api/specs";
import {CommonUI} from "../ui/state";
import {FlatNode} from "./vfs/upload/model";
import {List} from "immutable";
export type LockedStep =
    | { step: 'design' }
    | { step: 'outcome' };

export type LockedSpec = {
    seeds: Option<Seeds>, //: BridgeState,
    launchers: any [],
    entities: string [], 
    selectedEntity: Option<string>,
    selectedLauncher: Option<any>,
    virtualFolders: VfsWorkspace,
    mode: 'spec' | 'schema',
};

export const LockedSpec = {
    Updaters: {
        Core: {
            Default: (workspace: VfsWorkspace): LockedSpec => ({
                launchers: [],
                entities: [],
                selectedEntity: Option.Default.none(),
                selectedLauncher: Option.Default.none(),
                seeds: Option.Default.none(),
                virtualFolders: workspace,
                mode: 'spec',
            }),
            seed: (seeds: any): Updater<Ide> =>
                Updater(ide => ide.phase !== "locked" ? ide : ({...ide, locked: {...ide.locked, seeds: seeds}})),
            selectLauncher: (l: any): Updater<Ide> =>
                Updater(ide => {
                   
                    const launcher = ide.phase == 'locked' && ide.locked.launchers.find(launcher => launcher.key === l.key)
                    return ide.phase == 'locked'
                        ? ({
                            ...ide,
                            locked: {
                                ...ide.locked,
                                selectedLauncher: Option.Default.some(launcher)
                            }
                        })
                        : ide
                }),
            selectEntity: (e: string): Updater<Ide> =>
                Updater(ide => {
        
        
                    if(ide.phase == 'locked') {
                        const entity = ide.locked.entities.find(entity => entity === e)
                 
                        return ({
                            ...ide,
                            locked: {
                                ...ide.locked,
                                selectedEntity: Option.Default.some(entity!)
                            }
                        })
                    }
                    return ide
                }),
            vfs: (vfs: Updater<VfsWorkspace>): Updater<Ide> =>
                Updater(ide =>
                    ide.phase == 'locked'
                        ? ({...ide, locked: {...ide.locked, virtualFolders: vfs(ide.locked.virtualFolders)}})
                        : ({...ide})),

        }
    },
    Operations: {
        merge: (merged: KnownSections): Updater<Ide> =>
            LockedSpec.Updaters.Core.vfs(
                VfsWorkspace.Updaters.Core.merged(
                    replaceWith(
                        Option.Default.some(merged)
                    )
                )
            ).then(ide => 
           
                ide.phase == 'locked' 
                && ide.locked.virtualFolders.merged.kind == "r" ? 
                    ({...ide, 
                        locked: { 
                        ...ide.locked,
                            launchers: Object.entries(ide.locked.virtualFolders.merged.value.launchers!)
                                .filter(([, value]) => typeof value === "object" && value !== null)
                                .map(([key, value]) => ({
                                    key,
                                    ...(value as object),
                                })) as any
                        }
                    }) : ide
            ).then(ide => {
                if(ide.phase !== 'locked') { return ide }
               
                const schema = FlatNode.Operations.findFileByName(ide.locked.virtualFolders.nodes,"schema.json" );
                if(schema.kind == "l") return ({...ide, lockingError: ide.lockingError.push("schema file is missing")});
                const entitiesObj = Object.keys(schema.value.metadata.content.schema.entities || {})
               
                if(entitiesObj.length == 0) return ({...ide, lockingError: ide.lockingError.push("schema does not have entities")});
                const entities = entitiesObj
             
                return ({...ide, 
                    locked: { 
                    ...ide.locked, 
                        virtualFolders: {
                        ...ide.locked.virtualFolders, 
                            schema :schema
                    }, entities: entities
                }
                })
            })
    }
    
}
