import { CoTypedFactory } from "../../../../../../../../../main";
import { Co } from "../../../abstract-renderers/table/coroutines/builder";
import {
  DispatchEditFormLauncherContext,
  DispatchEditFormLauncherState,
  DispatchEditFormLauncherForeignMutationsExpected,
} from "../state";

export const DispatchEditFormRunner = <T>() => {
  const CreateCo = CoTypedFactory<
    DispatchEditFormLauncherContext<T>,
    DispatchEditFormLauncherState<T>
  >();

  return CreateCo.Template<
    DispatchEditFormLauncherForeignMutationsExpected<T>
  >(CreateCo.Repeat(CreateCo.Seq([CreateCo.Wait(2500)])), {
    runFilter: (_) => false,
  });
};
