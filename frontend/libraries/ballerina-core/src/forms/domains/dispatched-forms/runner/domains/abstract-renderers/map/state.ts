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
  ValueOrErrors,
  PredicateValue,
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
  Operations: {
    ExtractKeyValuePairFromTuple: (
      tuple: ValueTuple,
      elementIndex: number,
      domNodeAncestorPath: string,
    ): ValueOrErrors<
      { key: PredicateValue; value: PredicateValue },
      string
    > => {
      const keyValuePair = tuple.values.get(elementIndex);
      if (keyValuePair == undefined) {
        return ValueOrErrors.Default.throwOne(
          `Could not find key value pair for element at index ${elementIndex} in tuple ${JSON.stringify(tuple)}
          \n...When sending onchange value delta
          \n...When rendering
          \n...${domNodeAncestorPath}`,
        );
      }
      if (!PredicateValue.Operations.IsTuple(keyValuePair)) {
        return ValueOrErrors.Default.throwOne(
          `Tuple expected but got: ${JSON.stringify(keyValuePair)}
          \n...When sending onchange value delta
          \n...When rendering
          \n...${domNodeAncestorPath}`,
        );
      }
      const key = keyValuePair.values.get(0);
      const value = keyValuePair.values.get(1);
      if (key == undefined || value == undefined) {
        return ValueOrErrors.Default.throwOne(
          `Could not find key or value for element at index ${elementIndex} in tuple ${JSON.stringify(tuple)}
          \n...When sending onchange value delta
          \n...When rendering
          \n...${domNodeAncestorPath}`,
        );
      }
      return ValueOrErrors.Default.return({
        key,
        value,
      });
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
