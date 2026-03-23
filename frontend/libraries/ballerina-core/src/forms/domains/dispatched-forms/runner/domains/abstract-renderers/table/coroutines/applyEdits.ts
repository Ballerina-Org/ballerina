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
              kind: "TableAddBatchEmpty",
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
