import {
  Unit,
  View,
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  CommonAbstractRendererReadonlyContext,
  DispatchPrimitiveType,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";

export type SecretAbstractRendererReadonlyContext<
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  DispatchPrimitiveType<any>,
  string,
  CustomPresentationContext,
  ExtraContext
>;

export type SecretAbstractRendererState = CommonAbstractRendererState;

export const SecretAbstractRendererState = {
  Default: (): SecretAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
  }),
};

export type SecretAbstractRendererForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<string, Flags>;
};

export type SecretAbstractRendererViewForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<string, Flags>;
  setNewValue: ValueCallbackWithOptionalFlags<string, Flags>;
};

export type SecretAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  SecretAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    SecretAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  SecretAbstractRendererState,
  SecretAbstractRendererViewForeignMutationsExpected<Flags>
>;
