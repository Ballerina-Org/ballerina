import {
  DispatchCreateFormLauncherContext,
  DispatchCreateFormLauncherState,
  DispatchCreateFormLauncherForeignMutationsExpected,
} from "./state";
import { Template } from "../../../../../../../../main";
import React from "react";
import { DispatchCreateFormRunner } from "./coroutines/runner";

export const DispatchCreateFormLauncherTemplate = <T, FS>() =>
  Template.Default<
    DispatchCreateFormLauncherContext<T, FS>,
    DispatchCreateFormLauncherState<T, FS>,
    DispatchCreateFormLauncherForeignMutationsExpected<T, FS>
  >((props) => {
    return <>create</>;
  }).any([DispatchCreateFormRunner<T, FS>()]);
