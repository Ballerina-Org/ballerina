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
      data:
        pendingOps.kind == "add"
          ? value.data.concat(
              pendingOps.pending
                .skipWhile((p) => p.idx < value.data.size)
                .map((p) => [p.id, p.record]),
            )
          : pendingOps.kind == "remove"
              ? value.data.filterNot((_, k) =>
                  pendingOps.pending.some((p) => p.id == k),
                )
              : value.data,
    }),
  },
};
