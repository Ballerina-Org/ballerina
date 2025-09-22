import {
  CoTypedFactory,
  DispatchInjectablesTypes,
  Unit,
} from "../../../../../../../../../main";
import {
  DispatchPassthroughFormLauncherContext,
  DispatchPassthroughFormLauncherState,
  DispatchPassthroughFormLauncherForeignMutationsExpected,
} from "../state";

export const DispatchPassthroughFormRunner = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
>() => {
  const CreateCo = CoTypedFactory<
    DispatchPassthroughFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchPassthroughFormLauncherState<T>
  >();

  return CreateCo.Template<
    DispatchPassthroughFormLauncherForeignMutationsExpected<T>
  >(CreateCo.Repeat(CreateCo.Seq([CreateCo.Wait(2500)])), {
    runFilter: (_) => false,
  });
};
