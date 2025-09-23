import {
  DispatchFormRunnerContext,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  EditLauncherRef,
  Unit,
} from "../../../../../../../../main";

export type DispatchEditFormLauncherContext<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
> = Omit<
  DispatchFormRunnerContext<T, Flags, CustomPresentationContexts, ExtraContext>,
  "launcherRef"
> & {
  launcherRef: EditLauncherRef<Flags>;
};

export type DispatchEditFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> = DispatchFormRunnerState<T, Flags>;

export type DispatchEditFormLauncherForeignMutationsExpected<T> = {};
