import { CoTypedFactory } from "../../../../../../../../../main";
import { Co } from "../../../abstract-renderers/table/coroutines/builder";
import {
  DispatchCreateFormLauncherContext,
  DispatchCreateFormLauncherForeignMutationsExpected,
  DispatchCreateFormLauncherState,
} from "../state";

export const DispatchCreateFormRunner = <T, FS>() => {
  const CreateCo = CoTypedFactory<
    DispatchCreateFormLauncherContext<T, FS>,
    DispatchCreateFormLauncherState<T, FS>
  >();

  return CreateCo.Template<
    DispatchCreateFormLauncherForeignMutationsExpected<T, FS>
  >(CreateCo.Repeat(CreateCo.Seq([CreateCo.Wait(2500)])), {
    runFilter: (_) => false,
  });
};
