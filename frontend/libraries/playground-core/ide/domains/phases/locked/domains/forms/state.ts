import {DeltaDrain} from "./domains/delta/state"
import {
    Option, simpleUpdater,
    Updater,
    Visibility
} from "ballerina-core";

import {INode, Meta} from "../folders/node";
import {CustomEntity} from "../../../custom-fields/state";
import {KnownSections} from "../../../../types/Json";

export type UIFramework = 'ui-kit' | 'tailwind';

export type UI = { kind: UIFramework, theme: string }

export type FormsSpec = {
    specDefinition: KnownSections,
    specName: string,
    specPath: string [],
}

export type LockedDisplay =
    {
        launchers:
            {
                names: string [],
                selected: Option<string>
            },
        workspace: {
            nodes: INode<Meta>,
            selected: INode<Meta>,
        },
        customEntity: Option<CustomEntity>,
        deltas: Option<DeltaDrain>,
        ui: UI,
        show: {
            deltas: Visibility,
            customEntities: Visibility,
        },
        spec: FormsSpec
    }

export const LockedDisplay = {
    Updaters: {
        Core: {
            ...simpleUpdater<LockedDisplay>()("deltas"),
            ...simpleUpdater<LockedDisplay>()("launchers"),
            ...simpleUpdater<LockedDisplay>()("show"),
            ...simpleUpdater<LockedDisplay>()("customEntity"),
            ...simpleUpdater<LockedDisplay>()("spec"),
            
            changeUIFramework:(framework: UIFramework, theme: string): Updater<LockedDisplay> =>
                Updater(current => ({
                    ...current,
                    ui: {
                        kind: framework,
                        theme: theme,
                    }
                } satisfies LockedDisplay))
        }
    }
}  
