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

export const DequeueRemoveOps = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>().While(
    ([current]) =>
      TableAbstractRendererPendingOps.Operations.dataHasBeenRemoved(
        current.value.data,
        current.customFormState.pendingOps,
      ),
    Co<CustomPresentationContext, ExtraContext>()
      .Wait(15)
      .then(() =>
        Co<CustomPresentationContext, ExtraContext>()
          .GetState()
          .then((current) => {
            if (current.customFormState.pendingOps.kind != "remove") {
              console.error(
                "Unexpected error: pending ops are not remove operations",
              );
              return Co<CustomPresentationContext, ExtraContext>().Return<Unit>(
                unit,
              );
            }

            const completedRemoveOps =
              TableAbstractRendererPendingOps.Operations.getCompletedRemoveOps(
                current.value.data,
                current.customFormState.pendingOps,
              );

            const remainingRemoveOperations =
              current.customFormState.pendingOps.pending
                .skip(completedRemoveOps.size)
                .takeWhile(
                  (v, k, iter) =>
                    k == 0 ||
                    JSON.stringify(v.flags) ==
                      JSON.stringify(iter.get(k - 1)!.flags),
                );

            if (remainingRemoveOperations.size > 0) {
              const removeBatchDelta = {
                kind: "TableRemoveBatch",
                ids: remainingRemoveOperations.map((_) => _.id).toArray(),
                flags: remainingRemoveOperations.first()!.flags,
                uniqueTableIdentifier: current.uniqueTableIdentifier,
                sourceAncestorLookupTypeNames: current.lookupTypeAncestorNames,
              };

              current.onChange(
                Option.Default.none(),
                removeBatchDelta as DispatchDelta<any>,
              );
            }

            return Co<CustomPresentationContext, ExtraContext>().SetState(
              TableAbstractRendererState.Updaters.Core.customFormState.children.pendingOps(
                TableAbstractRendererPendingOps.Updaters.Template.dequeuePendingRemoveOperations(
                  completedRemoveOps,
                ),
              ),
            );
          }),
      ),
  );
