import { CoTypedFactory } from "../../../../../../../../../main";
import { Co } from "../../../abstract-renderers/table/coroutines/builder";
import {
  DispatchCreateFormLauncherContext,
  DispatchCreateFormLauncherForeignMutationsExpected,
  DispatchCreateFormLauncherState,
} from "../state";

export const DispatchCreateFormRunner = <T>() => {
  const CreateCo = CoTypedFactory<
    DispatchCreateFormLauncherContext<T>,
    DispatchCreateFormLauncherState<T>
  >();

  return CreateCo.Template<
    DispatchCreateFormLauncherForeignMutationsExpected<T>
  >(CreateCo.Repeat(CreateCo.Seq([CreateCo.Wait(2500)])), {
    runFilter: (_) => false,
  });
};
