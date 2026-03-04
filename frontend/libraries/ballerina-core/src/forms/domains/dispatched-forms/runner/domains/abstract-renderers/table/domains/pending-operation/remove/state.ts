import { List } from "immutable";
import { simpleUpdater } from "ballerina-core";

export type TableAbstractRendererPendingRemoveOperation = {
  id: string;
};

export type TableAbstractRendererPendingRemoveOps = {
  kind: "remove";
  pending: List<TableAbstractRendererPendingRemoveOperation>;
};
export const TableAbstractRendererPendingRemoveOps = {
  Default: (
    pending: List<TableAbstractRendererPendingRemoveOperation>,
  ): TableAbstractRendererPendingRemoveOps => ({
    kind: "remove",
    pending,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<TableAbstractRendererPendingRemoveOps>()("pending"),
    },
  },
};
