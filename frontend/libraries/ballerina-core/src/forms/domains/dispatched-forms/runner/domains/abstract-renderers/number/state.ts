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

export type NumberAbstractRendererReadonlyContext<
  CustomPresentationContext,
  ExtraContext,
> = CommonAbstractRendererReadonlyContext<
  DispatchPrimitiveType<any>,
  number,
  CustomPresentationContext,
  ExtraContext
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
  ExtraContext = Unit,
> = View<
  NumberAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    NumberAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  NumberAbstractRendererState,
  NumberAbstractRendererForeignMutationsExpected<Flags>
>;
