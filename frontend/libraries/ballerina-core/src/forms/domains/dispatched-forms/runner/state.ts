import { List } from "immutable";
import {
  BasicFun,
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
} from "../../../../../main";
import {
  DispatchCreateFormLauncherApi,
  DispatchCreateFormLauncherState,
} from "./domains/kind/create/state";
import {
  DispatchEditFormLauncherApi,
  DispatchEditFormLauncherState,
} from "./domains/kind/edit/state";
import { DispatchPassthroughFormLauncherState } from "./domains/kind/passthrough/state";

export type ApiSources = {
  infiniteStreamSources: DispatchInfiniteStreamSources;
  enumOptionsSources: DispatchEnumOptionsSources;
  entityApis: DispatchEntityApis;
  tableApiSources?: DispatchTableApiSources;
  lookupSources?: DispatchLookupSources;
};

export type BaseLauncherRef = {
  name: string;
  apiSources: ApiSources;
};

export type PassthroughLauncherRef<Flags = Unit> = {
  kind: "passthrough";
  entity: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
  config: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
  onEntityChange: DispatchOnChange<PredicateValue, Flags>;
} & BaseLauncherRef;

export type EditLauncherRef<Flags = Unit> = {
  kind: "edit";
  entityId: Guid;
  apiHandlers?: FormRefEditApiHandlers<any>;
} & BaseLauncherRef;

export type CreateLauncherRef<Flags = Unit> = {
  kind: "create";
  apiHandlers?: FormRefCreateApiHandlers<any>;
  config:
    | {
        source: "entity";
        value: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
      }
    | {
        source: "api";
        getGlobalConfig?: () => Promise<any>;
      };
} & BaseLauncherRef;

export type LauncherRef<Flags = Unit> =
  | PassthroughLauncherRef<Flags>
  | EditLauncherRef<Flags>
  | CreateLauncherRef<Flags>;

export type DispatchFormRunnerStatus<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> =
  | { kind: "not initialized" }
  | { kind: "loading" }
  | {
      kind: "loaded";
      Form: Template<
        any,
        any,
        {
          onChange: DispatchOnChange<PredicateValue, Flags>;
        },
        any
      >;
    }
  | { kind: "error"; errors: List<string> };

export type DispatchFormRunnerContext<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
> = {
  extraContext: ExtraContext;
  launcherRef: LauncherRef<Flags>;
  showFormParsingErrors: BasicFun<
    DispatchSpecificationDeserializationResult<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    JSX.Element
  >;
  remoteEntityVersionIdentifier: string;
  loadingComponent?: JSX.Element;
  errorComponent?: JSX.Element;
} & DispatchFormsParserState<
  T,
  Flags,
  CustomPresentationContexts,
  ExtraContext
>;

export type DispatchFormRunnerState<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> = {
  innerFormState:
    | { kind: "create"; state: DispatchCreateFormLauncherState<T, Flags> }
    | { kind: "edit"; state: DispatchEditFormLauncherState<T, Flags> }
    | {
        kind: "passthrough";
        state: DispatchPassthroughFormLauncherState<T, Flags>;
      };
};

export const DispatchFormRunnerState = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
>() => ({
  Default: {
    create: (): DispatchFormRunnerState<T, Flags> => ({
      innerFormState: {
        kind: "create",
        state: DispatchCreateFormLauncherState<T, Flags>().Default(),
      },
    }),
    edit: (): DispatchFormRunnerState<T, Flags> => ({
      innerFormState: {
        kind: "edit",
        state: DispatchEditFormLauncherState<T, Flags>().Default(),
      },
    }),
    passthrough: (): DispatchFormRunnerState<T, Flags> => ({
      innerFormState: {
        kind: "passthrough",
        state: DispatchPassthroughFormLauncherState<T, Flags>().Default(),
      },
    }),
  },
  Updaters: {
    Core: {
      ...simpleUpdater<DispatchFormRunnerState<T, Flags>>()("innerFormState"),
    },
    Template: {
      create: (
        upd: BasicUpdater<DispatchCreateFormLauncherState<T, Flags>>,
      ): Updater<DispatchFormRunnerState<T, Flags>> =>
        DispatchFormRunnerState<T, Flags>().Updaters.Core.innerFormState((v) =>
          v.kind === "create" ? { ...v, state: upd(v.state) } : v,
        ),
      edit: (
        upd: BasicUpdater<DispatchEditFormLauncherState<T, Flags>>,
      ): Updater<DispatchFormRunnerState<T, Flags>> =>
        DispatchFormRunnerState<T, Flags>().Updaters.Core.innerFormState((v) =>
          v.kind === "edit" ? { ...v, state: upd(v.state) } : v,
        ),
      passthrough: (
        upd: BasicUpdater<DispatchPassthroughFormLauncherState<T, Flags>>,
      ): Updater<DispatchFormRunnerState<T, Flags>> =>
        DispatchFormRunnerState<T, Flags>().Updaters.Core.innerFormState((v) =>
          v.kind === "passthrough" ? { ...v, state: upd(v.state) } : v,
        ),
    },
  },
});

export type DispatchFormRunnerForeignMutationsExpected = Unit;

export type DispatchCommonFormRunnerState<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> = {
  status: DispatchFormRunnerStatus<T, Flags>;
  formState: any;
};

export const DispatchCommonFormRunnerState = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
>() => {
  return {
    Default: (): DispatchCommonFormRunnerState<T, Flags> => ({
      status: { kind: "not initialized" },
      formState: unit,
    }),
    Updaters: {
      ...simpleUpdater<DispatchCommonFormRunnerState<T, Flags>>()("status"),
      ...simpleUpdater<DispatchCommonFormRunnerState<T, Flags>>()("formState"),
    },
  };
};
