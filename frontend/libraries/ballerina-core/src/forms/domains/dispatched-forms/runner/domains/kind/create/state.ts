import {
  DispatchFormRunnerContext,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  CreateLauncherRef,
  Unit,
  ApiErrors,
  Synchronized,
  PredicateValue,
  unit,
  simpleUpdater,
  ApiResponseChecker,
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
};

export type DispatchCreateFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> = DispatchFormRunnerState<T, Flags> & {
  entity: Synchronized<Unit, PredicateValue>;
  initApiChecker: ApiResponseChecker;
};

export const DispatchCreateFormLauncherState = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
>() => ({
  Default: (): DispatchCreateFormLauncherState<T, Flags> => ({
    ...DispatchFormRunnerState<T, Flags>().Default(),
    entity: Synchronized.Default(unit),
    initApiChecker: ApiResponseChecker.Default(false),
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<DispatchCreateFormLauncherState<T, Flags>>()("status"),
      ...simpleUpdater<DispatchCreateFormLauncherState<T, Flags>>()(
        "formState",
      ),
      ...simpleUpdater<DispatchCreateFormLauncherState<T, Flags>>()("entity"),
      ...simpleUpdater<DispatchCreateFormLauncherState<T, Flags>>()(
        "initApiChecker",
      ),
    },
  },
});

export type DispatchCreateFormLauncherForeignMutationsExpected<T> = {};
