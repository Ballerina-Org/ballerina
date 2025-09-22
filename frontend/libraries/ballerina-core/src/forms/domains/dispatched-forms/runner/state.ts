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
} from "../../../../../main";

export type ApiSources = {
  infiniteStreamSources: DispatchInfiniteStreamSources;
  enumOptionsSources: DispatchEnumOptionsSources;
  entityApis: DispatchEntityApis;
  tableApiSources?: DispatchTableApiSources;
  lookupSources?: DispatchLookupSources;
};

export type LauncherRef<Flags = Unit> = {
  name: string;
  kind: "passthrough";
  entity: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
  config: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
  onEntityChange: DispatchOnChange<PredicateValue, Flags>;
  apiSources: ApiSources;
};

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
  status: DispatchFormRunnerStatus<T, Flags>;
  formState: any;
};
export type DispatchFormRunnerForeignMutationsExpected = Unit;
export const DispatchFormRunnerState = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
>() => {
  return {
    Default: (): DispatchFormRunnerState<T, Flags> => ({
      status: { kind: "not initialized" },
      formState: unit,
    }),
    Updaters: {
      ...simpleUpdater<DispatchFormRunnerState<T, Flags>>()("status"),
      ...simpleUpdater<DispatchFormRunnerState<T, Flags>>()("formState"),
    },
  };
};
