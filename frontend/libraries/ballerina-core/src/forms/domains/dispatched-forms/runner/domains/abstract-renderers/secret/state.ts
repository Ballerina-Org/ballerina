import {
  FormLabel,
  SimpleCallback,
  Unit,
  Value,
  View,
  DispatchCommonFormState,
  DispatchOnChange,
  DomNodeIdReadonlyContext,
  ValueCallbackWithOptionalFlags,
} from "../../../../../../../../main";

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
  Flags = Unit,
> = View<
  Context &
    DomNodeIdReadonlyContext &
    Value<string> & { commonFormState: DispatchCommonFormState } & {
      disabled: boolean;
    },
  SecretAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<string, Flags>;
    setNewValue: ValueCallbackWithOptionalFlags<string, Flags>;
  }
>;
