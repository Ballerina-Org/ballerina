import {
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  DispatchCommonFormState,
  DispatchOnChange,
  DispatchPrimitiveType,
  simpleUpdater,
  Unit,
  ValueUnit,
  View,
  VoidCallbackWithOptionalFlags,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";

export type UnitAbstractRendererReadonlyContext<
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  DispatchPrimitiveType<any>,
  ValueUnit,
  CustomPresentationContext,
  ExtraContext
>;

export type UnitAbstractRendererState = CommonAbstractRendererState & {
  customFormState: Unit;
};

export const UnitAbstractRendererState = {
  Default: (): UnitAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {},
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<UnitAbstractRendererState>()("commonFormState"),
    },
  },
};

export type UnitAbstractRendererForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueUnit, Flags>;
};

export type UnitAbstractRendererViewForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueUnit, Flags>;
  set: VoidCallbackWithOptionalFlags<Flags>;
};

export type UnitAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  UnitAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext> &
    UnitAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  UnitAbstractRendererState,
  UnitAbstractRendererViewForeignMutationsExpected<Flags>,
  Unit
>;
