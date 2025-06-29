import { List } from "immutable";

import {
  MapAbstractRendererForeignMutationsExpected,
  MapAbstractRendererReadonlyContext,
  MapAbstractRendererState,
  MapAbstractRendererView,
} from "./state";
import { Template } from "../../../../../../../template/state";
import {
  PredicateValue,
  ValueTuple,
  Updater,
  DispatchDelta,
  BasicUpdater,
  ListRepo,
  replaceWith,
  DispatchCommonFormState,
  DispatchOnChange,
  IdWrapperProps,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  Option,
  Unit,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererForeignMutationsExpected,
} from "../../../../../../../../main";
import {
  DispatchParsedType,
  MapType,
  StringSerializedType,
} from "../../../../deserializer/domains/specification/domains/types/state";

export const MapAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
>(
  GetDefaultKeyFormState: () => CommonAbstractRendererState,
  GetDefaultKeyFormValue: () => PredicateValue,
  GetDefaultValueFormState: () => CommonAbstractRendererState,
  GetDefaultValueFormValue: () => PredicateValue,
  keyTemplate: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue,
      CustomPresentationContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>
  >,
  valueTemplate: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue,
      CustomPresentationContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>
  >,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) => {
  const embeddedKeyTemplate =
    (elementIndex: number) => (flags: Flags | undefined) =>
      keyTemplate
        .mapContext(
          (
            _: MapAbstractRendererReadonlyContext<CustomPresentationContext> &
              MapAbstractRendererState,
          ) => ({
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
            domNodeId: _.identifiers.withoutLauncher.concat(
              `[${elementIndex}][key]`,
            ),
            type: _.type.args[0],
            CustomPresentationContext: _.CustomPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
          }),
        )
        .mapState(
          (
            _: BasicUpdater<CommonAbstractRendererState>,
          ): Updater<MapAbstractRendererState> =>
            MapAbstractRendererState.Updaters.Template.upsertElementKeyFormState(
              elementIndex,
              GetDefaultKeyFormState(),
              GetDefaultValueFormState(),
              _,
            ),
        )
        .mapForeignMutationsFromProps<
          MapAbstractRendererForeignMutationsExpected<Flags>
        >((props) => ({
          onChange: (elementUpdater, nestedDelta) => {
            const delta: DispatchDelta<Flags> = {
              kind: "MapKey",
              value: [elementIndex, nestedDelta],
              flags,
            };
            props.foreignMutations.onChange(
              elementUpdater.kind == "l"
                ? Option.Default.none()
                : Option.Default.some(
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
                                      elementUpdater.value(_.values.get(0)!),
                                      _.values.get(1)!,
                                    ]),
                                  ),
                        ),
                      ),
                    ),
                  ),
              delta,
            );
            props.setState(
              MapAbstractRendererState.Updaters.Core.commonFormState(
                DispatchCommonFormState.Updaters.modifiedByUser(
                  replaceWith(true),
                ),
              ).then(
                MapAbstractRendererState.Updaters.Template.upsertElementKeyFormState(
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
        }));

  const embeddedValueTemplate =
    (elementIndex: number) => (flags: Flags | undefined) =>
      valueTemplate
        .mapContext(
          (
            _: MapAbstractRendererReadonlyContext<CustomPresentationContext> &
              MapAbstractRendererState,
          ) => ({
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
            domNodeId: _.identifiers.withoutLauncher.concat(
              `[${elementIndex}][value]`,
            ),
            type: _.type.args[1],
            CustomPresentationContext: _.CustomPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
          }),
        )
        .mapState(
          (
            _: BasicUpdater<CommonAbstractRendererState>,
          ): Updater<MapAbstractRendererState> =>
            MapAbstractRendererState.Updaters.Template.upsertElementValueFormState(
              elementIndex,
              GetDefaultKeyFormState(),
              GetDefaultValueFormState(),
              _,
            ),
        )
        .mapForeignMutationsFromProps<
          MapAbstractRendererForeignMutationsExpected<Flags>
        >(
          (
            props,
          ): {
            onChange: DispatchOnChange<PredicateValue, Flags>;
          } => ({
            onChange: (elementUpdater, nestedDelta) => {
              const delta: DispatchDelta<Flags> = {
                kind: "MapValue",
                value: [elementIndex, nestedDelta],
                flags,
              };
              props.foreignMutations.onChange(
                elementUpdater.kind == "l"
                  ? Option.Default.none()
                  : Option.Default.some(
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
                                        elementUpdater.value(_.values.get(1)!),
                                      ]),
                                    ),
                          ),
                        ),
                      ),
                    ),
                delta,
              );
              props.setState(
                MapAbstractRendererState.Updaters.Core.commonFormState(
                  DispatchCommonFormState.Updaters.modifiedByUser(
                    replaceWith(true),
                  ),
                ).then(
                  MapAbstractRendererState.Updaters.Template.upsertElementValueFormState(
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
    MapAbstractRendererReadonlyContext<CustomPresentationContext> &
      MapAbstractRendererState,
    MapAbstractRendererState,
    MapAbstractRendererForeignMutationsExpected<Flags>,
    MapAbstractRendererView<CustomPresentationContext, Flags>
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
              add: (flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "MapAdd",
                  keyValue: [
                    GetDefaultKeyFormValue(),
                    GetDefaultValueFormValue(),
                  ],
                  keyState: GetDefaultKeyFormState(),
                  keyType: (props.context.type as MapType<any>).args[0],
                  valueState: GetDefaultValueFormState(),
                  valueType: (props.context.type as MapType<any>).args[1],
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(
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
                  ),
                  delta,
                );
                props.setState(
                  MapAbstractRendererState.Updaters.Core.commonFormState(
                    DispatchCommonFormState.Updaters.modifiedByUser(
                      replaceWith(true),
                    ),
                  ),
                );
              },
              remove: (index, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "MapRemove",
                  index,
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(
                    Updater((list) =>
                      PredicateValue.Default.tuple(
                        ListRepo.Updaters.remove<ValueTuple>(index)(
                          list.values as List<ValueTuple>,
                        ),
                      ),
                    ),
                  ),
                  delta,
                );
                props.setState(
                  MapAbstractRendererState.Updaters.Core.commonFormState(
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
