import {
  CoTypedFactory,
  DispatchInjectablesTypes,
} from "../../../../../../../../../main";
import {
  DispatchEditFormLauncherContext,
  DispatchEditFormLauncherState,
} from "../state";

export const EditCoBuilder = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() =>
  CoTypedFactory<
    DispatchEditFormLauncherContext<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    DispatchEditFormLauncherState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >
  >();
