import {
  FormLabel,
  SimpleCallback,
  Unit,
  Value,
  View,
  DispatchCommonFormState,
} from "../../../../../../../../main";
import { DispatchOnChange } from "../../dispatcher/state-3";

export type SecretAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: Unit;
};

export const SecretAbstractRendererState = {
  Default: (): SecretAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {},
  }),
};

export type SecretAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<string> & { commonFormState: DispatchCommonFormState } & {
      disabled: boolean;
    },
  SecretAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<string>;
    setNewValue: SimpleCallback<string>;
  }
>;
