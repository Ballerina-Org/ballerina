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
  Flags,
  CustomPresentationContext,
  ExtraContext,
> = Omit<
  DispatchFormRunnerContext<T, Flags, CustomPresentationContext, ExtraContext>,
  "launcherRef"
> & {
  launcherRef: EditLauncherRef;
};

export type DispatchEditFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
> = DispatchCommonFormRunnerState<
  T,
  Flags,
  CustomPresentationContext,
  ExtraContext
> & {
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
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() => ({
  Default: (): DispatchEditFormLauncherState<
    T,
    Flags,
    CustomPresentationContext,
    ExtraContext
  > => ({
    ...DispatchCommonFormRunnerState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >().Default(),
    entity: Synchronized.Default(unit),
    config: Synchronized.Default(unit),
    apiChecker: {
      init: ApiResponseChecker.Default(false),
      update: ApiResponseChecker.Default(false),
    },
    apiRunner: Synchronized.Default(unit),
    formName: "",
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("status"),
      ...simpleUpdater<
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("formState"),
      ...simpleUpdater<
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("entity"),
      ...simpleUpdater<
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("config"),
      ...simpleUpdater<
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("apiRunner"),
      ...simpleUpdater<
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("formName"),
      ...simpleUpdaterWithChildren<
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()({
        ...simpleUpdater<
          DispatchEditFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >["apiChecker"]
        >()("init"),
        ...simpleUpdater<
          DispatchEditFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >["apiChecker"]
        >()("update"),
      })("apiChecker"),
    },
    Template: {
      entity: (
        _: BasicUpdater<PredicateValue>,
      ): Updater<
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      > =>
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >().Updaters.Core.entity(
          Synchronized.Updaters.sync(AsyncState.Operations.map(_)),
        ),
      submit: (): Updater<
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      > =>
        DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >().Updaters.Core.apiRunner(
          Synchronized.Updaters.sync(AsyncState.Updaters.toLoading()),
        ),
    },
  },
});
