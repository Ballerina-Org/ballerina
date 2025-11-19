import {List} from "immutable";
import {
    AggregatedFlags,
    DispatchDelta,
    DispatchDeltaTransfer,
    DispatchDeltaTransferComparand,
    Product
} from "ballerina-core";
type TD = [
    DispatchDeltaTransfer<any>,
    DispatchDeltaTransferComparand,
    AggregatedFlags<any>,
]
export type Deltas = List<TD> // List<DispatchDelta<any>>
export type DeltaDrain = Product<Deltas, Deltas>