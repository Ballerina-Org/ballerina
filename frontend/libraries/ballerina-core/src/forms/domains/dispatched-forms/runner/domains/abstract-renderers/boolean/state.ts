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

export type BoolAbstractRendererReadonlyContext<
  CustomPresentationContext,
  ExtraContext,
> = CommonAbstractRendererReadonlyContext<
  DispatchPrimitiveType<any>,
  boolean,
  CustomPresentationContext,
  ExtraContext
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
  ExtraContext = Unit,
> = View<
  BoolAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext> &
    BoolAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  BoolAbstractRendererState,
  BoolAbstractRendererForeignMutationsExpected<Flags>
>;
