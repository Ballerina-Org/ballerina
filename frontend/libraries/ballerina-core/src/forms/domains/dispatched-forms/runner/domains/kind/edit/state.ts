import {
  DispatchFormRunnerContext,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  EditLauncherRef,
  Guid,
  ApiErrors,
  Unit,
} from "../../../../../../../../main";

export type DispatchEditFormLauncherApi = {
  get: (id: Guid) => Promise<any>;
  update: (id: Guid, raw: any) => Promise<ApiErrors>;
  getGlobalConfiguration: () => Promise<any>;
};

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
  api?: DispatchEditFormLauncherApi;
};

export type DispatchEditFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> = DispatchFormRunnerState<T, Flags>;

export type DispatchEditFormLauncherForeignMutationsExpected<T> = {};
