import {
  Bindings,
  DispatchOnChange,
  DispatchParsedType,
  PredicateValue,
  simpleUpdater,
  simpleUpdaterWithChildren,
  Unit,
} from "../../../../../../../main";

export const getLeafIdentifierFromIdentifier = (identifier: string): string => {
  const matches = [...identifier.matchAll(/\[([^\]]+)\]/g)];
  if (matches.length === 0) return "";
  return matches[matches.length - 1][1];
};

export type CommonAbstractRendererReadonlyContext<
  T extends DispatchParsedType<any>,
  V extends PredicateValue,
  C = Unit,
> = {
  value: V;
  disabled: boolean;
  bindings: Bindings;
  extraContext: unknown;
  identifiers: { withLauncher: string; withoutLauncher: string };
  domNodeId: string;
  type: T;
  label?: string;
  tooltip?: string;
  details?: string;
  CustomPresentationContext: C | undefined;
  remoteEntityVersionIdentifier: string;
  serializedTypeHierarchy: string[];
};

export type CommonAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: unknown;
};

export const CommonAbstractRendererState = {
  Default: (): CommonAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {},
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<CommonAbstractRendererState>()("commonFormState"),
      ...simpleUpdater<CommonAbstractRendererState>()("customFormState"),
      ...simpleUpdaterWithChildren<CommonAbstractRendererState>()({
        ...simpleUpdater<CommonAbstractRendererState["commonFormState"]>()(
          "modifiedByUser",
        ),
      })("commonFormState"),
    },
  },
};

export type CommonAbstractRendererForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<PredicateValue, Flags>;
};

export type DispatchCommonFormState = {
  modifiedByUser: boolean;
};

export const DispatchCommonFormState = {
  Default: (): DispatchCommonFormState => ({
    modifiedByUser: false,
  }),
  Updaters: {
    ...simpleUpdater<DispatchCommonFormState>()("modifiedByUser"),
  },
};
