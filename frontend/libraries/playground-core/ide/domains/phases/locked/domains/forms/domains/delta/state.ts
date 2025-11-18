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
export type Deltas = List<IdeDeltaTransfer> 
export type DeltaDrain = Product<Deltas, Deltas>