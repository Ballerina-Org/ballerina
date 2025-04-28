import { List } from "immutable";

import { MapAbstractRendererState, MapAbstractRendererView } from "./state";
import { Template } from "../../../../../../../template/state";
import {
  PredicateValue,
  Unit,
  SimpleCallback,
  Value,
  ValueTuple,
  Updater,
  DispatchDelta,
  BasicUpdater,
  MapRepo,
  ListRepo,
  id,
  Bindings,
} from "../../../../../../../../main";
import { DispatchCommonFormState } from "../../../../built-ins/state";
import { FormLabel } from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";
import {
  DispatchParsedType,
  MapType,
} from "../../../../deserializer/domains/specification/domains/types/state";

export const MapAbstractRenderer = <
  KeyFormState extends { commonFormState: DispatchCommonFormState },
  ValueFormState extends { commonFormState: DispatchCommonFormState },
  Context extends FormLabel & {
    type: DispatchParsedType<any>;
    disabled: boolean;
  },
  ForeignMutationsExpected,
>(
  GetDefaultKeyFormState: () => KeyFormState,
  GetDefaultKeyFormValue: () => PredicateValue,
  GetDefaultValueFormState: () => ValueFormState,
  GetDefaultValueFormValue: () => PredicateValue,
  keyTemplate: Template<
    Value<PredicateValue> & KeyFormState & { bindings: Bindings },
    KeyFormState,
    {
      onChange: DispatchOnChange<PredicateValue>;
    }
  >,
  valueTemplate: Template<
    Value<PredicateValue> & ValueFormState & { bindings: Bindings },
    ValueFormState,
    {
      onChange: DispatchOnChange<PredicateValue>;
    }
  >,
) => {
  const embeddedKeyTemplate = (elementIndex: number) =>
    keyTemplate
      .mapContext(
        (
          _: Context &
            Value<ValueTuple> &
            MapAbstractRendererState<KeyFormState, ValueFormState> & {
              bindings: Bindings;
              extraContext: any;
            },
        ): Value<PredicateValue> & KeyFormState & { bindings: Bindings } => ({
          ...(_.elementFormStates?.get(elementIndex)?.KeyFormState ||
            GetDefaultKeyFormState()),
          value:
            (_.value.values.get(elementIndex) as ValueTuple)?.values.get(0) ||
            GetDefaultKeyFormValue(),
          disabled: _.disabled,
          bindings: _.bindings,
          extraContext: _.extraContext,
        }),
      )
      .mapState(
        (
          _: BasicUpdater<KeyFormState>,
        ): Updater<MapAbstractRendererState<KeyFormState, ValueFormState>> =>
          MapAbstractRendererState<
            KeyFormState,
            ValueFormState
          >().Updaters.Core.elementFormStates(
            MapRepo.Updaters.upsert(
              elementIndex,
              () => ({
                KeyFormState: GetDefaultKeyFormState(),
                ValueFormState: GetDefaultValueFormState(),
              }),
              (current) => ({
                ...current,
                KeyFormState: _(current.KeyFormState),
              }),
            ),
          ),
      )
      .mapForeignMutationsFromProps<
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueTuple>;
        }
      >(
        (
          props,
        ): {
          onChange: DispatchOnChange<PredicateValue>;
        } => ({
          onChange: (elementUpdater, nestedDelta) => {
            const delta: DispatchDelta = {
              kind: "MapKey",
              value: [elementIndex, nestedDelta],
            };
            props.foreignMutations.onChange(
              Updater((elements: ValueTuple) =>
                PredicateValue.Default.tuple(
                  elements.values.update(
                    elementIndex,
                    PredicateValue.Default.unit(),
                    (_) =>
                      _ == undefined
                        ? _
                        : !PredicateValue.Operations.IsTuple(_)
                          ? _
                          : PredicateValue.Default.tuple(
                              List([
                                elementUpdater(_.values.get(0)!),
                                _.values.get(1)!,
                              ]),
                            ),
                  ),
                ),
              ),
              delta,
            );
            props.setState((_) => ({
              ..._,
              commonFormState: {
                ..._.commonFormState,
                modifiedByUser: true,
              },
              elementFormStates: MapRepo.Updaters.upsert(
                elementIndex,
                () => ({
                  KeyFormState: GetDefaultKeyFormState(),
                  ValueFormState: GetDefaultValueFormState(),
                }),
                (__) => {
                  return {
                    ValueFormState: __.ValueFormState,
                    KeyFormState: {
                      ...__.KeyFormState,
                      commonFormState: {
                        ...__.KeyFormState.commonFormState,
                        modifiedByUser: true,
                      },
                    },
                  };
                },
              )(_.elementFormStates),
            }));
          },
        }),
      );

  const embeddedValueTemplate = (elementIndex: number) =>
    valueTemplate
      .mapContext(
        (
          _: Context &
            Value<ValueTuple> &
            MapAbstractRendererState<KeyFormState, ValueFormState> & {
              bindings: Bindings;
              extraContext: any;
            },
        ): Value<PredicateValue> & ValueFormState & { bindings: Bindings } => {
          return {
            ...(_.elementFormStates?.get(elementIndex)?.ValueFormState ||
              GetDefaultValueFormState()),
            value:
              (_.value.values?.get(elementIndex) as ValueTuple)?.values.get(
                1,
              ) || GetDefaultValueFormValue(),
            disabled: _.disabled,
            bindings: _.bindings,
            extraContext: _.extraContext,
          };
        },
      )
      .mapState(
        (
          _: BasicUpdater<ValueFormState>,
        ): Updater<MapAbstractRendererState<KeyFormState, ValueFormState>> =>
          MapAbstractRendererState<
            KeyFormState,
            ValueFormState
          >().Updaters.Core.elementFormStates(
            MapRepo.Updaters.upsert(
              elementIndex,
              () => ({
                KeyFormState: GetDefaultKeyFormState(),
                ValueFormState: GetDefaultValueFormState(),
              }),
              (current) => ({
                ...current,
                ValueFormState: _(current.ValueFormState),
              }),
            ),
          ),
      )
      .mapForeignMutationsFromProps<
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueTuple>;
        }
      >(
        (
          props,
        ): {
          onChange: DispatchOnChange<PredicateValue>;
        } => ({
          onChange: (elementUpdater, nestedDelta) => {
            const delta: DispatchDelta = {
              kind: "MapValue",
              value: [elementIndex, nestedDelta],
            };
            props.foreignMutations.onChange(
              Updater((elements: ValueTuple) =>
                PredicateValue.Default.tuple(
                  elements.values.update(
                    elementIndex,
                    GetDefaultValueFormValue(),
                    (_) =>
                      _ == undefined
                        ? _
                        : !PredicateValue.Operations.IsTuple(_)
                          ? _
                          : PredicateValue.Default.tuple(
                              List([
                                _.values.get(0)!,
                                elementUpdater(_.values.get(1)!),
                              ]),
                            ),
                  ),
                ),
              ),
              delta,
            );
            props.setState((_) => ({
              ..._,
              commonFormState: {
                ..._.commonFormState,
                modifiedByUser: true,
              },
              elementFormStates: MapRepo.Updaters.upsert(
                elementIndex,
                () => ({
                  KeyFormState: GetDefaultKeyFormState(),
                  ValueFormState: GetDefaultValueFormState(),
                }),
                (__) => ({
                  KeyFormState: __.KeyFormState,
                  ValueFormState: {
                    ...__.ValueFormState,
                    commonFormState: {
                      ...__.ValueFormState.commonFormState,
                      modifiedByUser: true,
                    },
                  },
                }),
              )(_.elementFormStates),
            }));
          },
        }),
      );

  return Template.Default<
    Context & Value<ValueTuple> & { disabled: boolean },
    MapAbstractRendererState<KeyFormState, ValueFormState>,
    ForeignMutationsExpected & { onChange: DispatchOnChange<ValueTuple> },
    MapAbstractRendererView<
      KeyFormState,
      ValueFormState,
      Context,
      ForeignMutationsExpected
    >
  >((props) => {
    return (
      <>
        <props.view
          {...props}
          context={{
            ...props.context,
          }}
          foreignMutations={{
            ...props.foreignMutations,
            add: (_) => {
              const delta: DispatchDelta = {
                kind: "MapAdd",
                keyValue: [
                  GetDefaultKeyFormValue(),
                  GetDefaultValueFormValue(),
                ],
                keyState: GetDefaultKeyFormState(),
                keyType: (props.context.type as MapType<any>).args[0],
                valueState: GetDefaultValueFormState(),
                valueType: (props.context.type as MapType<any>).args[1],
              };
              props.foreignMutations.onChange(
                Updater((list) =>
                  PredicateValue.Default.tuple(
                    ListRepo.Updaters.push<ValueTuple>(
                      PredicateValue.Default.tuple(
                        List([
                          GetDefaultKeyFormValue(),
                          GetDefaultValueFormValue(),
                        ]),
                      ),
                    )(list.values as List<ValueTuple>),
                  ),
                ),
                delta,
              );
            },
            remove: (_) => {
              const delta: DispatchDelta = {
                kind: "MapRemove",
                index: _,
              };
              props.foreignMutations.onChange(
                Updater((list) =>
                  PredicateValue.Default.tuple(
                    ListRepo.Updaters.remove<ValueTuple>(_)(
                      list.values as List<ValueTuple>,
                    ),
                  ),
                ),
                delta,
              );
            },
          }}
          embeddedKeyTemplate={embeddedKeyTemplate}
          embeddedValueTemplate={embeddedValueTemplate}
        />
      </>
    );
  }).any([]);
};
