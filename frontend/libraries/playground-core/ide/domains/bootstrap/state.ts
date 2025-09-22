import {Updater, Option} from "ballerina-core";
import {Ide} from "../../state";

export type Bootstrap =
    | { kind: "kickedOff" }
    | { kind: "initializing", message: string }
    | { kind: "ready"}

export const Bootstrap = {
    Updaters: {
        Core: {
            init: (msg: string): Updater<Ide> => 
                Updater(ide => 
                    ({...ide, 
                        phase: 'bootstrap', 
                        bootstrap: {
                            kind: 'initializing', 
                            message: msg 
                        }
                    })),
            ready: (specNames: string []): Updater<Ide> =>
                Updater(ide =>
                    ide.phase === 'bootstrap'
                        ? { ...ide, specOrigin: specNames.length > 0 ? 'existing' : 'create', bootstrap: { kind: 'ready' }, existing: { specs: specNames, selected: Option.Default.none() } }
                        : ide),
        },
    }
}