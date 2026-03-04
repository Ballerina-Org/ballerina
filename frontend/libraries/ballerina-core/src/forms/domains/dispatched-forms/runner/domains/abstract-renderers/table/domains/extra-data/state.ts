import { ValueTable } from "ballerina-core";
import { OrderedMap } from "immutable";
import { TableAbstractRendererPendingOps } from "../pending-operation/state";

export const AbstractTableRendererValueTable = {
  Operations: {
    withExtraData: (
      value: ValueTable,
      pendingOps: TableAbstractRendererPendingOps,
    ): ValueTable => ({
      ...value,
      data: value.data.concat(
        pendingOps.kind == "add"
          ? pendingOps.pending
              .skipWhile((p) => p.idx < value.data.size)
              .map((p) => [p.id, p.record])
          : OrderedMap(),
      ),
    }),
  },
};
