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
  const Co = CoTypedFactory<
    DispatchCreateFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchCreateFormLauncherState<T, Flags>
  >();

  return Co.Template<DispatchCreateFormLauncherForeignMutationsExpected<T>>(
    Co.GetState().then((current) =>
      Co.Do(() => {
        console.log("DispatchCreateFormRunner", current);
      }),
    ),
    {
      runFilter: (_) => false,
    },
  );
};
