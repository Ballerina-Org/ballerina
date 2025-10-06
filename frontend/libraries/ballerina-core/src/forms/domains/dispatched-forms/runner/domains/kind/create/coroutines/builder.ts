import {
  CoTypedFactory,
  DispatchInjectablesTypes,
} from "../../../../../../../../../main";
import {
  DispatchCreateFormLauncherContext,
  DispatchCreateFormLauncherState,
} from "../state";

export const CreateCoBuilder = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() =>
  CoTypedFactory<
    DispatchCreateFormLauncherContext<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    DispatchCreateFormLauncherState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >
  >();
