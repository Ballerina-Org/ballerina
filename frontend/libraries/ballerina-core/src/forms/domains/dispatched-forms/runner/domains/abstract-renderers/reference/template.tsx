import { Map } from "immutable";
import React from "react";
import {
  BasicUpdater,
  DispatchCommonFormState,
  DispatchDelta,
  id,
  PredicateValue,
  RecordAbstractRendererState,
  RecordType,
  replaceWith,
  Template,
  ValueInfiniteStreamState,
  ValueOption,
  ValueOrErrors,
  ValueRecord,
  ValueUnit,
  IdWrapperProps,
  ErrorRendererProps,
  Debounced,
  Value,
  Option,
  Unit,
  MapRepo,
  DispatchParsedType,
  BaseFlags,
  Sum,
  ValueStreamPosition,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererForeignMutationsExpected,
  Updater,
  RecordAbstractRendererReadonlyContext,
  RecordAbstractRendererForeignMutationsExpected,
  NestedRenderer,
} from "../../../../../../../../main";
import {
  ReferenceAbstractRendererForeignMutationsExpected,
  ReferenceAbstractRendererReadonlyContext,
  ReferenceAbstractRendererState,
  ReferenceAbstractRendererView,
  ReferenceAbstractRendererViewForeignMutationsExpected,
} from "./state";
import {
  initializeReferenceRunner,
  initializeStreamRunnerReference,
  referenceTableDebouncerRunner,
  referenceTableLoaderRunner,
} from "./coroutines/runner";

/*
 * The clear, set, create and delete callbacks are used when and only when the reference is partial (it can have a value of unit or Reference)
 * This means the reference is inside a Sum<unit, Reference> (or inverse) renderer.
 * Clear and delete are used to set the sum to a left of unit or delete the referenced entity in the reference.
 * The sum defines the 'optionality', so when clearing, no delta is needed (the sum will return a delta indicated clearing)
 * and the sum exclusibely controls the updating on the entity value, so no updater is needed.
 * When deleting, the delta is needed to delete the referenced entity in the reference and will be nested in the sum's delta, but again
 * no updater is needed.
 * The set and create callbacks are used when the reference is inside a Sum whose current value is unit.
 * If the reference is not in a Sum<unit, Reference> (or inverse), then the set and create callbacks are not used.
 * The updater is always needed because we need to know the value of the new selection / creation.
 * The actual implementation and passing down of the callbacks is done in the concrete sum renderer.
 */

export const ReferenceAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags extends BaseFlags = BaseFlags,
  ExtraContext = Unit,
>(
  DetailsRenderer: Template<
    RecordAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      RecordAbstractRendererState,
    RecordAbstractRendererState,
    RecordAbstractRendererForeignMutationsExpected<Flags>
  >,
  PreviewRenderer:
    | Template<
        RecordAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        > &
          RecordAbstractRendererState,
        RecordAbstractRendererState,
        RecordAbstractRendererForeignMutationsExpected<Flags>
      >
    | undefined,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  DetailsRendererRaw: NestedRenderer<any>,
  PreviewRendererRaw: NestedRenderer<any> | undefined,
  referenceEntityType: RecordType<any>,
) => {
  const typedInitializeStreamRunner = initializeStreamRunnerReference<
    CustomPresentationContext,
    Flags,
    ExtraContext
  >();
  const typedInitializeReferenceRunner = initializeReferenceRunner<
    CustomPresentationContext,
    Flags,
    ExtraContext
  >();
  const typedReferenceTableLoaderRunner = referenceTableLoaderRunner<
    CustomPresentationContext,
    Flags,
    ExtraContext
  >();
  const typedReferenceTableDebouncerRunner = referenceTableDebouncerRunner<
    CustomPresentationContext,
    Flags,
    ExtraContext
  >();

  // value is Unit -> partial reference, dont'run
  // value is Option<none> -> signal to run the initialization
  // value is Option<some> -> there is a value, we do not care about what's inside

  const embeddedDetailsRenderer = (flags: Flags | undefined) =>
    DetailsRenderer.mapContext<
      ReferenceAbstractRendererState &
        ReferenceAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >
    >((_) => {
      const labelContext =
        CommonAbstractRendererState.Operations.GetLabelContext(
          _.labelContext,
          DetailsRendererRaw,
        );
      if (PredicateValue.Operations.IsUnit(_.value)) {
        return undefined;
      }

      if (!PredicateValue.Operations.IsRecord(_.value.value)) {
        console.error(
          `${_.domNodeAncestorPath + "[reference][details]"}: Record expected but got ${JSON.stringify(_.value.value)}`,
        );
        return undefined;
      }

      const state =
        _.customFormState?.detailsState ??
        RecordAbstractRendererState.Default.zero();

      return {
        value: _.value.value,
        ...state,
        readOnly: _.readOnly || _.globallyReadOnly,
        globallyReadOnly: _.globallyReadOnly,
        locked: _.locked,
        disabled: _.disabled || _.globallyDisabled,
        globallyDisabled: _.globallyDisabled,
        bindings: _.bindings,
        extraContext: _.extraContext,
        type: referenceEntityType,
        customPresentationContext: _.customPresentationContext,
        remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
        typeAncestors: [_.type as DispatchParsedType<any>].concat(
          _.typeAncestors,
        ),
        domNodeAncestorPath: _.domNodeAncestorPath + "[Value]",
        legacy_domNodeAncestorPath:
          _.legacy_domNodeAncestorPath + "[reference][details]",
        predictionAncestorPath: _.predictionAncestorPath + "[Value]",
        layoutAncestorPath: _.layoutAncestorPath + "[reference][details]",
        lookupTypeAncestorNames: _.lookupTypeAncestorNames,
        preprocessedSpecContext: _.preprocessedSpecContext,
        labelContext,
        usePreprocessor: _.usePreprocessor,
      };
    })
      .mapState(
        (
          _: BasicUpdater<RecordAbstractRendererState>,
        ): Updater<ReferenceAbstractRendererState> =>
          ReferenceAbstractRendererState.Updaters.Core.customFormState.children.detailsState(
            _,
          ),
      )
      .mapForeignMutationsFromProps<
        ReferenceAbstractRendererViewForeignMutationsExpected<Flags>
      >((props) => ({
        onChange: (
          updater: Option<BasicUpdater<ValueRecord>>,
          nestedDelta: DispatchDelta<Flags>,
        ) => {
          props.setState(
            ReferenceAbstractRendererState.Updaters.Core.commonFormState.children
              .modifiedByUser(replaceWith(true))
              .then(
                ReferenceAbstractRendererState.Updaters.Core.customFormState.children.detailsState(
                  RecordAbstractRendererState.Updaters.Core.commonFormState(
                    DispatchCommonFormState.Updaters.modifiedByUser(
                      replaceWith(true),
                    ),
                  ),
                ),
              ),
          );

          const delta: DispatchDelta<Flags> = {
            kind: "ReferenceValue",
            nestedDelta,
            flags,
            sourceAncestorLookupTypeNames:
              nestedDelta.sourceAncestorLookupTypeNames,
          };

          // The Option component of the reference is a lazy load signal. Either the value is provided initially,
          // or it is loaded lazily. Here we always update a some, because if the detail renderer is displayed,
          // we must already have a value, and the option is a some.
          props.foreignMutations.onChange(
            updater.kind == "l"
              ? Option.Default.none()
              : Option.Default.some<BasicUpdater<ValueOption | ValueUnit>>(
                  (__: ValueOption | ValueUnit): ValueOption | ValueUnit =>
                    __.kind == "unit"
                      ? ValueUnit.Default()
                      : !PredicateValue.Operations.IsRecord(__.value)
                        ? ValueUnit.Default()
                        : ValueOption.Default.some(updater.value(__.value)),
                ),
            delta,
          );
        },
      }));

  const embeddedPreviewRenderer =
    PreviewRenderer && PreviewRendererRaw
      ? (value: ValueRecord) => (id: string) => (flags: Flags | undefined) =>
          PreviewRenderer.mapContext<
            ReferenceAbstractRendererState &
              ReferenceAbstractRendererReadonlyContext<
                CustomPresentationContext,
                ExtraContext
              >
          >((_) => {
            const labelContext =
              CommonAbstractRendererState.Operations.GetLabelContext(
                _.labelContext,
                PreviewRendererRaw,
              );
            const state =
              _.customFormState?.previewStates.get(id) ??
              RecordAbstractRendererState.Default.zero();
            return {
              ...state,
              value,
              disabled: _.disabled || _.globallyDisabled,
              globallyDisabled: _.globallyDisabled,
              readOnly: _.readOnly || _.globallyReadOnly,
              globallyReadOnly: _.globallyReadOnly,
              locked: _.locked,
              bindings: _.bindings,
              extraContext: _.extraContext,
              type: referenceEntityType,
              customPresentationContext: _.customPresentationContext,
              remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
              typeAncestors: [_.type as DispatchParsedType<any>].concat(
                _.typeAncestors,
              ),
              domNodeAncestorPath: _.domNodeAncestorPath + "[Value]",
              legacy_domNodeAncestorPath:
                _.legacy_domNodeAncestorPath + "[reference][preview]",
              predictionAncestorPath: _.predictionAncestorPath + "[Value]",
              layoutAncestorPath: _.layoutAncestorPath + "[reference][preview]",
              lookupTypeAncestorNames: _.lookupTypeAncestorNames,
              preprocessedSpecContext: _.preprocessedSpecContext,
              labelContext,
              usePreprocessor: _.usePreprocessor,
            };
          })
            .mapState(
              (
                _: BasicUpdater<RecordAbstractRendererState>,
              ): Updater<ReferenceAbstractRendererState> =>
                ReferenceAbstractRendererState.Updaters.Core.customFormState.children.previewStates(
                  MapRepo.Updaters.upsert(
                    id,
                    () => RecordAbstractRendererState.Default.zero(),
                    _,
                  ),
                ),
            )
            .mapForeignMutationsFromProps<
              ReferenceAbstractRendererViewForeignMutationsExpected<Flags>
            >((props) => ({
              onChange: (
                updater: Option<BasicUpdater<ValueRecord>>,
                nestedDelta: DispatchDelta<Flags>,
              ) => {
                props.setState(
                  ReferenceAbstractRendererState.Updaters.Core.commonFormState.children
                    .modifiedByUser(replaceWith(true))
                    .then(
                      ReferenceAbstractRendererState.Updaters.Core.customFormState.children.detailsState(
                        RecordAbstractRendererState.Updaters.Core.commonFormState(
                          DispatchCommonFormState.Updaters.modifiedByUser(
                            replaceWith(true),
                          ),
                        ),
                      ),
                    ),
                );

                const delta: DispatchDelta<Flags> = {
                  kind: "ReferenceValue",
                  nestedDelta,
                  flags,
                  sourceAncestorLookupTypeNames:
                    nestedDelta.sourceAncestorLookupTypeNames,
                };

                props.foreignMutations.onChange(
                  updater.kind == "l"
                    ? Option.Default.none()
                    : Option.Default.some<
                        BasicUpdater<ValueOption | ValueUnit>
                      >(
                        (
                          __: ValueOption | ValueUnit,
                        ): ValueOption | ValueUnit =>
                          __.kind == "unit"
                            ? ValueUnit.Default()
                            : !PredicateValue.Operations.IsRecord(__.value)
                              ? ValueUnit.Default()
                              : ValueOption.Default.some(
                                  updater.value(__.value),
                                ),
                      ),
                  delta,
                );
              },
            }))
      : undefined;

  return Template.Default<
    ReferenceAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext>,
    ReferenceAbstractRendererState,
    ReferenceAbstractRendererForeignMutationsExpected<Flags>,
    ReferenceAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath;
    const legacy_domNodeId = props.context.legacy_domNodeAncestorPath + "[reference]";
    const value = props.context.value;

    if (
      !PredicateValue.Operations.IsUnit(value) &&
      (!PredicateValue.Operations.IsOption(value) ||
        (PredicateValue.Operations.IsOption(value) &&
          value.isSome &&
          !PredicateValue.Operations.IsRecord(value.value)))
    ) {
      return (
        <ErrorRenderer
          message={`${domNodeId}: Option of record or unit expected but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    const maybeId = ReferenceAbstractRendererState.Operations.GetIdFromContext(
      props.context,
    ).MapErrors((_) => _.concat(`\n...${domNodeId}`));

    if (maybeId.kind === "errors") {
      const errorMsg = maybeId.errors.join("\n");
      return <ErrorRenderer message={errorMsg} />;
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
              value,
              hasMoreValues:
                props.context.customFormState.stream.kind === "r"
                  ? false
                  : !!props.context.customFormState.stream.value.loadedElements.last()
                      ?.hasMoreValues,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              toggleOpen: () =>
                props.setState(
                  ReferenceAbstractRendererState.Updaters.Core.customFormState.children
                    .status(
                      replaceWith(
                        props.context.customFormState.status == "closed"
                          ? "open"
                          : "closed",
                      ),
                    )
                    .then(
                      props.context.customFormState.stream.kind === "l" &&
                        props.context.customFormState.stream.value.loadedElements.count() ==
                          0
                        ? ReferenceAbstractRendererState.Updaters.Core.customFormState.children.stream(
                            Sum.Updaters.left(
                              ValueInfiniteStreamState.Updaters.Template.initLoad(),
                            ),
                          )
                        : id,
                    ),
                ),
              setStreamParam: (
                key: string,
                value: string,
                shouldReload: boolean,
              ) =>
                props.setState(
                  ReferenceAbstractRendererState.Updaters.Template.streamParam(
                    key,
                    replaceWith(value),
                    shouldReload,
                  ).then(
                    ReferenceAbstractRendererState.Updaters.Core.customFormState.children.stream(
                      Sum.Updaters.left(
                        ValueInfiniteStreamState.Updaters.Core.position(
                          ValueStreamPosition.Updaters.Core.nextStart(
                            replaceWith(0),
                          ),
                        ).then(
                          ValueInfiniteStreamState.Updaters.Core.position(
                            ValueStreamPosition.Updaters.Core.disallowParamsChange(
                              replaceWith(true),
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
              loadMore: () =>
                props.setState(
                  ReferenceAbstractRendererState.Updaters.Core.customFormState.children.stream(
                    Sum.Updaters.left(
                      ValueInfiniteStreamState.Updaters.Template.loadMore(),
                    ),
                  ),
                ),
              clear: () =>
                // See comment at top of file
                props.foreignMutations.clear && props.foreignMutations.clear(),
              delete: (flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "ReferenceDeleteValue",
                  flags,
                  sourceAncestorLookupTypeNames:
                    props.context.lookupTypeAncestorNames,
                };
                props.foreignMutations.delete &&
                  props.foreignMutations.delete(delta);
              },
              select: (value, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "ReferenceReplace",
                  replace: value,
                  flags,
                  type: props.context.type,
                  sourceAncestorLookupTypeNames:
                    props.context.lookupTypeAncestorNames,
                };

                const updater = replaceWith<ValueUnit | ValueOption>(
                  ValueOption.Default.some(value),
                );

                props.foreignMutations.select &&
                PredicateValue.Operations.IsUnit(props.context.value)
                  ? props.foreignMutations.select(updater, delta)
                  : props.foreignMutations.onChange(
                      Option.Default.some(updater),
                      delta,
                    );
              },
              create: (value, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "ReferenceCreateValue",
                  value,
                  flags,
                  type: props.context.type,
                  sourceAncestorLookupTypeNames:
                    props.context.lookupTypeAncestorNames,
                };

                const updater = replaceWith<ValueUnit | ValueOption>(
                  ValueOption.Default.some(value),
                );

                props.foreignMutations.select &&
                PredicateValue.Operations.IsUnit(props.context.value)
                  ? props.foreignMutations.select(updater, delta)
                  : props.foreignMutations.onChange(
                      Option.Default.some(updater),
                      delta,
                    );
              },
              reinitializeStream: () =>
                props.setState(
                  ReferenceAbstractRendererState.Updaters.Core.customFormState.children.stream(
                    replaceWith(
                      Sum.Default.right<
                        ValueInfiniteStreamState,
                        "not initialized"
                      >("not initialized"),
                    ),
                  ),
                ),
            }}
            DetailsRenderer={
              value.kind === "unit" || value.isSome
                ? embeddedDetailsRenderer
                : undefined
            }
            PreviewRenderer={
              value.kind === "unit" || value.isSome
                ? embeddedPreviewRenderer
                : undefined
            }
          />
        </IdProvider>
      </>
    );
  }).any([
    typedInitializeStreamRunner,
    typedReferenceTableLoaderRunner,
    typedInitializeReferenceRunner.mapContextFromProps((props) => ({
      ...props.context,
      onChange: props.foreignMutations.onChange,
    })),
    typedReferenceTableDebouncerRunner.mapContextFromProps((props) => {
      const maybeId = ReferenceAbstractRendererState.Operations.GetIdFromContext(
        props.context,
      ).MapErrors((_) =>
        _.concat(`\n...${props.context.domNodeAncestorPath + "[reference]"}`),
      );

      if (maybeId.kind === "errors") {
        console.error(maybeId.errors.join("\n"));
        return undefined;
      }

      return {
        ...props.context,
        onDebounce: () => {
          if (props.context.customFormState.streamParams.value[1]) {
            props.setState(
              ReferenceAbstractRendererState.Updaters.Core.customFormState.children.stream(
                Sum.Updaters.left(
                  ValueInfiniteStreamState.Updaters.Template.reload(
                    // safe because we check for undefined in the runFilter
                    props.context.customFormState.getChunkWithParams!(
                      maybeId.value,
                    )(props.context.customFormState.streamParams.value[0]),
                  ),
                ),
              ),
            );
          }
        },
      };
    }),
  ]);
};
