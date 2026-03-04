import {
  Option,
  Unit,
  ValueTable,
  MapRepo,
  unit,
} from "../../../../../../../../../main";
import { ApplyEditsCo as Co } from "./builder";
import { TableAbstractRendererPendingOps } from "../domains/pending-operation/state";
import { TableAbstractRendererState } from "../state";

export const ApplyEdits = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>().Seq([
    Co<CustomPresentationContext, ExtraContext>()
      .GetState()
      .then((current) => {
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
  ]);
