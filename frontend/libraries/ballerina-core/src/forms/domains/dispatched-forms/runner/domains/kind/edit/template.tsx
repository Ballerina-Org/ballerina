import {
  DispatchEditFormLauncherContext,
  DispatchEditFormLauncherState,
  DispatchEditFormLauncherForeignMutationsExpected,
} from "./state";
import { Template } from "../../../../../../../../main";
import React from "react";
import { DispatchEditFormRunner } from "./coroutines/runner";

export const DispatchEditFormLauncherTemplate = <T, FS>() =>
  Template.Default<
    DispatchEditFormLauncherContext<T, FS>,
    DispatchEditFormLauncherState<T, FS>,
    DispatchEditFormLauncherForeignMutationsExpected<T, FS>
  >((props) => {
    return <>edit</>;
  }).any([DispatchEditFormRunner<T, FS>()]);
