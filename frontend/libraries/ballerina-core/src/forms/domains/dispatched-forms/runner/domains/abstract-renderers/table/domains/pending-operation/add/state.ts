import { List } from "immutable";
import { simpleUpdater, ValueRecord } from "ballerina-core";

export type PendingAddOperationId = `placeholder-${string}`;
export type TableAbstractRendererPendingAddOperation = {
  idx: number;
  id: PendingAddOperationId;
  record: ValueRecord;
  flags: any;
};
export const TableAbstractRendererPendingAddOperation = {
  Default: (
    idx: number,
    id: PendingAddOperationId,
    record: ValueRecord,
    flags: any,
  ): TableAbstractRendererPendingAddOperation => ({
    idx,
    id,
    record,
    flags,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<TableAbstractRendererPendingAddOperation>()("record"),
    },
  },
};

export type TableAbstractRendererPendingAddOps = {
  kind: "add";
  pending: List<TableAbstractRendererPendingAddOperation>;
  initialTableSize: number;
  totalAdded: number;
};
export const TableAbstractRendererPendingAddOps = {
  Default: {
    empty: (initialTableSize: number): TableAbstractRendererPendingAddOps => ({
      kind: "add",
      pending: List(),
      totalAdded: 0,
      initialTableSize,
    }),
    singleton: (
      v: TableAbstractRendererPendingAddOperation,
      initialTableSize: number,
    ): TableAbstractRendererPendingAddOps => ({
      kind: "add",
      pending: List([v]),
      totalAdded: 1,
      initialTableSize,
    }),
    fromList: (
      pending: List<TableAbstractRendererPendingAddOperation>,
      initialTableSize: number,
    ): TableAbstractRendererPendingAddOps => ({
      kind: "add",
      pending,
      totalAdded: pending.size,
      initialTableSize,
    }),
  },
  Updaters: {
    Core: {
      ...simpleUpdater<TableAbstractRendererPendingAddOps>()("pending"),
      ...simpleUpdater<TableAbstractRendererPendingAddOps>()("totalAdded"),
    },
  },
};
