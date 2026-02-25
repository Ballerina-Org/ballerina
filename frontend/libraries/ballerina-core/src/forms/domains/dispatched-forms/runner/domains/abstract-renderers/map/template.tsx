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
  Option,
  Unit,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererForeignMutationsExpected,
  NestedRenderer,
} from "../../../../../../../../main";
import {
  DispatchParsedType,
  MapType,
} from "../../../../deserializer/domains/specification/domains/types/state";

export const MapAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  GetDefaultKeyFormState: () => CommonAbstractRendererState,
  GetDefaultKeyFormValue: () => PredicateValue,
  GetDefaultValueFormState: () => CommonAbstractRendererState,
  GetDefaultValueFormValue: () => PredicateValue,
  keyTemplate: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue,
      CustomPresentationContext,
      ExtraContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>
  >,
  valueTemplate: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue,
      CustomPresentationContext,
      ExtraContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>
  >,
  KeyRenderer: NestedRenderer<any>,
  ValueRenderer: NestedRenderer<any>,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const embeddedKeyTemplate =
    (elementIndex: number) => (flags: Flags | undefined) =>
      keyTemplate
        .mapContext(
          (
            _: MapAbstractRendererReadonlyContext<
              CustomPresentationContext,
              ExtraContext
            > &
              MapAbstractRendererState,
          ) => {
            const labelContext =
              CommonAbstractRendererState.Operations.GetLabelContext(
                _.labelContext,
                KeyRenderer,
              );
            return {
              ...(_.elementFormStates?.get(elementIndex)?.KeyFormState ||
                GetDefaultKeyFormState()),
              value:
                (_.value.values.get(elementIndex) as ValueTuple)?.values.get(
                  0,
                ) || GetDefaultKeyFormValue(),
              disabled: _.disabled || _.globallyDisabled,
              globallyDisabled: _.globallyDisabled,
              readOnly: _.readOnly || _.globallyReadOnly,
              globallyReadOnly: _.globallyReadOnly,
              locked: _.locked,
              bindings: _.bindings,
              extraContext: _.extraContext,
              type: _.type.args[0],
              customPresentationContext: _.customPresentationContext,
              remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
              domNodeAncestorPath:
                _.domNodeAncestorPath + `[${elementIndex}][Key]`,
              legacy_domNodeAncestorPath:
                _.legacy_domNodeAncestorPath + `[map][${elementIndex}][key]`,
              predictionAncestorPath:
                _.predictionAncestorPath + `[element][Key]`,
              layoutAncestorPath: _.layoutAncestorPath + `[map][key]`,
              typeAncestors: [_.type as DispatchParsedType<any>].concat(
                _.typeAncestors,
              ),
              labelContext,
              lookupTypeAncestorNames: _.lookupTypeAncestorNames,
              preprocessedSpecContext: _.preprocessedSpecContext,
              usePreprocessor: _.usePreprocessor,
            };
          },
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
            const keyValuePair =
              MapAbstractRendererState.Operations.ExtractKeyValuePairFromTuple(
                props.context.value,
                elementIndex,
                props.context.domNodeAncestorPath,
              );
            if (keyValuePair.kind == "errors") {
              console.error(keyValuePair.errors.join("\n"));
              return;
            }
            const { key } = keyValuePair.value;
            const delta: DispatchDelta<Flags> = {
              kind: "MapKey",
              value: [elementIndex, nestedDelta],
              ballerinaValue: {
                oldKey: key,
                newKey:
                  elementUpdater.kind == "l" ? key : elementUpdater.value(key),
              },
              flags,
              sourceAncestorLookupTypeNames:
                nestedDelta.sourceAncestorLookupTypeNames,
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
            _: MapAbstractRendererReadonlyContext<
              CustomPresentationContext,
              ExtraContext
            > &
              MapAbstractRendererState,
          ) => {
            const labelContext =
              CommonAbstractRendererState.Operations.GetLabelContext(
                _.labelContext,
                ValueRenderer,
              );
            return {
              ...(_.elementFormStates?.get(elementIndex)?.ValueFormState ||
                GetDefaultValueFormState()),
              value:
                (_.value.values?.get(elementIndex) as ValueTuple)?.values.get(
                  1,
                ) || GetDefaultValueFormValue(),
              disabled: _.disabled || _.globallyDisabled,
              globallyDisabled: _.globallyDisabled,
              readOnly: _.readOnly || _.globallyReadOnly,
              globallyReadOnly: _.globallyReadOnly,
              locked: _.locked,
              bindings: _.bindings,
              extraContext: _.extraContext,
              type: _.type.args[1],
              labelContext,
              customPresentationContext: _.customPresentationContext,
              remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
              domNodeAncestorPath:
                _.domNodeAncestorPath + `[${elementIndex}][Value]`,
              legacy_domNodeAncestorPath:
                _.legacy_domNodeAncestorPath + `[map][${elementIndex}][value]`,
              predictionAncestorPath:
                _.predictionAncestorPath + `[element][Value]`,
              layoutAncestorPath: _.layoutAncestorPath + `[map][value]`,
              typeAncestors: [_.type as DispatchParsedType<any>].concat(
                _.typeAncestors,
              ),
              lookupTypeAncestorNames: _.lookupTypeAncestorNames,
              preprocessedSpecContext: _.preprocessedSpecContext,
              usePreprocessor: _.usePreprocessor,
            };
          },
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
              const keyValuePair =
                MapAbstractRendererState.Operations.ExtractKeyValuePairFromTuple(
                  props.context.value,
                  elementIndex,
                  props.context.domNodeAncestorPath,
                );
              if (keyValuePair.kind == "errors") {
                console.error(keyValuePair.errors.join("\n"));
                return;
              }
              const { key, value } = keyValuePair.value;
              const delta: DispatchDelta<Flags> = {
                kind: "MapValue",
                value: [elementIndex, nestedDelta],
                ballerinaValue: {
                  key,
                  value,
                },
                flags,
                sourceAncestorLookupTypeNames:
                  nestedDelta.sourceAncestorLookupTypeNames,
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
    MapAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      MapAbstractRendererState,
    MapAbstractRendererState,
    MapAbstractRendererForeignMutationsExpected<Flags>,
    MapAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath;
    const legacy_domNodeId = props.context.legacy_domNodeAncestorPath + "[map]";

    if (!PredicateValue.Operations.IsTuple(props.context.value)) {
      console.error(
        `Tuple expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Tuple value expected but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    return (
      <>
        <IdProvider
          domNodeId={
            props.context.usePreprocessor ? domNodeId : legacy_domNodeId
          }
        >
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId,
              legacy_domNodeId,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              // TODO: this will not work for new ballerina deltas
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
                  sourceAncestorLookupTypeNames:
                    props.context.lookupTypeAncestorNames,
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
                const keyValuePair =
                  MapAbstractRendererState.Operations.ExtractKeyValuePairFromTuple(
                    props.context.value,
                    index,
                    props.context.domNodeAncestorPath,
                  );
                if (keyValuePair.kind == "errors") {
                  console.error(keyValuePair.errors.join("\n"));
                  return;
                }
                const { key } = keyValuePair.value;
                const delta: DispatchDelta<Flags> = {
                  kind: "MapRemove",
                  index,
                  ballerinaValue: {
                    key,
                  },
                  flags,
                  sourceAncestorLookupTypeNames:
                    props.context.lookupTypeAncestorNames,
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
