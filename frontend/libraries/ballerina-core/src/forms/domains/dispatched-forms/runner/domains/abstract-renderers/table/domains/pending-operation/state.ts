import {
  BasicFun,
  BasicUpdater,
  DispatchDelta,
  ListRepo,
  Option,
  TableAbstractRendererReadonlyContext,
  TableAbstractRendererState,
  TableAbstractRendererForeignMutationsExpected,
  Unit,
  Updater,
  ValueRecord,
  ValueTable,
  id,
  replaceWith,
} from "ballerina-core";
import { List, OrderedMap } from "immutable";
import {
  TableAbstractRendererPendingAddOperation,
  TableAbstractRendererPendingAddOps,
} from "./add/state";
import { TableAbstractRendererNoPendingOps } from "./empty/state";
import {
  TableAbstractRendererPendingRemoveOperation,
  TableAbstractRendererPendingRemoveOps,
} from "./remove/state";

export type TableAbstractRendererPendingOps =
  | TableAbstractRendererNoPendingOps
  | TableAbstractRendererPendingAddOps
  | TableAbstractRendererPendingRemoveOps;

export const TableAbstractRendererPendingOps = {
  Default: {
    empty: (): TableAbstractRendererPendingOps =>
      TableAbstractRendererNoPendingOps.Default(),
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
            : replaceWith(TableAbstractRendererPendingOps.Default.add(pending))(
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
            : replaceWith(
                TableAbstractRendererPendingOps.Default.remove(pending),
              )(_),
        ),
    },
    Template: {
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
        operationIds: TableAbstractRendererPendingRemoveOps["pending"],
      ): Updater<TableAbstractRendererPendingOps> =>
        Updater((_) =>
          _.kind == "remove"
            ? _.pending.size <= operationIds.size
              ? TableAbstractRendererPendingOps.Default.empty()
              : {
                  ..._,
                  pending: _.pending.filterNot((_) =>
                    operationIds.some((o) => o.id == _.id),
                  ),
                }
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
    dataHasBeenRemoved: (
      data: OrderedMap<string, ValueRecord>,
      pendingOps: TableAbstractRendererPendingOps,
    ): boolean =>
      pendingOps.kind == "remove" &&
      pendingOps.pending.some((_) => !data.has(_.id)),
    getCompletedRemoveOps: (
      data: OrderedMap<string, ValueRecord>,
      pendingOps: TableAbstractRendererPendingOps,
    ): List<TableAbstractRendererPendingRemoveOperation> =>
      pendingOps.kind == "remove"
        ? pendingOps.pending.filterNot((_) => data.has(_.id))
        : List(),
    optimisticUpdate:
      <
        CusomtPresentationContext = Unit,
        Flags = Unit,
        ExtraContext = Unit,
      >(props: {
        context: TableAbstractRendererReadonlyContext<
          CusomtPresentationContext,
          ExtraContext
        > &
          TableAbstractRendererState;
        setState: BasicFun<BasicUpdater<TableAbstractRendererState>, void>;
        foreignMutations: TableAbstractRendererForeignMutationsExpected<Flags>;
      }) =>
      (idx: number, valueRecordUpdater: Updater<ValueRecord>) =>
      (
        updater: Option<BasicUpdater<ValueTable>>,
        delta: DispatchDelta<Flags>,
      ) => {
        // custom onChange function provided to the cell templates to handle updates on "fake" rows
        if (props.context.customFormState.pendingOps.kind == "add") {
          // if the index is part of the extra data
          // apply the updater to the extra data

          const extraDataIdx =
            idx - props.context.customFormState.pendingOps.pending.first()!.idx;

          if (
            extraDataIdx >= 0 &&
            extraDataIdx < props.context.customFormState.pendingOps.pending.size
          ) {
            const addEditToApplyUpd =
              TableAbstractRendererState.Updaters.Core.customFormState.children.pendingOps(
                TableAbstractRendererPendingOps.Updaters.Core.add(
                  TableAbstractRendererPendingAddOps.Updaters.Core.pending(
                    ListRepo.Updaters.update(
                      extraDataIdx,
                      Updater<TableAbstractRendererPendingAddOperation>(
                        (_) => ({
                          ..._,
                          // update the record with the new value
                          record: valueRecordUpdater(_.record),
                          // store information about the edit to apply it later
                          // once the real row is added to the table
                          editsToApply: _.editsToApply.push({
                            recordUpdater: valueRecordUpdater,
                            updater:
                              updater.kind == "l"
                                ? Updater(id)
                                : Updater(updater.value),
                            delta,
                          }),
                        }),
                      ),
                    ),
                  ),
                ),
              );

            props.setState(addEditToApplyUpd);
          } else {
            // index referes to a row in the table data
            // so we can apply the updater to the row directly
            // but only locally
            props.foreignMutations.onChange(updater, {
              ...delta,
              flags: { kind: "localOnly" } as Flags,
            });
          }

          return;
        }

        props.foreignMutations.onChange(updater, delta);
      },
  },
};

export const PendingOps = {
  Operations: {
    getPendingAddIds: (
      pendingOps: TableAbstractRendererPendingOps,
    ): List<string> =>
      pendingOps.kind == "add" ? pendingOps.pending.map((_) => _.id) : List(),
    getPendingRemoveIds: (
      pendingOps: TableAbstractRendererPendingOps,
    ): List<string> =>
      pendingOps.kind == "remove"
        ? pendingOps.pending.map((_) => _.id)
        : List(),
  },
};
