import {
  Bindings,
  DispatchParsedType,
  PredicateValue,
  Value,
  simpleUpdater,
} from "../../../../../../../main";

export type CommonAbstractRendererReadonlyContext<
  T extends DispatchParsedType<any>,
  V extends PredicateValue,
> = {
  value: V;
  disabled: boolean;
  bindings: Bindings;
  extraContext: any;
  identifiers: { withLauncher: string; withoutLauncher: string };
  type: T;
  label?: string;
  tooltip?: string;
  details?: string;
};

export type CommonAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
};

export const CommonAbstractRendererState = {
  Default: (): CommonAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
  }),
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
