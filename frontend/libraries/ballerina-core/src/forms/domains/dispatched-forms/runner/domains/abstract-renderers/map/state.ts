import { List, Map } from "immutable";
import {
  BasicFun,
  Bindings,
  FormLabel,
  DispatchCommonFormState,
  SimpleCallback,
  simpleUpdater,
  Template,
  Unit,
  Value,
  ValueTuple,
  View,
  MapRepo,
  BasicUpdater,
  DispatchOnChange,
  DomNodeIdReadonlyContext,
} from "../../../../../../../../main";

export type MapAbstractRendererState<KeyFormState, ValueFormState> = {
  commonFormState: DispatchCommonFormState;
  elementFormStates: Map<
    number,
    { KeyFormState: KeyFormState; ValueFormState: ValueFormState }
  >;
};

export const MapAbstractRendererState = <KeyFormState, ValueFormState>() => ({
  Default: {
    zero: (): MapAbstractRendererState<KeyFormState, ValueFormState> => ({
      commonFormState: DispatchCommonFormState.Default(),
      elementFormStates: Map(),
    }),
    elementFormStates: (
      elementFormStates: MapAbstractRendererState<
        KeyFormState,
        ValueFormState
      >["elementFormStates"],
    ): MapAbstractRendererState<KeyFormState, ValueFormState> => ({
      commonFormState: DispatchCommonFormState.Default(),
      elementFormStates,
    }),
  },
  Updaters: {
    Core: {
      ...simpleUpdater<
        MapAbstractRendererState<KeyFormState, ValueFormState>
      >()("commonFormState"),
      ...simpleUpdater<
        MapAbstractRendererState<KeyFormState, ValueFormState>
      >()("elementFormStates"),
    },
    Template: {
      upsertElementKeyFormState: (
        elementIndex: number,
        defaultKeyFormState: KeyFormState,
        defaultValueFormState: ValueFormState,
        updater: BasicUpdater<KeyFormState>,
      ) =>
        MapAbstractRendererState<
          KeyFormState,
          ValueFormState
        >().Updaters.Core.elementFormStates(
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
        defaultKeyFormState: KeyFormState,
        defaultValueFormState: ValueFormState,
        updater: BasicUpdater<ValueFormState>,
      ) =>
        MapAbstractRendererState<
          KeyFormState,
          ValueFormState
        >().Updaters.Core.elementFormStates(
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
});
export type MapAbstractRendererView<
  KeyFormState,
  ValueFormState,
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<ValueTuple> &
    DomNodeIdReadonlyContext &
    MapAbstractRendererState<KeyFormState, ValueFormState>,
  MapAbstractRendererState<KeyFormState, ValueFormState>,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueTuple>;
    add: SimpleCallback<Unit>;
    remove: SimpleCallback<number>;
  },
  {
    embeddedKeyTemplate: BasicFun<
      number,
      Template<
        Context &
          Value<ValueTuple> &
          MapAbstractRendererState<KeyFormState, ValueFormState> & {
            bindings: Bindings;
            extraContext: any;
          },
        MapAbstractRendererState<KeyFormState, ValueFormState>,
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueTuple>;
        }
      >
    >;
    embeddedValueTemplate: BasicFun<
      number,
      Template<
        Context &
          Value<ValueTuple> &
          MapAbstractRendererState<KeyFormState, ValueFormState> & {
            bindings: Bindings;
            extraContext: any;
          },
        MapAbstractRendererState<KeyFormState, ValueFormState>,
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueTuple>;
        }
      >
    >;
  }
>;
