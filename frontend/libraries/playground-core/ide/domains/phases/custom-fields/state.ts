import {List} from "immutable";
import {Maybe, SimpleCallback, simpleUpdater, View} from "ballerina-core";
import {WorkspaceState} from "../locked/domains/folders/state";



export type CustomFieldsPhase = { 
    errors: List<string>,
    workspace: WorkspaceState
}

export const CustomFieldsPhase = {
    Default: (): CustomFieldsPhase => ({
        errors: List<string>(),
        workspace: WorkspaceState.Default({ kind: 'explore'}, null)
    }),
    Updaters: {
        Core: {
            ...simpleUpdater<CustomFieldsPhase>()("errors"),
            ...simpleUpdater<CustomFieldsPhase>()("workspace"),
        }
    }
}
