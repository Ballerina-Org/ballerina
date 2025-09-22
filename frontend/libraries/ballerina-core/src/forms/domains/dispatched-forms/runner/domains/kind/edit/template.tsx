import {
  DispatchEditFormLauncherContext,
  DispatchEditFormLauncherState,
  DispatchEditFormLauncherForeignMutationsExpected,
} from "./state";
import { Template } from "../../../../../../../../main";
import React from "react";
import { DispatchEditFormRunner } from "./coroutines/runner";

export const DispatchEditFormLauncherTemplate = <T,>() =>
  Template.Default<
    DispatchEditFormLauncherContext<T>,
    DispatchEditFormLauncherState<T>,
    DispatchEditFormLauncherForeignMutationsExpected<T>
  >((props) => {
    return <>edit</>;
  }).any([DispatchEditFormRunner<T>()]);
