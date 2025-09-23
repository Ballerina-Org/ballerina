import {
  DispatchFormRunnerContext,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  CreateLauncherRef,
  Unit,
  ApiErrors,
} from "../../../../../../../../main";

export type DispatchCreateFormLauncherApi = {
  default: () => Promise<any>;
  create: (raw: any) => Promise<ApiErrors>;
  getGlobalConfiguration: () => Promise<any>;
};

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
  api?: DispatchCreateFormLauncherApi;
};

export type DispatchCreateFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> = DispatchFormRunnerState<T, Flags>;

export type DispatchCreateFormLauncherForeignMutationsExpected<T> = {};
