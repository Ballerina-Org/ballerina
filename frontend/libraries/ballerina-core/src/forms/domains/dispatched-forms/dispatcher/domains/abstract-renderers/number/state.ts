import {
  FormLabel,
  SimpleCallback,
  Unit,
  Value,
  View,
  DispatchCommonFormState,
} from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";

export type NumberAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: Unit;
};

export const NumberAbstractRendererState = {
  Default: (): NumberAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {},
  }),
};

export type NumberAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<number> & { commonFormState: DispatchCommonFormState } & {
      disabled: boolean;
    },
  NumberAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<number>;
    setNewValue: SimpleCallback<number>;
  }
>;
