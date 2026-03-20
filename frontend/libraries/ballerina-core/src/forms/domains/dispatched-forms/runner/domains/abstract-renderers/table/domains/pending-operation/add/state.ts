import { List } from "immutable";
import { PendingEdit } from "../common";
import { simpleUpdater, Updater, ValueRecord } from "ballerina-core";

export type PendingAddOperationId = `placeholder-${string}`;
export type TableAbstractRendererPendingAddOperation = {
  idx: number;
  id: PendingAddOperationId;
  record: ValueRecord;
  editsToApply: List<PendingEdit<any>>;
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
    editsToApply: List(),
    flags,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<TableAbstractRendererPendingAddOperation>()("record"),
      ...simpleUpdater<TableAbstractRendererPendingAddOperation>()(
        "editsToApply",
      ),
    },
  },
};

export type TableAbstractRendererPendingAddOps = {
  kind: "add";
  pending: List<TableAbstractRendererPendingAddOperation>;
};
export const TableAbstractRendererPendingAddOps = {
  Default: {
    empty: (): TableAbstractRendererPendingAddOps => ({
      kind: "add",
      pending: List(),
    }),
    singleton: (
      v: TableAbstractRendererPendingAddOperation,
    ): TableAbstractRendererPendingAddOps => ({
      kind: "add",
      pending: List([v]),
    }),
    fromList: (
      pending: List<TableAbstractRendererPendingAddOperation>,
    ): TableAbstractRendererPendingAddOps => ({
      kind: "add",
      pending,
    }),
  },
  Updaters: {
    Core: {
      ...simpleUpdater<TableAbstractRendererPendingAddOps>()("pending"),
    },
  },
};
