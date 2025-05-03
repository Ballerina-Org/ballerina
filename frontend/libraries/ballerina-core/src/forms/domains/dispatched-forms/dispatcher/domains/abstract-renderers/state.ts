import {
  Bindings,
  PredicateValue,
  simpleUpdater,
} from "../../../../../../../main";

export type CommonAbstractRendererReadonlyContext = {
  value: PredicateValue;
  disabled: boolean;
  bindings: Bindings;
  extraContext: any;
  identifiers: { withLauncher: string; withoutLauncher: string };
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
