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
  DispatchCommonFormRunnerState,
  AsyncState,
  BasicUpdater,
  Updater,
  Debounced,
  simpleUpdaterWithChildren,
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
> = DispatchCommonFormRunnerState<T, Flags> & {
  entity: Synchronized<Unit, PredicateValue>;
  config: Synchronized<Unit, PredicateValue>;
  apiChecker: {
    init: ApiResponseChecker;
    create: ApiResponseChecker;
  };
  apiRunner: Debounced<Synchronized<Unit, ApiErrors>>;
};

export const DispatchCreateFormLauncherState = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
>() => ({
  Default: (): DispatchCreateFormLauncherState<T, Flags> => ({
    ...DispatchCommonFormRunnerState<T, Flags>().Default(),
    entity: Synchronized.Default(unit),
    config: Synchronized.Default(unit),
    apiChecker: {
      init: ApiResponseChecker.Default(false),
      create: ApiResponseChecker.Default(false),
    },
    apiRunner: Debounced.Default(Synchronized.Default(unit)),
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<DispatchCreateFormLauncherState<T, Flags>>()("status"),
      ...simpleUpdater<DispatchCreateFormLauncherState<T, Flags>>()(
        "formState",
      ),
      ...simpleUpdater<DispatchCreateFormLauncherState<T, Flags>>()("entity"),
      ...simpleUpdater<DispatchCreateFormLauncherState<T, Flags>>()("config"),
      ...simpleUpdaterWithChildren<DispatchCreateFormLauncherState<T, Flags>>()(
        {
          ...simpleUpdater<
            DispatchCreateFormLauncherState<T, Flags>["apiChecker"]
          >()("init"),
          ...simpleUpdater<
            DispatchCreateFormLauncherState<T, Flags>["apiChecker"]
          >()("create"),
        },
      )("apiChecker"),
    },
    Template: {
      entity: (
        _: BasicUpdater<PredicateValue>,
      ): Updater<DispatchCreateFormLauncherState<T, Flags>> =>
        DispatchCreateFormLauncherState<T, Flags>().Updaters.Core.entity(
          Synchronized.Updaters.sync(AsyncState.Operations.map(_)),
        ),
    },
  },
});

export type DispatchCreateFormLauncherForeignMutationsExpected<T> = {};
