import {
  CoTypedFactory,
  DispatchInjectablesTypes,
} from "../../../../../../../../../main";
import {
  DispatchCreateFormLauncherContext,
  DispatchCreateFormLauncherForeignMutationsExpected,
  DispatchCreateFormLauncherState,
} from "../state";

export const DispatchCreateFormRunner = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>() => {
  const CreateCo = CoTypedFactory<
    DispatchCreateFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchCreateFormLauncherState<T, Flags>
  >();

  return CreateCo.Template<
    DispatchCreateFormLauncherForeignMutationsExpected<T>
  >(CreateCo.Repeat(CreateCo.Seq([CreateCo.Wait(2500)])), {
    runFilter: (_) => false,
  });
};
