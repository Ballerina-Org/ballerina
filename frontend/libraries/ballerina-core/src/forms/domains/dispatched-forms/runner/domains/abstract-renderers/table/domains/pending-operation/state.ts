import {
  DispatchDelta,
  ListRepo,
  Option,
  Updater,
  ValueRecord,
  ValueTable,
  replaceWith,
} from "ballerina-core";
import { List, OrderedMap } from "immutable";
import {
  TableAbstractRendererPendingAddOperation,
  TableAbstractRendererPendingAddOps,
} from "./add/state";
import { TableAbstractRendererNoPendingOps } from "./empty/state";
import { TableAbstractRendererPendingRemoveOps } from "./remove/state";

export type TableAbstractRendererPendingOps =
  | TableAbstractRendererNoPendingOps
  | TableAbstractRendererPendingAddOps
  | TableAbstractRendererPendingRemoveOps;

export const TableAbstractRendererPendingOps = {
  Default: {
    empty: (): TableAbstractRendererPendingOps => ({ kind: "empty" }),
    add: (
      pending: TableAbstractRendererPendingAddOps["pending"],
    ): TableAbstractRendererPendingOps =>
      TableAbstractRendererPendingAddOps.Default.fromList(pending),
    remove: (
      pending: TableAbstractRendererPendingRemoveOps["pending"],
    ): TableAbstractRendererPendingOps =>
      TableAbstractRendererPendingRemoveOps.Default(pending),
  },
  Updaters: {
    Core: {
      add: (
        updater: Updater<TableAbstractRendererPendingAddOps>,
      ): Updater<TableAbstractRendererPendingOps> =>
        Updater((_) => (_.kind == "add" ? updater(_) : _)),
      pendingAddOperations: (
        pending: TableAbstractRendererPendingAddOps["pending"],
      ): Updater<TableAbstractRendererPendingOps> =>
        Updater((_) =>
          _.kind == "add"
            ? TableAbstractRendererPendingAddOps.Updaters.Core.pending((_) =>
                _.concat(pending),
              )(_)
            : TableAbstractRendererPendingOps.Updaters.Template.toAdd(pending)(
                _,
              ),
        ),
      pendingRemoveOperations: (
        pending: TableAbstractRendererPendingRemoveOps["pending"],
      ): Updater<TableAbstractRendererPendingOps> =>
        Updater((_) =>
          _.kind == "remove"
            ? TableAbstractRendererPendingRemoveOps.Updaters.Core.pending((_) =>
                _.concat(pending),
              )(_)
            : TableAbstractRendererPendingOps.Updaters.Template.toRemove(
                pending,
              )(_),
        ),
    },
    Template: {
      toAdd: (
        pending: TableAbstractRendererPendingAddOps["pending"],
      ): Updater<TableAbstractRendererPendingOps> =>
        replaceWith(TableAbstractRendererPendingOps.Default.add(pending)),
      toRemove: (
        pending: TableAbstractRendererPendingRemoveOps["pending"],
      ): Updater<TableAbstractRendererPendingOps> =>
        replaceWith(TableAbstractRendererPendingOps.Default.remove(pending)),
      enqueuePendingAddOperation: (
        pending: TableAbstractRendererPendingAddOps["pending"],
      ): Updater<TableAbstractRendererPendingOps> =>
        Updater((_) =>
          _.kind == "add" ? { ..._, pending: _.pending.concat(pending) } : _,
        ),
      dequeuePendingAddOperations: (
        count: number,
      ): Updater<TableAbstractRendererPendingOps> =>
        Updater((_) =>
          _.kind == "add"
            ? _.pending.size <= count
              ? TableAbstractRendererPendingOps.Default.empty()
              : { ..._, pending: _.pending.skip(count) }
            : _,
        ),
      dequeuePendingRemoveOperations: (
        count: number,
      ): Updater<TableAbstractRendererPendingOps> =>
        Updater((_) =>
          _.kind == "remove"
            ? _.pending.size <= count
              ? TableAbstractRendererPendingOps.Default.empty()
              : { ..._, pending: _.pending.skip(count) }
            : _,
        ),
      enqueuePendingRemoveOperation: (
        pending: TableAbstractRendererPendingRemoveOps["pending"],
      ): Updater<TableAbstractRendererPendingOps> =>
        Updater((_) =>
          _.kind == "remove" ? { ..._, pending: _.pending.concat(pending) } : _,
        ),
    },
  },
  Operations: {
    canEnqueueAddOperation: (
      pendingOps: TableAbstractRendererPendingOps,
    ): boolean => pendingOps.kind == "add" || pendingOps.kind == "empty",
    canEnqueueRemoveOperation: (
      pendingOps: TableAbstractRendererPendingOps,
    ): boolean => pendingOps.kind == "remove" || pendingOps.kind == "empty",
    /// Returns true if there are any pending add operations that have been added to the table
    hasNewData: (
      data: OrderedMap<string, ValueRecord>,
      pendingOps: TableAbstractRendererPendingOps,
    ): pendingOps is TableAbstractRendererPendingAddOps =>
      pendingOps.kind == "add" &&
      pendingOps.pending.size > 0 &&
      data.size > pendingOps.pending.first()!.idx,
    /// Returns a list of pending add operations that have completed (got the response)
    getNewDataOrNone: (
      data: OrderedMap<string, ValueRecord>,
      pendingOps: TableAbstractRendererPendingOps,
    ): Option<List<TableAbstractRendererPendingAddOperation>> =>
      TableAbstractRendererPendingOps.Operations.hasNewData(data, pendingOps)
        ? Option.Default.some(
            List(pendingOps.pending.takeWhile((_) => data.size > _.idx)),
          )
        : Option.Default.none(),
  },
};
