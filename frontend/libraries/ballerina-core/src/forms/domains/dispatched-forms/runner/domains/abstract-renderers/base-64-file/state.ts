import { View } from "../../../../../../../template/state";
import {
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  CommonAbstractRendererReadonlyContext,
  DispatchPrimitiveType,
  CommonAbstractRendererState,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../../main";

export type Base64FileAbstractRendererReadonlyContext<
  CustomPresentationContext,
> = CommonAbstractRendererReadonlyContext<
  DispatchPrimitiveType<any>,
  string,
  CustomPresentationContext
>;

export type Base64FileAbstractRendererState = CommonAbstractRendererState;

export const Base64FileAbstractRendererState = {
  Default: () => CommonAbstractRendererState.Default(),
};

export type Base64FileAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<string, Flags>;
  setNewValue: ValueCallbackWithOptionalFlags<string, Flags>;
};

export type Base64FileAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
> = View<
  Base64FileAbstractRendererReadonlyContext<CustomPresentationContext> &
    Base64FileAbstractRendererState,
  Base64FileAbstractRendererState,
  Base64FileAbstractRendererForeignMutationsExpected<Flags>
>;
