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
  CustomPresentationContexts,
  ExtraContext,
>() =>
  CoTypedFactory<
    DispatchEditFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchEditFormLauncherState<T, Flags>
  >();
