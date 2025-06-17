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
  Flags = Unit,
> = View<
  Context &
    Value<number> &
    DomNodeIdReadonlyContext & { commonFormState: DispatchCommonFormState } & {
      disabled: boolean;
    },
  NumberAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<number, Flags>;
    setNewValue: ValueCallbackWithOptionalFlags<number, Flags>;
  }
>;
