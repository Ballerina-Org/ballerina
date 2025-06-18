import {
  FormLabel,
  Unit,
  Value,
  View,
  DispatchCommonFormState,
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  CommonAbstractRendererState,
  DispatchPrimitiveType,
  CommonAbstractRendererReadonlyContext,
} from "../../../../../../../../main";

export type StringAbstractRendererReadonlyContext<CustomContext = Unit> =
  CommonAbstractRendererReadonlyContext<
    DispatchPrimitiveType<any>,
    string,
    CustomContext
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
  CustomContext = Unit,
  Flags = Unit,
> = View<
  StringAbstractRendererReadonlyContext<CustomContext> &
    StringAbstractRendererState,
  StringAbstractRendererState,
  StringAbstractRendererViewForeignMutationsExpected<Flags>
>;
