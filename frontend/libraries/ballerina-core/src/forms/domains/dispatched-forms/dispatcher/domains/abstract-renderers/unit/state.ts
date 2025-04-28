import {
  FormLabel,
  simpleUpdater,
  Unit,
  View,
} from "../../../../../../../../main";
import { DispatchCommonFormState } from "../../../../built-ins/state";
import { DispatchOnChange } from "../../../state";

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
