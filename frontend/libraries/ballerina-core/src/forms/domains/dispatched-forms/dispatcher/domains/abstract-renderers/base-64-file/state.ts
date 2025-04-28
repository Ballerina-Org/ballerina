import { Value } from "../../../../../../../value/state";
import { View } from "../../../../../../../template/state";
import { FormLabel, SimpleCallback } from "../../../../../../../../main";
import { Unit } from "../../../../../../../../main";
import { DispatchCommonFormState } from "../../../../built-ins/state";
import { DispatchOnChange } from "../../../state";

export type Base64FileAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: Unit;
};

export const Base64FileAbstractRendererState = {
  Default: (): Base64FileAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {},
  }),
};

export type Base64FileAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<string> & { commonFormState: DispatchCommonFormState } & {
      disabled: boolean;
    },
  Base64FileAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<string>;
    setNewValue: SimpleCallback<string>;
  }
>;
