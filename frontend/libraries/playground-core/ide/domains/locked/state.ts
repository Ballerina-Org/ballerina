import {Bridge, BridgeState} from "../bridge/state";
import {BasicUpdater, Option, Updater, Value} from "ballerina-core";
import {VfsWorkspace} from "../vfs/state";
import {Ide} from "../../state";
import {Spec} from "../spec/state";

export type LockedSpec = {
    bridge: BridgeState,
    launchers: string [],
    selectedLauncher: Option<Value<string>>,
    virtualFolders: VfsWorkspace,
    mode: 'spec' | 'schema',
};

export const LockedSpec = {
    Updaters: {
        Core: {
            Default: (spec: Spec, workspace: VfsWorkspace): LockedSpec => ({
                launchers: [], //spec.v1.launchers ? Array.from(Object.keys(spec.v1.launchers)): [],
                selectedLauncher: Option.Default.none(),
                bridge: Bridge.Default(spec),
                virtualFolders: workspace,
                mode: 'spec',
            }),
            toSchemaMode: (): Updater<Ide> =>
                Updater(ide =>
                    ide.phase !== "locked" ? ide : ({...ide, locked: {...ide.locked, mode: 'schema' }})),
            toSpecMode: (): Updater<Ide> =>
                Updater(ide =>
                    ide.phase !== "locked" ? ide : ({...ide, locked: {...ide.locked, mode: 'spec' }})),
            seed: (seeds: any): Updater<Ide> =>
                Updater(ide =>
                    ide.phase !== "locked" ? ide : ({...ide, locked: {...ide.locked, bridge: {...ide.locked.bridge, seeds: seeds}}})),
            selectLauncher: (name: string): Updater<Ide> =>
                Updater(ide =>
                        ide.phase == 'locked'
                        ? ({...ide, locked: {...ide.locked, selectedLauncher: Option.Default.some(Value.Default(name))}})
                        : ({...ide})),
            bridge: {
                v1: (value: string): Updater<Ide> =>
                    Updater(ide =>
                        ide.phase == 'locked'
                            ? ({...ide, locked: {...ide.locked, bridge: Bridge.Updaters.Template.setV1Body(Value.Default(value))(ide.locked.bridge)}})
                            : ({...ide})),
            }
        }
    }
}
