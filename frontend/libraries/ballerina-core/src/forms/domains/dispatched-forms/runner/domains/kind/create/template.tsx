import {
  DispatchCreateFormLauncherContext,
  DispatchCreateFormLauncherState,
  DispatchCreateFormLauncherForeignMutationsExpected,
} from "./state";
import {
  DispatchInjectablesTypes,
  Template,
  Unit,
} from "../../../../../../../../main";
import { DispatchCreateFormRunner } from "./coroutines/runner";

export const DispatchCreateFormLauncherTemplate = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>() =>
  Template.Default<
    DispatchCreateFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchCreateFormLauncherState<T, Flags>,
    DispatchCreateFormLauncherForeignMutationsExpected<T>
  >((props) => {
    return <>create</>;
  }).any([
    DispatchCreateFormRunner<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >(),
  ]);
