import {Updater, Option} from "ballerina-core";
import {Ide} from "../../state";
import {BootstrapUpdater} from "../types/PhaseUpdater";

export type Bootstrap =
    | { kind: "kickedOff" }
    | { kind: "initializing", message: string }
    | { kind: "ready"}

export const Bootstrap = {
    Updaters: {
        Core: {
            init: (msg: string): Updater<Ide> =>
                Updater(
                    BootstrapUpdater(bootstrap => ({
                        kind: 'initializing', 
                        message: msg
                    }))),
            ready: (specNames: string []): Updater<Ide> =>
                Updater(ide =>
                    ide.phase === 'bootstrap'
                        ? { ...ide, 
                            specOrigin: specNames.length > 0 
                                ? 'existing' 
                                : 'create', 
                            bootstrap: { kind: 'ready' }, 
                            specSelection: { specs: specNames, selected: Option.Default.none() } }
                        : ide),
        },
    }
}