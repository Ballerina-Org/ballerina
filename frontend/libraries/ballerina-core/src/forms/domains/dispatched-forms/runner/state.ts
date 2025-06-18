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
} from "../../../../../main";

export type LauncherRef<Flags = Unit> = {
  name: string;
  kind: "passthrough";
  entity: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
  config: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
  onEntityChange: DispatchOnChange<PredicateValue, Flags>;
};

export type DispatchFormRunnerStatus<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> =
  | { kind: "not initialized" }
  | { kind: "loading" }
  | {
      kind: "loaded";
      Form: Template<any, any, {
        onChange: DispatchOnChange<PredicateValue, Flags>;
      }, any>;
    }
  | { kind: "error"; errors: List<string> };

export type DispatchFormRunnerContext<
  T extends DispatchInjectablesTypes<T>,
> = {
  extraContext: any;
  launcherRef: LauncherRef;
  showFormParsingErrors: BasicFun<
    DispatchSpecificationDeserializationResult<T>,
    JSX.Element
  >;
  remoteEntityVersionIdentifier: string;
  loadingComponent?: JSX.Element;
  errorComponent?: JSX.Element;
} & DispatchFormsParserState<T>;

export type DispatchFormRunnerState<
  T extends DispatchInjectablesTypes<T>,
> = {
  status: DispatchFormRunnerStatus<T>;
  formState: any;
};
export type DispatchFormRunnerForeignMutationsExpected = Unit;
export const DispatchFormRunnerState = <
  T extends DispatchInjectablesTypes<T>,
>() => {
  return {
    Default: (): DispatchFormRunnerState<T> => ({
      status: { kind: "not initialized" },
      formState: unit,
    }),
    Updaters: {
      ...simpleUpdater<DispatchFormRunnerState<T>>()("status"),
      ...simpleUpdater<DispatchFormRunnerState<T>>()("formState"),
    },
  };
};
