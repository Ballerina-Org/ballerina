/* Experimental phase updater */

import {Ide} from "../../state";
import {LockedSpec} from "../locked/state";
import {Bootstrap} from "../bootstrap/state";

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
    updater: (locked: LockedSpec) => LockedSpec
) => PhaseUpdater<Ide, "locked", "locked">("locked", "locked", updater);

export const LockedUpdaterFull = (
    updater: (locked: LockedSpec, ide: Ide) => LockedSpec | Ide
): ((ide: Ide) => Ide) => {
    return (ide: Ide): Ide => {
        if (ide.phase === "locked") {
            const result = updater(ide.locked, ide);
            if ("phase" in result) {
                return result as Ide;
            } else {
                return { ...ide, locked: result as LockedSpec };
            }
        }
        return ide;
    };
};

export const BootstrapUpdater = (
    updater: (bootstrap: Bootstrap) => Bootstrap
) => PhaseUpdater<Ide, "bootstrap", "bootstrap">("bootstrap", "bootstrap", updater);
