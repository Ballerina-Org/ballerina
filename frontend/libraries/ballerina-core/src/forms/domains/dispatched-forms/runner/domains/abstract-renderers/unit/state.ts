import {
  DispatchCommonFormState,
  FormLabel,
  simpleUpdater,
  Unit,
  View,
  DispatchOnChange,
  DomNodeIdReadonlyContext,
} from "../../../../../../../../main";

export type UnitAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: Unit;
};

export const UnitAbstractRendererState = {
  Default: (): UnitAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {},
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<UnitAbstractRendererState>()("commonFormState"),
    },
  },
};

export type UnitAbstractRendererView<Context extends FormLabel> = View<
  Context & UnitAbstractRendererState & DomNodeIdReadonlyContext,
  UnitAbstractRendererState,
  { onChange: DispatchOnChange<Unit> }
>;
