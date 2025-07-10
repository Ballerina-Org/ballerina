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
          ) => ({
            ...(_.elementFormStates?.get(elementIndex)?.KeyFormState ||
              GetDefaultKeyFormState()),
            value:
              (_.value.values.get(elementIndex) as ValueTuple)?.values.get(0) ||
              GetDefaultKeyFormValue(),
            disabled: _.disabled,
            locked: _.locked,
            bindings: _.bindings,
            extraContext: _.extraContext,
            type: _.type.args[0],
            customPresentationContext: _.customPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            domNodeAncestorPath:
              _.domNodeAncestorPath + `[map][${elementIndex}][key]`,
            typeAncestors: [_.type as DispatchParsedType<any>].concat(
              _.typeAncestors,
            ),
            lookupTypeAncestorNames: _.lookupTypeAncestorNames,
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
          ) => ({
            ...(_.elementFormStates?.get(elementIndex)?.ValueFormState ||
              GetDefaultValueFormState()),
            value:
              (_.value.values?.get(elementIndex) as ValueTuple)?.values.get(
                1,
              ) || GetDefaultValueFormValue(),
            disabled: _.disabled,
            locked: _.locked,
            bindings: _.bindings,
            extraContext: _.extraContext,
            type: _.type.args[1],
            customPresentationContext: _.customPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            domNodeAncestorPath:
              _.domNodeAncestorPath + `[map][${elementIndex}][value]`,
            typeAncestors: [_.type as DispatchParsedType<any>].concat(
              _.typeAncestors,
            ),
            lookupTypeAncestorNames: _.lookupTypeAncestorNames,
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
    const domNodeId = props.context.domNodeAncestorPath + "[map]";

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
        <IdProvider domNodeId={domNodeId}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId,
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
                const delta: DispatchDelta<Flags> = {
                  kind: "MapRemove",
                  index,
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
