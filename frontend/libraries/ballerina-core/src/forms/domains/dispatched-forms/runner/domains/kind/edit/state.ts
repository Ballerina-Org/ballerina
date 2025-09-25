import {
  DispatchFormRunnerContext,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  EditLauncherRef,
  Guid,
  ApiErrors,
  Unit,
  DispatchCommonFormRunnerState,
  ApiResponseChecker,
  Synchronized,
  unit,
  simpleUpdaterWithChildren,
  simpleUpdater,
  PredicateValue,
  BasicUpdater,
  Updater,
  AsyncState,
} from "../../../../../../../../main";

export type DispatchEditFormLauncherApi = {
  get: (id: Guid) => Promise<any>;
  update: (id: Guid, raw: any) => Promise<ApiErrors>;
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
> = DispatchCommonFormRunnerState<T, Flags> & {
  entity: Synchronized<Unit, PredicateValue>;
  config: Synchronized<Unit, PredicateValue>;
  apiChecker: {
    init: ApiResponseChecker;
    update: ApiResponseChecker;
  };
  apiRunner: Synchronized<Unit, ApiErrors>;
};

export type DispatchEditFormLauncherForeignMutationsExpected<T> = {};

export const DispatchEditFormLauncherState = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
>() => ({
  Default: (): DispatchEditFormLauncherState<T, Flags> => ({
    ...DispatchCommonFormRunnerState<T, Flags>().Default(),
    entity: Synchronized.Default(unit),
    config: Synchronized.Default(unit),
    apiChecker: {
      init: ApiResponseChecker.Default(false),
      update: ApiResponseChecker.Default(false),
    },
    apiRunner: Synchronized.Default(unit),
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<DispatchEditFormLauncherState<T, Flags>>()("status"),
      ...simpleUpdater<DispatchEditFormLauncherState<T, Flags>>()("formState"),
      ...simpleUpdater<DispatchEditFormLauncherState<T, Flags>>()("entity"),
      ...simpleUpdater<DispatchEditFormLauncherState<T, Flags>>()("config"),
      ...simpleUpdater<DispatchEditFormLauncherState<T, Flags>>()("apiRunner"),
      ...simpleUpdaterWithChildren<DispatchEditFormLauncherState<T, Flags>>()({
        ...simpleUpdater<
          DispatchEditFormLauncherState<T, Flags>["apiChecker"]
        >()("init"),
        ...simpleUpdater<
          DispatchEditFormLauncherState<T, Flags>["apiChecker"]
        >()("update"),
      })("apiChecker"),
    },
    Template: {
      entity: (
        _: BasicUpdater<PredicateValue>,
      ): Updater<DispatchEditFormLauncherState<T, Flags>> =>
        DispatchEditFormLauncherState<T, Flags>().Updaters.Core.entity(
          Synchronized.Updaters.sync(AsyncState.Operations.map(_)),
        ),
      submit: (): Updater<DispatchEditFormLauncherState<T, Flags>> =>
        DispatchEditFormLauncherState<T, Flags>().Updaters.Core.apiRunner(
          Synchronized.Updaters.sync(AsyncState.Updaters.toLoading()),
        ),
    },
  },
});
