import { List } from "immutable";
import {
  BasicFun,
  DispatchDelta,
  DispatcherContext,
  DispatchSpecificationDeserializationResult,
  DispatchFormsParserState,
  DispatchParsedLauncher,
  PredicateValue,
  simpleUpdater,
  Sum,
  Unit,
  Updater,
  ValueOrErrors,
  Template,
  unit,
} from "../../../../../main";

export type LauncherRef = {
  name: string;
  kind: "passthrough";
  entity: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
  config: Sum<ValueOrErrors<PredicateValue, string>, "not initialized">;
  onEntityChange: (
    updater: Updater<PredicateValue>,
    delta: DispatchDelta,
  ) => void;
};

export type DispatchFormRunnerStatus<
  T extends { [key in keyof T]: { type: any; state: any } },
> =
  | { kind: "not initialized" }
  | { kind: "loading" }
  | {
      kind: "loaded";
      Form: Template<any, any, any, any>;
    }
  | { kind: "error"; errors: List<string> };

export type DispatchFormRunnerContext<
  T extends { [key in keyof T]: { type: any; state: any } },
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
  T extends { [key in keyof T]: { type: any; state: any } },
> = {
  status: DispatchFormRunnerStatus<T>;
  formState: any;
};
export type DispatchFormRunnerForeignMutationsExpected = Unit;
export const DispatchFormRunnerState = <
  T extends { [key in keyof T]: { type: any; state: any } },
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
