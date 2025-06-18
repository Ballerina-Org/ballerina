import {
  FormLabel,
  SimpleCallback,
  Unit,
  Value,
  View,
  DispatchCommonFormState,
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  DispatchParsedType,
  CommonAbstractRendererReadonlyContext,
  DispatchPrimitiveType,
  CommonAbstractRendererState,
} from "../../../../../../../../main";

export type NumberAbstractRendererReadonlyContext<CustomContext> =
  CommonAbstractRendererReadonlyContext<
    DispatchPrimitiveType<any>,
    number,
    CustomContext
  >;

export type NumberAbstractRendererState = CommonAbstractRendererState;

export const NumberAbstractRendererState = {
  Default: (): NumberAbstractRendererState =>
    CommonAbstractRendererState.Default(),
};

export type NumberAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<number, Flags>;
  setNewValue: ValueCallbackWithOptionalFlags<number, Flags>;
};

export type NumberAbstractRendererView<
  CustomContext = Unit,
  Flags = Unit,
> = View<
  NumberAbstractRendererReadonlyContext<CustomContext> &
    NumberAbstractRendererState,
  NumberAbstractRendererState,
  NumberAbstractRendererForeignMutationsExpected<Flags>
>;
