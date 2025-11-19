/* Experimental phase updater */

import {Ide} from "../../state";
import {LockedPhase} from "../locked/state";
import {BootstrapPhase} from "../bootstrap/state";

type ExtractPhase<TUnion, TPhase extends string> =
    Extract<TUnion, { phase: TPhase }>;

export function PhaseUpdater<
    TUnion extends { phase: string },
    TPhase extends TUnion["phase"],
    TKey extends keyof ExtractPhase<TUnion, TPhase>
>(
    phase: TPhase,
    key: TKey,
    updater: (
        value: ExtractPhase<TUnion, TPhase>[TKey]
    ) => ExtractPhase<TUnion, TPhase>[TKey]
) {
    return (ide: TUnion): TUnion => {
        if (ide.phase === phase) {
            return {
                ...ide,
                [key]: updater((ide as ExtractPhase<TUnion, TPhase>)[key]),
            } as TUnion;
        }
        return ide;
    };
}

/* example for Locked */

export const LockedUpdater = (
    updater: (locked: LockedPhase) => LockedPhase
) => PhaseUpdater<Ide, "locked", "locked">("locked", "locked", updater);

export const LockedUpdaterFull = (
    updater: (locked: LockedPhase, ide: Ide) => LockedPhase | Ide
): ((ide: Ide) => Ide) => {
    return (ide: Ide): Ide => {
        if (ide.phase === "locked") {
            const result = updater(ide.locked, ide);
            if ("kind" in result) {
                return result as Ide;
            } else {
                return { ...ide, locked: result as LockedPhase };
            }
        }
        return ide;
    };
};

export const BootstrapUpdater = (
    updater: (bootstrap: BootstrapPhase) => BootstrapPhase
) => PhaseUpdater<Ide, "bootstrap", "bootstrap">("bootstrap", "bootstrap", updater);
