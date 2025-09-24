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
  CustomPresentationContexts,
  ExtraContext,
>() =>
  CoTypedFactory<
    DispatchCreateFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchCreateFormLauncherState<T, Flags>
  >();
