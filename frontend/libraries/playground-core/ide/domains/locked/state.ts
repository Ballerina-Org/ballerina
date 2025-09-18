import {Option, replaceWith, Updater, Value} from "ballerina-core";
import {Ide} from "../../state";
import {KnownSections, VfsWorkspace} from "./vfs/state";
import {Seeds} from "../seeds/state";
import {validate} from "../../api/specs";
import {CommonUI} from "../ui/state";
export type LockedStep =
    | { step: 'design' }
    | { step: 'outcome' };

export type LockedSpec = {
    seeds: Option<Seeds>, //: BridgeState,
    launchers: string [],
    selectedLauncher: Option<Value<string>>,
    virtualFolders: VfsWorkspace,
    mode: 'spec' | 'schema',
};

export const LockedSpec = {
    Updaters: {
        Core: {
            Default: (workspace: VfsWorkspace): LockedSpec => ({
                launchers: [], //spec.v1.launchers ? Array.from(Object.keys(spec.v1.launchers)): [],
                selectedLauncher: Option.Default.none(),
                seeds: Option.Default.none(),
                virtualFolders: workspace,
                mode: 'spec',
            }),
            seed: (seeds: any): Updater<Ide> =>
                Updater(ide => ide.phase !== "locked" ? ide : ({...ide, lockedPhase: {...ide.locked, seeds: seeds}})),
            selectLauncher: (name: string): Updater<Ide> =>
                Updater(ide =>
                    ide.phase == 'locked'
                        ? ({
                            ...ide,
                            lockedPhase: {...ide.locked, selectedLauncher: Option.Default.some(Value.Default(name))}
                        })
                        : ({...ide})),
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
            )
    }
    
}
