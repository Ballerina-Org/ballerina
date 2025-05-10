import {
  DispatchCommonFormState,
  FormLabel,
  simpleUpdater,
  Unit,
  View,
} from "../../../../../../../../main";
import { DispatchOnChange } from "../../dispatcher/state-3";

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
  Context & UnitAbstractRendererState,
  UnitAbstractRendererState,
  { onChange: DispatchOnChange<Unit> }
>;
