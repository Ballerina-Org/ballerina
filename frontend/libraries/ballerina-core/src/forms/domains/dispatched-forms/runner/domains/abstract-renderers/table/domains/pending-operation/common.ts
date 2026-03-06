import {
  DispatchDelta,
  Updater,
  ValueRecord,
  ValueTable,
} from "ballerina-core";

export type PendingEdit<Flags> = {
  recordUpdater: Updater<ValueRecord>;
  updater: Updater<ValueTable>;
  delta: DispatchDelta<Flags>;
};
