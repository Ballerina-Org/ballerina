import {
  DispatchFormRunnerContext,
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
  simpleUpdaterWithChildren,
} from "../../../../../../../../main";

export type DispatchCreateFormLauncherApi = {
  default: () => Promise<any>;
  create: (raw: any) => Promise<ApiErrors>;
};

export type DispatchCreateFormLauncherContext<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
> = Omit<
  DispatchFormRunnerContext<T, Flags, CustomPresentationContext, ExtraContext>,
  "launcherRef"
> & {
  launcherRef: CreateLauncherRef;
};

export type DispatchCreateFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext = Unit,
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
    create: ApiResponseChecker;
  };
  apiRunner: Synchronized<Unit, ApiErrors>;
};

export const DispatchCreateFormLauncherState = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() => ({
  Default: (): DispatchCreateFormLauncherState<
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
      create: ApiResponseChecker.Default(false),
    },
    apiRunner: Synchronized.Default(unit),
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<
        DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("status"),
      ...simpleUpdater<
        DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("formState"),
      ...simpleUpdater<
        DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("entity"),
      ...simpleUpdater<
        DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("config"),
      ...simpleUpdater<
        DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("apiRunner"),
      ...simpleUpdaterWithChildren<
        DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()({
        ...simpleUpdater<
          DispatchCreateFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >["apiChecker"]
        >()("init"),
        ...simpleUpdater<
          DispatchCreateFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >["apiChecker"]
        >()("create"),
      })("apiChecker"),
    },
    Template: {
      entity: (
        _: BasicUpdater<PredicateValue>,
      ): Updater<
        DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      > =>
        DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >().Updaters.Core.entity(
          Synchronized.Updaters.sync(AsyncState.Operations.map(_)),
        ),
      submit: (): Updater<
        DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      > =>
        DispatchCreateFormLauncherState<
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

export type DispatchCreateFormLauncherForeignMutationsExpected<T> = {};
