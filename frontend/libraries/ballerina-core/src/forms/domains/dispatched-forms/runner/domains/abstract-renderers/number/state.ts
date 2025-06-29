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
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";

export type NumberAbstractRendererReadonlyContext<CustomPresentationContext> =
  CommonAbstractRendererReadonlyContext<
    DispatchPrimitiveType<any>,
    number,
    CustomPresentationContext
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
  CustomPresentationContext = Unit,
  Flags = Unit,
> = View<
  NumberAbstractRendererReadonlyContext<CustomPresentationContext> &
    NumberAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  NumberAbstractRendererState,
  NumberAbstractRendererForeignMutationsExpected<Flags>
>;
