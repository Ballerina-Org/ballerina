import {
  DispatchCreateFormLauncherContext,
  DispatchCreateFormLauncherState,
  DispatchCreateFormLauncherForeignMutationsExpected,
} from "./state";
import { Template } from "../../../../../../../../main";
import React from "react";
import { DispatchCreateFormRunner } from "./coroutines/runner";

export const DispatchCreateFormLauncherTemplate = <T,>() =>
  Template.Default<
    DispatchCreateFormLauncherContext<T>,
    DispatchCreateFormLauncherState<T>,
    DispatchCreateFormLauncherForeignMutationsExpected<T>
  >((props) => {
    return <>create</>;
  }).any([DispatchCreateFormRunner<T>()]);
