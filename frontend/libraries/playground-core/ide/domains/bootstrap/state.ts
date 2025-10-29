import {Updater, Option, replaceWith} from "ballerina-core";
import {Ide} from "../../state";
import {BootstrapUpdater} from "../types/PhaseUpdater";
import {LockedPhase} from "../locked/state";

export type BootstrapPhase =
    | { kind: "kickedOff" }
    | { kind: "initializing", message: string }
    | { kind: "ready"}

export const BootstrapPhase = {
    Updaters: {
        Core: {
            ready: (): Updater<BootstrapPhase> =>
                Updater((_: BootstrapPhase) => {
                    return { kind : 'ready' } as BootstrapPhase;
                })
        },
        Coroutine: {
            init: (msg: string): Updater<BootstrapPhase> => 
              replaceWith<BootstrapPhase>({ kind: 'initializing', message: msg})
        }
    }
}