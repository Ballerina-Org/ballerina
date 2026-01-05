import {List} from "immutable";
import {
    AggregatedFlags,
    DispatchDelta,
    DispatchDeltaTransfer,
    DispatchDeltaTransferComparand,
    Product
} from "ballerina-core";

export type IdeDeltaTransfer = [
    DispatchDeltaTransfer<any>,
    DispatchDeltaTransferComparand,
    AggregatedFlags<any>,
]

/**
 * `DeltaDrain` contains two independent streams of `IdeDeltaTransfer`:
 *
 * - **Left**  — current, unprocessed deltas
 * - **Right** — all previously processed deltas
 *
 */
export type DeltaDrain = Product<List<IdeDeltaTransfer>, List<IdeDeltaTransfer>>