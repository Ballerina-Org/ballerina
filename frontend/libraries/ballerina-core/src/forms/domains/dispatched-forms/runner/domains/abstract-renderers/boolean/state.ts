import { View } from "../../../../../../../template/state";
import {
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  CommonAbstractRendererReadonlyContext,
  DispatchPrimitiveType,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";

export type BoolAbstractRendererReadonlyContext<CustomPresentationContext> =
  CommonAbstractRendererReadonlyContext<
    DispatchPrimitiveType<any>,
    boolean,
    CustomPresentationContext
  >;

export type BoolAbstractRendererState = CommonAbstractRendererState;

export const BoolAbstractRendererState = {
  Default: (): BoolAbstractRendererState =>
    CommonAbstractRendererState.Default(),
};

export type BoolAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<boolean, Flags>;
  setNewValue: ValueCallbackWithOptionalFlags<boolean, Flags>;
};

export type BoolAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
> = View<
  BoolAbstractRendererReadonlyContext<CustomPresentationContext> &
    BoolAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  BoolAbstractRendererState,
  BoolAbstractRendererForeignMutationsExpected<Flags>
>;
