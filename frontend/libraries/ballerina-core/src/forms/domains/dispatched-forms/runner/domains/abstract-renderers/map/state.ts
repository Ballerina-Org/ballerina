import { Map } from "immutable";
import {
  simpleUpdater,
  Template,
  Unit,
  ValueTuple,
  View,
  MapRepo,
  BasicUpdater,
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  VoidCallbackWithOptionalFlags,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  MapType,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";

export type MapAbstractRendererReadonlyContext<
  CustomPresentationContext,
  ExtraContext,
> = CommonAbstractRendererReadonlyContext<
  MapType<any>,
  ValueTuple,
  CustomPresentationContext,
  ExtraContext
>;

export type MapAbstractRendererState = CommonAbstractRendererState & {
  elementFormStates: Map<
    number,
    {
      KeyFormState: CommonAbstractRendererState;
      ValueFormState: CommonAbstractRendererState;
    }
  >;
};

export const MapAbstractRendererState = {
  Default: {
    zero: (): MapAbstractRendererState => ({
      ...CommonAbstractRendererState.Default(),
      elementFormStates: Map(),
    }),
    elementFormStates: (
      elementFormStates: MapAbstractRendererState["elementFormStates"],
    ): MapAbstractRendererState => ({
      ...CommonAbstractRendererState.Default(),
      elementFormStates,
    }),
  },
  Updaters: {
    Core: {
      ...simpleUpdater<MapAbstractRendererState>()("commonFormState"),
      ...simpleUpdater<MapAbstractRendererState>()("elementFormStates"),
    },
    Template: {
      upsertElementKeyFormState: (
        elementIndex: number,
        defaultKeyFormState: CommonAbstractRendererState,
        defaultValueFormState: CommonAbstractRendererState,
        updater: BasicUpdater<CommonAbstractRendererState>,
      ) =>
        MapAbstractRendererState.Updaters.Core.elementFormStates(
          MapRepo.Updaters.upsert(
            elementIndex,
            () => ({
              KeyFormState: defaultKeyFormState,
              ValueFormState: defaultValueFormState,
            }),
            (_) => ({
              ..._,
              KeyFormState: updater(_.KeyFormState),
            }),
          ),
        ),
      upsertElementValueFormState: (
        elementIndex: number,
        defaultKeyFormState: CommonAbstractRendererState,
        defaultValueFormState: CommonAbstractRendererState,
        updater: BasicUpdater<CommonAbstractRendererState>,
      ) =>
        MapAbstractRendererState.Updaters.Core.elementFormStates(
          MapRepo.Updaters.upsert(
            elementIndex,
            () => ({
              KeyFormState: defaultKeyFormState,
              ValueFormState: defaultValueFormState,
            }),
            (_) => ({
              ..._,
              ValueFormState: updater(_.ValueFormState),
            }),
          ),
        ),
    },
  },
};

export type MapAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<ValueTuple, Flags>;
  add: VoidCallbackWithOptionalFlags<Flags>;
  remove: ValueCallbackWithOptionalFlags<number, Flags>;
};

export type MapAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  MapAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext> &
    MapAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  MapAbstractRendererState,
  MapAbstractRendererForeignMutationsExpected<Flags>,
  {
    embeddedKeyTemplate: (
      elementIndex: number,
    ) => (
      flags: Flags | undefined,
    ) => Template<
      MapAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      > &
        MapAbstractRendererState,
      MapAbstractRendererState,
      MapAbstractRendererForeignMutationsExpected<Flags>
    >;
    embeddedValueTemplate: (
      elementIndex: number,
    ) => (
      flags: Flags | undefined,
    ) => Template<
      MapAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      > &
        MapAbstractRendererState,
      MapAbstractRendererState,
      MapAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
