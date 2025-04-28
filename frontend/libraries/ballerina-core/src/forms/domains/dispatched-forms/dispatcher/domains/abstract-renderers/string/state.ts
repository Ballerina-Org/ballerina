import {
  FormLabel,
  SimpleCallback,
  Unit,
  Value,
  View,
  DispatchCommonFormState,
} from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";

export type StringAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: Unit;
};

export const StringAbstractRendererState = {
  Default: (): StringAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {},
  }),
};

export type StringAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<string> & {
      commonFormState: DispatchCommonFormState;
      customFormState: Unit;
    } & { disabled: boolean },
  { commonFormState: DispatchCommonFormState; customFormState: Unit },
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<string>;
    setNewValue: SimpleCallback<string>;
  }
>;
