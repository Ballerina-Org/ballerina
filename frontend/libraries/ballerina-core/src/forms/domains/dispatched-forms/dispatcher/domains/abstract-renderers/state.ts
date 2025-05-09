import {
  Bindings,
  DispatchParsedType,
  PredicateValue,
  Value,
  simpleUpdater,
  simpleUpdaterWithChildren,
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
  customFormState: any;
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
