import {
  DispatchEditFormLauncherContext,
  DispatchEditFormLauncherState,
  DispatchEditFormLauncherForeignMutationsExpected,
} from "./state";
import {
  DispatchInjectablesTypes,
  Template,
  Unit,
} from "../../../../../../../../main";
import React from "react";
import { DispatchEditFormRunner } from "./coroutines/runner";

export const DispatchEditFormLauncherTemplate = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>() =>
  Template.Default<
    DispatchEditFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchEditFormLauncherState<T, Flags>,
    DispatchEditFormLauncherForeignMutationsExpected<T>
  >((props) => {
    return <>edit</>;
  }).any([
    DispatchEditFormRunner<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >(),
  ]);
