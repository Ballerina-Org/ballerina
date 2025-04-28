import { View } from "../../../../../../../template/state";
import {
  FormLabel,
  SimpleCallback,
  Value,
  DispatchCommonFormState,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { DispatchOnChange } from "../../../state";

export type BoolAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: Unit;
};

export const BoolAbstractRendererState = {
  Default: (): BoolAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {},
  }),
};

export type BoolAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<boolean> & { commonFormState: DispatchCommonFormState } & {
      disabled: boolean;
    },
  BoolAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<boolean>;
    setNewValue: SimpleCallback<boolean>;
  }
>;
