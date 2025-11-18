import {DeltaDrain} from "./domains/delta/state"
import {
    AggregatedFlags,
    ConcreteRenderers,
    DispatchDeltaTransfer,
    DispatchDeltaTransferComparand,
    Option, simpleUpdater,
    Updater,
    Visibility
} from "ballerina-core";
import {WorkspaceState, WorkspaceVariant} from "../folders/state";
import {KnownSections} from "../../../../types/Json";
import {List} from "immutable";
import {INode, Meta} from "../folders/node";

type TD = [
    DispatchDeltaTransfer<any>,
    DispatchDeltaTransferComparand,
    AggregatedFlags<any>,
]


export type UIFramework = 'tailwind' | 'ui-kit';

export type UI =
    (| { kind: 'tailwind' }
     | { kind: 'ui-kit'  }) & { theme: string }

export type Forms = {
    spec:any, 
    ui: UI,
    specName: string,
    setState: (state: any) => void,
    launcher: string,
    deltas: Option<DeltaDrain>,
    showDeltas: boolean,
    path: string [],
}

export type Delta = {
    visibility: Visibility,
    drain: Option<DeltaDrain> }

export type LockedDisplay =
    {
        launchers:
            {
                names: string [],
                selected: Option<string>
            },
        deltas: Delta,
        ui: UI
    }

export const LockedDisplay = {
    Updaters: {
        Core: {
            ...simpleUpdater<LockedDisplay>()("deltas"),
            ...simpleUpdater<LockedDisplay>()("launchers"),
            change:(framework: UIFramework, theme?: string): Updater<LockedDisplay> =>
                Updater(ld => ({
                    ...ld,
                    ui: {
                        kind: framework,
                        theme: theme || "",
                    }
                }))
        }
    }
}  
