import { View } from "../../../../../../../template/state";
import {
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  CommonAbstractRendererReadonlyContext,
  DispatchPrimitiveType,
  CommonAbstractRendererState,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";

export type BoolAbstractRendererReadonlyContext<CustomContext> =
  CommonAbstractRendererReadonlyContext<
    DispatchPrimitiveType<any>,
    boolean,
    CustomContext
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

export type BoolAbstractRendererView<CustomContext = Unit, Flags = Unit> = View<
  BoolAbstractRendererReadonlyContext<CustomContext> &
    BoolAbstractRendererState,
  BoolAbstractRendererState,
  BoolAbstractRendererForeignMutationsExpected<Flags>
>;
