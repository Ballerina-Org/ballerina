import { CoTypedFactory } from "../../../../../../../../../main";
import { Co } from "../../../abstract-renderers/table/coroutines/builder";
import {
  DispatchEditFormLauncherContext,
  DispatchEditFormLauncherState,
  DispatchEditFormLauncherForeignMutationsExpected,
} from "../state";

export const DispatchEditFormRunner = <T, FS>() => {
  const CreateCo = CoTypedFactory<
    DispatchEditFormLauncherContext<T, FS>,
    DispatchEditFormLauncherState<T, FS>
  >();

  return CreateCo.Template<
    DispatchEditFormLauncherForeignMutationsExpected<T, FS>
  >(CreateCo.Repeat(CreateCo.Seq([CreateCo.Wait(2500)])), {
    runFilter: (_) => false,
  });
};
