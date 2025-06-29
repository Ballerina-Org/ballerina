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
> = CommonAbstractRendererReadonlyContext<
  DispatchPrimitiveType<any>,
  string,
  CustomPresentationContext
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
> = View<
  SecretAbstractRendererReadonlyContext<CustomPresentationContext> &
    SecretAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  SecretAbstractRendererState,
  SecretAbstractRendererViewForeignMutationsExpected<Flags>
>;
