import {
  CoTypedFactory,
  DispatchInjectablesTypes,
} from "../../../../../../../../../main";
import {
  DispatchEditFormLauncherContext,
  DispatchEditFormLauncherState,
  DispatchEditFormLauncherForeignMutationsExpected,
} from "../state";

export const DispatchEditFormRunner = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>() => {
  const CreateCo = CoTypedFactory<
    DispatchEditFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchEditFormLauncherState<T, Flags>
  >();

  return CreateCo.Template<DispatchEditFormLauncherForeignMutationsExpected<T>>(
    CreateCo.Repeat(CreateCo.Seq([CreateCo.Wait(2500)])),
    {
      runFilter: (_) => false,
    },
  );
};
