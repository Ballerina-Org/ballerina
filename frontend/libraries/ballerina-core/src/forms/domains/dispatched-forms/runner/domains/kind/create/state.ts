import {
  DispatchFormRunnerContext,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  CreateLauncherRef,
  Unit,
} from "../../../../../../../../main";

export type DispatchCreateFormLauncherContext<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
> = Omit<
  DispatchFormRunnerContext<T, Flags, CustomPresentationContexts, ExtraContext>,
  "launcherRef"
> & {
  launcherRef: CreateLauncherRef<Flags>;
};

export type DispatchCreateFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> = DispatchFormRunnerState<T, Flags>;

export type DispatchCreateFormLauncherForeignMutationsExpected<T> = {};
