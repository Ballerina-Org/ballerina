import {
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  DispatchCommonFormState,
  DispatchOnChange,
  DispatchPrimitiveType,
  FormLabel,
  simpleUpdater,
  Unit,
  ValueUnit,
  View,
  VoidCallbackWithOptionalFlags,
} from "../../../../../../../../main";

export type UnitAbstractRendererReadonlyContext<CustomPresentationContext = Unit> =
  CommonAbstractRendererReadonlyContext<
    DispatchPrimitiveType<any>,
    ValueUnit,
    CustomPresentationContext
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

export type UnitAbstractRendererView<CustomPresentationContext = Unit, Flags = Unit> = View<
  UnitAbstractRendererReadonlyContext<CustomPresentationContext> &
    UnitAbstractRendererState,
  UnitAbstractRendererState,
  UnitAbstractRendererViewForeignMutationsExpected<Flags>,
  Unit
>;
