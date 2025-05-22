import { List } from "immutable";

import { MapAbstractRendererState, MapAbstractRendererView } from "./state";
import { Template } from "../../../../../../../template/state";
import {
  PredicateValue,
  Value,
  ValueTuple,
  Updater,
  DispatchDelta,
  BasicUpdater,
  ListRepo,
  Bindings,
  replaceWith,
  DispatchCommonFormState,
  DispatchOnChange,
  IdWrapperProps,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
} from "../../../../../../../../main";
import { FormLabel } from "../../../../../../../../main";
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
    identifiers: { withLauncher: string; withoutLauncher: string };
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
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
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
          identifiers: {
            withLauncher: _.identifiers.withLauncher.concat(
              `[${elementIndex}][key]`,
            ),
            withoutLauncher: _.identifiers.withoutLauncher.concat(
              `[${elementIndex}][key]`,
            ),
          },
        }),
      )
      .mapState(
        (
          _: BasicUpdater<KeyFormState>,
        ): Updater<MapAbstractRendererState<KeyFormState, ValueFormState>> =>
          MapAbstractRendererState<
            KeyFormState,
            ValueFormState
          >().Updaters.Template.upsertElementKeyFormState(
            elementIndex,
            GetDefaultKeyFormState(),
            GetDefaultValueFormState(),
            _,
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
              isWholeEntityMutation: true,
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
            props.setState(
              MapAbstractRendererState<KeyFormState, ValueFormState>()
                .Updaters.Core.commonFormState(
                  DispatchCommonFormState.Updaters.modifiedByUser(
                    replaceWith(true),
                  ),
                )
                .then(
                  MapAbstractRendererState<
                    KeyFormState,
                    ValueFormState
                  >().Updaters.Template.upsertElementKeyFormState(
                    elementIndex,
                    GetDefaultKeyFormState(),
                    GetDefaultValueFormState(),
                    (_) => ({
                      ..._,
                      commonFormState:
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        )(_.commonFormState),
                    }),
                  ),
                ),
            );
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
              identifiers: { withLauncher: string; withoutLauncher: string };
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
            identifiers: {
              withLauncher: _.identifiers.withLauncher.concat(
                `[${elementIndex}][value]`,
              ),
              withoutLauncher: _.identifiers.withoutLauncher.concat(
                `[${elementIndex}][value]`,
              ),
            },
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
          >().Updaters.Template.upsertElementValueFormState(
            elementIndex,
            GetDefaultKeyFormState(),
            GetDefaultValueFormState(),
            _,
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
              isWholeEntityMutation: true,
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
            props.setState(
              MapAbstractRendererState<KeyFormState, ValueFormState>()
                .Updaters.Core.commonFormState(
                  DispatchCommonFormState.Updaters.modifiedByUser(
                    replaceWith(true),
                  ),
                )
                .then(
                  MapAbstractRendererState<
                    KeyFormState,
                    ValueFormState
                  >().Updaters.Template.upsertElementValueFormState(
                    elementIndex,
                    GetDefaultKeyFormState(),
                    GetDefaultValueFormState(),
                    (_) => ({
                      ..._,
                      commonFormState:
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        )(_.commonFormState),
                    }),
                  ),
                ),
            );
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
    if (!PredicateValue.Operations.IsTuple(props.context.value)) {
      console.error(
        `Tuple expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering map field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Tuple value expected for map but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }
    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.identifiers.withoutLauncher,
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
                  isWholeEntityMutation: true, // TODO: check
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
                props.setState(
                  MapAbstractRendererState<
                    KeyFormState,
                    ValueFormState
                  >().Updaters.Core.commonFormState(
                    DispatchCommonFormState.Updaters.modifiedByUser(
                      replaceWith(true),
                    ),
                  ),
                );
              },
              remove: (_) => {
                const delta: DispatchDelta = {
                  kind: "MapRemove",
                  index: _,
                  isWholeEntityMutation: true, // TODO: check
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
                props.setState(
                  MapAbstractRendererState<
                    KeyFormState,
                    ValueFormState
                  >().Updaters.Core.commonFormState(
                    DispatchCommonFormState.Updaters.modifiedByUser(
                      replaceWith(true),
                    ),
                  ),
                );
              },
            }}
            embeddedKeyTemplate={embeddedKeyTemplate}
            embeddedValueTemplate={embeddedValueTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
