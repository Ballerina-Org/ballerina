import {
  DispatchCommonFormState,
  FormLabel,
  simpleUpdater,
  Unit,
  View,
  DomNodeIdReadonlyContext,
  VoidCallbackWithOptionalFlags,
} from "../../../../../../../../main";

export type UnitAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
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

export type UnitAbstractRendererView<
  Context extends FormLabel,
  Flags = Unit,
> = View<
  Context & UnitAbstractRendererState & DomNodeIdReadonlyContext,
  UnitAbstractRendererState,
  { set: VoidCallbackWithOptionalFlags<Flags> }
>;
