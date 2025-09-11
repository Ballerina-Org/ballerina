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
} from "../../../../../../../../main";
import {
  OneAbstractRendererForeignMutationsExpected,
  OneAbstractRendererReadonlyContext,
  OneAbstractRendererState,
  OneAbstractRendererView,
  OneAbstractRendererViewForeignMutationsExpected,
} from "./state";
import {
  initializeOneRunner,
  initializeStreamRunner,
  oneTableDebouncerRunner,
  oneTableLoaderRunner,
} from "./coroutines/runner";

/*
 * The clear, set, create and delete callbacks are used when and only when the one is partial (it can have a value of unit or One)
 * This means the one is inside a Sum<unit, One> (or inverse) renderer.
 * Clear and delete are used to set the sum to a left of unit or delete the referenced entity in the one.
 * The sum defines the 'optionality', so when clearing, no delta is needed (the sum will return a delta indicated clearing)
 * and the sum exclusibely controls the updating on the entity value, so no updater is needed.
 * When deleting, the delta is needed to delete the referenced entity in the one and will be nested in the sum's delta, but again
 * no updater is needed.
 * The set and create callbacks are used when the one is inside a Sum whose current value is unit.
 * If the one is not in a Sum<unit, One> (or inverse), then the set and create callbacks are not used.
 * The updater is always needed because we need to know the value of the new selection / creation.
 * The actual implementation and passing down of the callbacks is done in the concrete sum renderer.
 */

export const OneAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags extends BaseFlags = BaseFlags,
  ExtraContext = Unit,
>(
  DetailsRenderer: Template<any, any, any, any>,
  PreviewRenderer: Template<any, any, any, any> | undefined,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  oneEntityType: RecordType<any>,
) => {
  const typedInitializeStreamRunner = initializeStreamRunner<
    CustomPresentationContext,
    Flags,
    ExtraContext
  >();
  const typedInitializeOneRunner = initializeOneRunner<
    CustomPresentationContext,
    Flags,
    ExtraContext
  >();
  const typedOneTableLoaderRunner = oneTableLoaderRunner<
    CustomPresentationContext,
    Flags,
    ExtraContext
  >();
  const typedOneTableDebouncerRunner = oneTableDebouncerRunner<
    CustomPresentationContext,
    Flags,
    ExtraContext
  >();

  // value is Unit -> partial one, dont'run
  // value is Option<none> -> signal to run the initialization
  // value is Option<some> -> there is a value, we do not care about what's inside

  const embeddedDetailsRenderer = (flags: Flags | undefined) =>
    DetailsRenderer.mapContext<
      OneAbstractRendererState &
        OneAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >
    >((_) => {
      if (PredicateValue.Operations.IsUnit(_.value)) {
        return undefined;
      }

      const state =
        _.customFormState?.detailsState ??
        RecordAbstractRendererState.Default.zero();

      return {
        value: _.value.value,
        ...state,
        disabled: _.disabled,
        bindings: _.bindings,
        extraContext: _.extraContext,
        type: oneEntityType,
        customPresentationContext: _.customPresentationContext,
        remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
        typeAncestors: [_.type as DispatchParsedType<any>].concat(
          _.typeAncestors,
        ),
        domNodeAncestorPath: _.domNodeAncestorPath + "[one][details]",
        lookupTypeAncestorNames: _.lookupTypeAncestorNames,
      };
    })
      .mapState(
        (
          _: BasicUpdater<RecordAbstractRendererState>,
        ): BasicUpdater<OneAbstractRendererState> =>
          OneAbstractRendererState.Updaters.Core.customFormState.children.detailsState(
            _,
          ),
      )
      .mapForeignMutationsFromProps<
        OneAbstractRendererViewForeignMutationsExpected<Flags>
      >((props) => ({
        onChange: (
          updater: Option<BasicUpdater<ValueRecord>>,
          nestedDelta: DispatchDelta<Flags>,
        ) => {
          props.setState(
            OneAbstractRendererState.Updaters.Core.commonFormState.children
              .modifiedByUser(replaceWith(true))
              .then(
                OneAbstractRendererState.Updaters.Core.customFormState.children.detailsState(
                  RecordAbstractRendererState.Updaters.Core.commonFormState(
                    DispatchCommonFormState.Updaters.modifiedByUser(
                      replaceWith(true),
                    ),
                  ),
                ),
              ),
          );

          const delta: DispatchDelta<Flags> = {
            kind: "OneValue",
            nestedDelta,
            flags,
            sourceAncestorLookupTypeNames:
              nestedDelta.sourceAncestorLookupTypeNames,
          };

          // The Option component of the one is a lazy load signal. Either the value is provided initially,
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

  const embeddedPreviewRenderer = PreviewRenderer
    ? (value: ValueRecord) => (id: string) => (flags: Flags | undefined) =>
        PreviewRenderer.mapContext<
          OneAbstractRendererState &
            OneAbstractRendererReadonlyContext<
              CustomPresentationContext,
              ExtraContext
            >
        >((_) => {
          const state =
            _.customFormState?.previewStates.get(id) ??
            RecordAbstractRendererState.Default.zero();
          return {
            ...state,
            value,
            disabled: _.disabled,
            bindings: _.bindings,
            extraContext: _.extraContext,
            type: oneEntityType,
            customPresentationContext: _.customPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            typeAncestors: [_.type as DispatchParsedType<any>].concat(
              _.typeAncestors,
            ),
            domNodeAncestorPath: _.domNodeAncestorPath + "[one][preview]",
            lookupTypeAncestorNames: _.lookupTypeAncestorNames,
          };
        })
          .mapState(
            (
              _: BasicUpdater<RecordAbstractRendererState>,
            ): BasicUpdater<OneAbstractRendererState> =>
              OneAbstractRendererState.Updaters.Core.customFormState.children.previewStates(
                MapRepo.Updaters.upsert(
                  id,
                  () => RecordAbstractRendererState.Default.zero(),
                  _,
                ),
              ),
          )
          .mapForeignMutationsFromProps<
            OneAbstractRendererViewForeignMutationsExpected<Flags>
          >((props) => ({
            onChange: (
              updater: Option<BasicUpdater<ValueRecord>>,
              nestedDelta: DispatchDelta<Flags>,
            ) => {
              props.setState(
                OneAbstractRendererState.Updaters.Core.commonFormState.children
                  .modifiedByUser(replaceWith(true))
                  .then(
                    OneAbstractRendererState.Updaters.Core.customFormState.children.detailsState(
                      RecordAbstractRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ),
                    ),
                  ),
              );

              const delta: DispatchDelta<Flags> = {
                kind: "OneValue",
                nestedDelta,
                flags,
                sourceAncestorLookupTypeNames:
                  nestedDelta.sourceAncestorLookupTypeNames,
              };

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
          }))
    : undefined;

  return Template.Default<
    OneAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext>,
    OneAbstractRendererState,
    OneAbstractRendererForeignMutationsExpected<Flags>,
    OneAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[one]";
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

    const maybeId = OneAbstractRendererState.Operations.GetIdFromContext(
      props.context,
    ).MapErrors((_) => _.concat(`\n...${domNodeId}`));

    if (maybeId.kind === "errors") {
      const errorMsg = maybeId.errors.join("\n");
      return <ErrorRenderer message={errorMsg} />;
    }

    return (
      <>
        <IdProvider domNodeId={domNodeId}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId,
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
                  OneAbstractRendererState.Updaters.Core.customFormState.children
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
                        ? OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
                            Sum.Updaters.left(
                              ValueInfiniteStreamState.Updaters.Template.initLoad(),
                            ),
                          )
                        : id,
                    ),
                ),
              setStreamParam: (key: string, _) =>
                props.setState(
                  OneAbstractRendererState.Updaters.Template.streamParam(
                    key,
                    replaceWith(_),
                  ).then(
                    OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
                      Sum.Updaters.left(
                        ValueInfiniteStreamState.Updaters.Core.position(
                          ValueStreamPosition.Updaters.Core.nextStart(
                            replaceWith(0),
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
              loadMore: () =>
                props.setState(
                  OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
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
                  kind: "OneDeleteValue",
                  flags,
                  sourceAncestorLookupTypeNames:
                    props.context.lookupTypeAncestorNames,
                };
                props.foreignMutations.delete &&
                  props.foreignMutations.delete(delta);
              },
              select: (value, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "OneReplace",
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
                  kind: "OneCreateValue",
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
    typedOneTableLoaderRunner,
    typedInitializeOneRunner.mapContextFromProps((props) => ({
      ...props.context,
      onChange: props.foreignMutations.onChange,
    })),
    typedOneTableDebouncerRunner.mapContextFromProps((props) => {
      const maybeId = OneAbstractRendererState.Operations.GetIdFromContext(
        props.context,
      ).MapErrors((_) =>
        _.concat(`\n...${props.context.domNodeAncestorPath + "[one]"}`),
      );

      if (maybeId.kind === "errors") {
        console.error(maybeId.errors.join("\n"));
        return undefined;
      }

      return {
        ...props.context,
        onDebounce: () => {
          props.setState(
            OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
              Sum.Updaters.left(
                ValueInfiniteStreamState.Updaters.Template.reload(
                  // safe because we check for undefined in the runFilter
                  props.context.customFormState.getChunkWithParams!(
                    maybeId.value,
                  )(props.context.customFormState.streamParams.value),
                ),
              ),
            ),
          );
        },
      };
    }),
  ]);
};
