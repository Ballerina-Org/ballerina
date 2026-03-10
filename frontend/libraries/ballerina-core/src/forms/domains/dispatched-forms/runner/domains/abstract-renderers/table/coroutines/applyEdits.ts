import {
  Option,
  Unit,
  ValueTable,
  MapRepo,
  unit,
  DispatchDelta,
} from "../../../../../../../../../main";
import { PendingOperationsCo as Co } from "./builder";
import { TableAbstractRendererPendingOps } from "../domains/pending-operation/state";
import { TableAbstractRendererState } from "../state";

export const ApplyEdits = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>().While(
    ([current]) =>
      TableAbstractRendererPendingOps.Operations.hasNewData(
        current.value.data,
        current.customFormState.pendingOps,
      ),

    Co<CustomPresentationContext, ExtraContext>()
      .GetState()
      .then((current) => {
        if (current.customFormState.pendingOps.kind != "add") {
          console.error("Unexpected error: pending ops are not add operations");
          return Co<CustomPresentationContext, ExtraContext>().Return<Unit>(
            unit,
          );
        }

        const newData =
          TableAbstractRendererPendingOps.Operations.getNewDataOrNone(
            current.value.data,
            current.customFormState.pendingOps,
          );

        if (newData.kind == "r") {
          // apply the correct edits to all the rows that have been added to the table
          // since the previously applied edits have been overwritten by the new data
          newData.value.forEach((op) => {
            op.editsToApply.forEach((edit) => {
              const rowId = current.value.data.keySeq().get(op.idx);
              if (rowId == undefined) {
                console.warn("Actual id is undefined for edit:", edit);
                return;
              }

              const updater = Option.Default.some(
                ValueTable.Updaters.data(
                  MapRepo.Updaters.update(rowId, edit.recordUpdater),
                ),
              );

              const correctRowIdDelta =
                edit.delta.kind == "TableValue"
                  ? {
                      ...edit.delta,
                      id: rowId,
                    }
                  : edit.delta;

              current.onChange(updater, correctRowIdDelta);
            });
          });

          // bundle together all the add operations that have the same flags
          const remainingAddOperations =
            current.customFormState.pendingOps.pending
              .skip(newData.value.size)
              .takeWhile(
                (v, k, iter) =>
                  k == 0 ||
                  // TODO: find a better way to compare flags
                  JSON.stringify(v.flags) ==
                    JSON.stringify(iter.get(k - 1)!.flags),
              );

          if (remainingAddOperations.size > 0) {
            const addBatchDelta = {
              kind: "TableAddBatch",
              count: remainingAddOperations.size,
              uniqueTableIdentifier: current.uniqueTableIdentifier,
              flags: remainingAddOperations.first()!.flags,
              sourceAncestorLookupTypeNames: current.lookupTypeAncestorNames,
            };

            current.onChange(
              Option.Default.none(),
              addBatchDelta as DispatchDelta<any>,
            );
          }

          return Co<CustomPresentationContext, ExtraContext>().SetState(
            TableAbstractRendererState.Updaters.Core.customFormState.children.pendingOps(
              TableAbstractRendererPendingOps.Updaters.Template.dequeuePendingAddOperations(
                newData.value.size,
              ),
            ),
          );
        }

        return Co<CustomPresentationContext, ExtraContext>().Return<Unit>(unit);
      }),
  );
