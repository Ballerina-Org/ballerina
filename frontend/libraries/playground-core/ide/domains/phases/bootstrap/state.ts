import {Updater, replaceWith, simpleUpdater, View, Maybe} from "ballerina-core";
import {WorkspaceVariant} from "../locked/domains/folders/state";
import {List} from "immutable";
import {HeroPhase} from "../hero/state";

export type BootstrapPhase =
    (| { kind: "kickedOff" }
     | { kind: "initializing", message: string }
     | { kind: "ready"}
    ) & { variant: WorkspaceVariant, errors: List<string> }

export const BootstrapPhase = {
    Default: (variant: WorkspaceVariant): BootstrapPhase => ({ kind: 'kickedOff', variant: variant, errors: List<string>() }),
    Updaters: {
        Core: {
            ...simpleUpdater<BootstrapPhase>()("errors"),
            ready: (): Updater<BootstrapPhase> => Updater(b => ({...b, kind: 'ready'}))
        },
        Coroutine: {
            init: (msg: string): Updater<BootstrapPhase> =>
                Updater(b => ({...b, kind: 'initializing', message: msg})),
        }
    }
}

export type BootstrapPhaseForeignMutationsExpected = {}

export type BootstrapPhaseView = View<
    Maybe<BootstrapPhase>,
    Maybe<BootstrapPhase>,
    BootstrapPhaseForeignMutationsExpected,
    {
    }
>;
