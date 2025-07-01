import {
  Bindings,
  DispatchOnChange,
  DispatchParsedType,
  PredicateValue,
  simpleUpdater,
  simpleUpdaterWithChildren,
  Unit,
} from "../../../../../../../main";

export type CommonAbstractRendererReadonlyContext<
  T extends DispatchParsedType<any>,
  V extends PredicateValue,
  C = Unit,
  ExtraContext = Unit,
> = {
  value: V;
  disabled: boolean;
  bindings: Bindings;
  extraContext: ExtraContext;
  type: T;
  label?: string;
  tooltip?: string;
  details?: string;
  customPresentationContext: C | undefined;
  remoteEntityVersionIdentifier: string;
  domNodeAncestorPath: string;
  serializedTypeHierarchy: string[];
};

export type CommonAbstractRendererViewOnlyReadonlyContext = {
  domNodeId: string;
  completeSerializedTypeHierarchy: string[];
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
