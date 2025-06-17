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
  Flags = Unit,
> = View<
  Context &
    Value<string> &
    DomNodeIdReadonlyContext & {
      commonFormState: DispatchCommonFormState;
      customFormState: Unit;
    } & { disabled: boolean },
  { commonFormState: DispatchCommonFormState; customFormState: Unit },
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<string, Flags>;
    setNewValue: ValueCallbackWithOptionalFlags<string, Flags>;
  }
>;
