import {
  DispatchFormRunnerContext,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  EditLauncherRef,
  Guid,
  ApiErrors,
  Unit,
  DispatchCommonFormRunnerState,
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
};

export type DispatchEditFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> = DispatchCommonFormRunnerState<T, Flags>;

export type DispatchEditFormLauncherForeignMutationsExpected<T> = {};

export const DispatchEditFormLauncherState = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
>() => ({
  Default: (): DispatchEditFormLauncherState<T, Flags> => ({
    ...DispatchCommonFormRunnerState<T, Flags>().Default(),
  }),
});
