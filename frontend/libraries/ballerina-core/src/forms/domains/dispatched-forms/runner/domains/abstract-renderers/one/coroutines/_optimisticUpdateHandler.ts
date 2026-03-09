import {
  OnePendingOperation,
  replaceWith,
  OneAbstractRendererState,
  Sum,
  Unit,
  Option,
  unit,
  BaseFlags,
} from "../../../../../../../../../main";
import { OptimisticUpdateCo as Co } from "./builder";

export const optimisticUpdateHandler = <
  CustomPresentationContext = Unit,
  Flags extends BaseFlags = BaseFlags,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, Flags, ExtraContext>().While(
    ([current]) =>
      OneAbstractRendererState.Operations.ShouldProcessPendingOperation(
        current,
      ),
    Co<CustomPresentationContext, Flags, ExtraContext>()
      .GetState()
      .then((current) => {
        if (current.customFormState.pendingOperation.kind == "l") {
          return Co<CustomPresentationContext, Flags, ExtraContext>().Return(0);
        }

        current.customFormState.pendingOperation.value.run();

        return Co<CustomPresentationContext, Flags, ExtraContext>().SetState(
          OneAbstractRendererState.Updaters.Core.customFormState.children.pendingOperation(
            replaceWith(Sum.Default.left(unit)),
          ),
        );
      }),
  );
