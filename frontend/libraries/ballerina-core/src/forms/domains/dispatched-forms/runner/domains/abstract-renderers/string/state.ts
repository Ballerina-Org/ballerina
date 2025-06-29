import {
  Unit,
  View,
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  CommonAbstractRendererState,
  DispatchPrimitiveType,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";

export type StringAbstractRendererReadonlyContext<
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  DispatchPrimitiveType<any>,
  string,
  CustomPresentationContext,
  ExtraContext
>;

export type StringAbstractRendererState = CommonAbstractRendererState;

export const StringAbstractRendererState = {
  Default: (): StringAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
  }),
};

export type StringAbstractRendererForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<string, Flags>;
};

export type StringAbstractRendererViewForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<string, Flags>;
  setNewValue: ValueCallbackWithOptionalFlags<string, Flags>;
};

export type StringAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  StringAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    StringAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  StringAbstractRendererState,
  StringAbstractRendererViewForeignMutationsExpected<Flags>
>;
