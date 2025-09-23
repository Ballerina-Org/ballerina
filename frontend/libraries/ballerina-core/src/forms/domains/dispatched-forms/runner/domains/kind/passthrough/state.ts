import {
  DispatchFormRunnerContext,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  PassthroughLauncherRef,
  Unit,
} from "../../../../../../../../main";

export type DispatchPassthroughFormLauncherContext<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
> = Omit<
  DispatchFormRunnerContext<T, Flags, CustomPresentationContexts, ExtraContext>,
  "launcherRef"
> & {
  launcherRef: PassthroughLauncherRef<Flags>;
};

export type DispatchPassthroughFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> = DispatchFormRunnerState<T, Flags>;

export type DispatchPassthroughFormLauncherForeignMutationsExpected<T> = {};
