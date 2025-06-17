import { View } from "../../../../../../../template/state";
import {
  FormLabel,
  Value,
  DispatchCommonFormState,
  DispatchOnChange,
  DomNodeIdReadonlyContext,
  ValueCallbackWithOptionalFlags,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";

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
  Flags = Unit,
> = View<
  Context &
    Value<boolean> &
    DomNodeIdReadonlyContext & { commonFormState: DispatchCommonFormState } & {
      disabled: boolean;
    },
  BoolAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<boolean, Flags>;
    setNewValue: ValueCallbackWithOptionalFlags<boolean, Flags>;
  }
>;
