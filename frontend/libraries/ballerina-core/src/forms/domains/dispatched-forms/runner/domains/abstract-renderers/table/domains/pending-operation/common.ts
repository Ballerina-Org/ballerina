import { Updater, ValueRecord, ValueTable } from "ballerina-core";
import { DispatchDelta } from "../../../../deltas/dispatch-delta/state";

export type PendingEdit<Flags> = {
  recordUpdater: Updater<ValueRecord>;
  updater: Updater<ValueTable>;
  delta: DispatchDelta<Flags>;
};
