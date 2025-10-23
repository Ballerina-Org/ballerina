import {List} from "immutable";
import {DispatchDelta, Product} from "ballerina-core";

export type Deltas = List<DispatchDelta<any>>
export type DeltaDrain = Product<Deltas, Deltas>