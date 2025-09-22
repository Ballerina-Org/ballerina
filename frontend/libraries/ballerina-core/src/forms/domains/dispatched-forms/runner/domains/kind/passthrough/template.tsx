import {
  DispatchPassthroughFormLauncherContext,
  DispatchPassthroughFormLauncherState,
  DispatchPassthroughFormLauncherForeignMutationsExpected,
} from "./state";
import { Template } from "../../../../../../../../main";
import React from "react";
import { DispatchPassthroughFormRunner } from "./coroutines/runner";

export const DispatchPassthroughFormLauncherTemplate = <T,>() =>
  Template.Default<
    DispatchPassthroughFormLauncherContext<T>,
    DispatchPassthroughFormLauncherState<T>,
    DispatchPassthroughFormLauncherForeignMutationsExpected<T>
  >((props) => {
    return <>passthrough</>;
  }).any([DispatchPassthroughFormRunner<T>()]);
