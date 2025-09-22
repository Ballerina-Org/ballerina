import { CoTypedFactory } from "../../../../../../../../../main";
import {
  DispatchPassthroughFormLauncherContext,
  DispatchPassthroughFormLauncherState,
  DispatchPassthroughFormLauncherForeignMutationsExpected,
} from "../state";

export const DispatchPassthroughFormRunner = <T>() => {
  const CreateCo = CoTypedFactory<
    DispatchPassthroughFormLauncherContext<T>,
    DispatchPassthroughFormLauncherState<T>
  >();

  return CreateCo.Template<
    DispatchPassthroughFormLauncherForeignMutationsExpected<T>
  >(CreateCo.Repeat(CreateCo.Seq([CreateCo.Wait(2500)])), {
    runFilter: (_) => false,
  });
};
