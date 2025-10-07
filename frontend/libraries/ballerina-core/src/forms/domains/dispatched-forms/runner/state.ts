import { List } from "immutable";
import {
  BasicFun,
  DispatchParsedType,
  DispatchSpecificationDeserializationResult,
  DispatchFormsParserState,
  PredicateValue,
  simpleUpdater,
  Sum,
  Unit,
  ValueOrErrors,
  Template,
  unit,
  DispatchOnChange,
  DispatchInjectablesTypes,
  DispatchInfiniteStreamSources,
  DispatchEnumOptionsSources,
  DispatchEntityApis,
  DispatchTableApiSources,
  DispatchLookupSources,
  FormRefEditApiHandlers,
  FormRefCreateApiHandlers,
  Guid,
  BasicUpdater,
  Updater,
  Renderer,
  DispatcherContext,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererForeignMutationsExpected,
} from "../../../../../main";
import { DispatchCreateFormLauncherState } from "./domains/kind/create/state";
import {
  DispatchEditFormLauncherApi,
  DispatchEditFormLauncherState,
} from "./domains/kind/edit/state";
import { DispatchPassthroughFormLauncherState } from "./domains/kind/passthrough/state";

export type DispatcherContextWithApiSources<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
> = Omit<
  DispatcherContext<T, Flags, CustomPresentationContext, ExtraContext>,
  "defaultState"
> &
  BaseApiSources & {
    defaultState: (
      t: DispatchParsedType<T>,
      renderer: Renderer<T>,
    ) => ValueOrErrors<any, string>;
  };

export type BaseLauncherRef<T extends BaseApiSources> = {
  name: string;
  apiSources: T;
};

export type LauncherRefWithEntityApis<T extends BaseApiSources> = T & {
  entityApis: DispatchEntityApis;
};

export type BaseApiSources = {
  infiniteStreamSources: DispatchInfiniteStreamSources;
  enumOptionsSources: DispatchEnumOptionsSources;
  tableApiSources?: DispatchTableApiSources;
  lookupSources?: DispatchLookupSources;
};

export type EntityLauncherRefConfig =
  | {
      source: "entity";
      value: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
    }
  | {
      source: "api";
      getGlobalConfig?: () => Promise<any>;
    };

export type PassthroughLauncherRef<Flags = Unit> = {
  kind: "passthrough";
  entity: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
  config: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
  onEntityChange: DispatchOnChange<PredicateValue, Flags>;
} & BaseLauncherRef<BaseApiSources>;

export type EditLauncherRef = {
  kind: "edit";
  entityId: Guid;
  apiHandlers?: FormRefEditApiHandlers<any>;
  config: EntityLauncherRefConfig;
} & BaseLauncherRef<LauncherRefWithEntityApis<BaseApiSources>>;

export type CreateLauncherRef = {
  kind: "create";
  apiHandlers?: FormRefCreateApiHandlers<any>;
  config: EntityLauncherRefConfig;
} & BaseLauncherRef<LauncherRefWithEntityApis<BaseApiSources>>;

export type LauncherRef<Flags = Unit> =
  | PassthroughLauncherRef<Flags>
  | EditLauncherRef
  | CreateLauncherRef;

export type DispatchFormRunnerStatus<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
> =
  | { kind: "not initialized" }
  | { kind: "loading" }
  | {
      kind: "loaded";
      Form: Template<
        CommonAbstractRendererReadonlyContext<
          DispatchParsedType<T>,
          PredicateValue,
          CustomPresentationContext,
          ExtraContext
        > &
          CommonAbstractRendererState,
        CommonAbstractRendererState,
        CommonAbstractRendererForeignMutationsExpected<Flags>
      >;
    }
  | { kind: "error"; errors: List<string> };

export type DispatchFormRunnerContext<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = {
  extraContext: ExtraContext;
  globallyDisabled: boolean;
  globallyReadOnly: boolean;
  launcherRef: LauncherRef<Flags>;
  showFormParsingErrors: BasicFun<
    DispatchSpecificationDeserializationResult<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    JSX.Element
  >;
  remoteEntityVersionIdentifier: string;
  loadingComponent?: JSX.Element;
  errorComponent?: JSX.Element;
} & DispatchFormsParserState<T, Flags, CustomPresentationContext, ExtraContext>;

export type DispatchFormRunnerState<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
> = {
  innerFormState:
    | {
        kind: "create";
        state: DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >;
      }
    | {
        kind: "edit";
        state: DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >;
      }
    | {
        kind: "passthrough";
        state: DispatchPassthroughFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >;
      };
};

export const DispatchFormRunnerState = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() => ({
  Default: {
    create: (): DispatchFormRunnerState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    > => ({
      innerFormState: {
        kind: "create",
        state: DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >().Default(),
      },
    }),
    edit: (): DispatchFormRunnerState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    > => ({
      innerFormState: {
        kind: "edit",
        state: DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >().Default(),
      },
    }),
    passthrough: (): DispatchFormRunnerState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    > => ({
      innerFormState: {
        kind: "passthrough",
        state: DispatchPassthroughFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >().Default(),
      },
    }),
  },
  Updaters: {
    Core: {
      ...simpleUpdater<
        DispatchFormRunnerState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("innerFormState"),
    },
    Template: {
      create: (
        upd: BasicUpdater<
          DispatchCreateFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >
        >,
      ): Updater<
        DispatchFormRunnerState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      > =>
        DispatchFormRunnerState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >().Updaters.Core.innerFormState((v) =>
          v.kind === "create" ? { ...v, state: upd(v.state) } : v,
        ),
      edit: (
        upd: BasicUpdater<
          DispatchEditFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >
        >,
      ): Updater<
        DispatchFormRunnerState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      > =>
        DispatchFormRunnerState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >().Updaters.Core.innerFormState((v) =>
          v.kind === "edit" ? { ...v, state: upd(v.state) } : v,
        ),
      passthrough: (
        upd: BasicUpdater<
          DispatchPassthroughFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >
        >,
      ): Updater<
        DispatchFormRunnerState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      > =>
        DispatchFormRunnerState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >().Updaters.Core.innerFormState((v) =>
          v.kind === "passthrough" ? { ...v, state: upd(v.state) } : v,
        ),
    },
  },
});

export type DispatchFormRunnerForeignMutationsExpected = Unit;

export type DispatchCommonFormRunnerState<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
> = {
  status: DispatchFormRunnerStatus<
    T,
    Flags,
    CustomPresentationContext,
    ExtraContext
  >;
  formState: CommonAbstractRendererState;
  formName: string;
};

export const DispatchCommonFormRunnerState = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() => {
  return {
    Default: (): DispatchCommonFormRunnerState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    > => ({
      status: { kind: "not initialized" },
      formState: CommonAbstractRendererState.Default(),
      formName: "",
    }),
    Updaters: {
      ...simpleUpdater<
        DispatchCommonFormRunnerState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("status"),
      ...simpleUpdater<
        DispatchCommonFormRunnerState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("formState"),
      ...simpleUpdater<
        DispatchCommonFormRunnerState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >()("formName"),
    },
  };
};
