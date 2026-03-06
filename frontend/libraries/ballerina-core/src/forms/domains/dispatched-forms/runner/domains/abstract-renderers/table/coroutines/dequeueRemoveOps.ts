import {
  Option,
  Unit,
  ValueTable,
  MapRepo,
  unit,
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
            const completedRemoveOps =
              TableAbstractRendererPendingOps.Operations.getCompletedRemoveOps(
                current.value.data,
                current.customFormState.pendingOps,
              );

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
