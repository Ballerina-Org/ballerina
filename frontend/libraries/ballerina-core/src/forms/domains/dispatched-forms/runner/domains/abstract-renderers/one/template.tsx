import { Map } from "immutable";
import React from "react";
import {
  AsyncState,
  BasicUpdater,
  DispatchCommonFormState,
  DispatchDelta,
  id,
  PredicateValue,
  RecordAbstractRendererState,
  RecordType,
  replaceWith,
  Synchronized,
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
  StringSerializedType,
  MapRepo,
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
  oneTableDebouncerRunner,
  oneTableLoaderRunner,
  reinitializeOneRunner,
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
  Flags = Unit,
  ExtraContext = Unit,
>(
  DetailsRenderer: Template<any, any, any, any>,
  PreviewRenderer: Template<any, any, any, any> | undefined,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
  oneEntityType: RecordType<any>,
) => {
  const typedInitializeOneRunner = initializeOneRunner<
    CustomPresentationContext,
    Flags,
    ExtraContext
  >();
  const typedReinitializeOneRunner = reinitializeOneRunner<
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

  const embeddedDetailsRenderer = (flags: Flags | undefined) =>
    DetailsRenderer.mapContext<
      Omit<
        OneAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >,
        "value"
      > & {
        value: ValueRecord | ValueUnit;
      } & OneAbstractRendererState
    >((_) => {
      if (
        !AsyncState.Operations.hasValue(_.customFormState.selectedValue.sync)
      ) {
        return undefined;
      }
      if (_.customFormState.selectedValue.sync.value.kind == "errors") {
        console.error(
          _.customFormState.selectedValue.sync.value.errors
            .join("\n")
            .concat(`\n...When parsing the "one" field value\n...`),
        );
        return undefined;
      }

      const value = _.customFormState.selectedValue.sync.value.value;

      if (PredicateValue.Operations.IsUnit(value)) {
        return undefined;
      }

      const state =
        _.customFormState?.detailsState ??
        RecordAbstractRendererState.Default.zero();

      return {
        value,
        ...state,
        disabled: _.disabled,
        bindings: _.bindings,
        extraContext: _.extraContext,
        type: oneEntityType,
        customPresentationContext: _.customPresentationContext,
        remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
        serializedTypeHierarchy: [SerializedType].concat(
          _.serializedTypeHierarchy,
        ),
        domNodeAncestorPath: _.domNodeAncestorPath + "[one][details]",
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
              )
              .then((__) => {
                if (
                  __.customFormState.selectedValue.sync.kind != "loaded" ||
                  __.customFormState.selectedValue.sync.value.kind ==
                    "errors" ||
                  !PredicateValue.Operations.IsRecord(
                    __.customFormState.selectedValue.sync.value.value,
                  ) ||
                  updater.kind == "l"
                ) {
                  return __;
                }
                return {
                  ...__,
                  customFormState: {
                    ...__.customFormState,
                    selectedValue: {
                      ...__.customFormState.selectedValue,
                      sync: {
                        ...__.customFormState.selectedValue.sync,
                        value: {
                          ...__.customFormState.selectedValue.sync.value,
                          value: updater.value(
                            __.customFormState.selectedValue.sync.value.value,
                          ),
                        },
                      },
                    },
                  },
                };
              }),
          );

          const delta: DispatchDelta<Flags> = {
            kind: "OneValue",
            nestedDelta,
            flags,
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
          Omit<
            OneAbstractRendererReadonlyContext<
              CustomPresentationContext,
              ExtraContext
            >,
            "value"
          > & {
            value: ValueRecord | ValueUnit;
          } & OneAbstractRendererState
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
            serializedTypeHierarchy: [SerializedType].concat(
              _.serializedTypeHierarchy,
            ),
            domNodeAncestorPath: _.domNodeAncestorPath + "[one][preview]",
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
                  )
                  .then((__) => {
                    if (
                      __.customFormState.selectedValue.sync.kind != "loaded" ||
                      __.customFormState.selectedValue.sync.value.kind ==
                        "errors" ||
                      !PredicateValue.Operations.IsRecord(
                        __.customFormState.selectedValue.sync.value.value,
                      ) ||
                      updater.kind == "l"
                    ) {
                      return __;
                    }
                    return {
                      ...__,
                      customFormState: {
                        ...__.customFormState,
                        selectedValue: {
                          ...__.customFormState.selectedValue,
                          sync: {
                            ...__.customFormState.selectedValue.sync,
                            value: {
                              ...__.customFormState.selectedValue.sync.value,
                              value: updater.value(
                                __.customFormState.selectedValue.sync.value
                                  .value,
                              ),
                            },
                          },
                        },
                      },
                    };
                  }),
              );

              const delta: DispatchDelta<Flags> = {
                kind: "OneValue",
                nestedDelta,
                flags,
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
    const completeSerializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    const domNodeId = props.context.domNodeAncestorPath + "[one]";

    const value = props.context.value;
    if (
      !PredicateValue.Operations.IsUnit(value) &&
      (!PredicateValue.Operations.IsOption(value) ||
        (PredicateValue.Operations.IsOption(value) &&
          value.isSome &&
          !PredicateValue.Operations.IsRecord(value.value)))
    ) {
      <ErrorRenderer
        message={`${domNodeId}: Option of record or unit expected but got ${JSON.stringify(
          props.context.value,
        )}`}
      />;
    }

    const local = props.context.bindings.get("local");
    if (local == undefined) {
      console.error(
        `local binding is undefined when intialising one\n
        ...when rendering \n
        ...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`local binding is undefined when intialising one\n...${domNodeId}`}
        />
      );
    }

    if (!PredicateValue.Operations.IsRecord(local)) {
      console.error(
        `local binding is not a record when intialising one\n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`local binding is not a record when intialising one\n...${domNodeId}`}
        />
      );
    }

    if (!local.fields.has("Id")) {
      console.error(
        `local binding is missing Id (check casing) when intialising one\n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`local binding is missing Id (check casing) when intialising one\n...${domNodeId}`}
        />
      );
    }

    const Id = local.fields.get("Id")!; // safe because of above check;
    if (!PredicateValue.Operations.IsString(Id)) {
      console.error(
        `local Id is not a string when intialising one\n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`local Id is not a string when intialising one\n...${domNodeId}`}
        />
      );
    }

    if (
      !AsyncState.Operations.hasValue(
        props.context.customFormState.selectedValue.sync,
      )
    ) {
      return (
        <>
          <IdProvider domNodeId={domNodeId}>
            <props.view
              {...props}
              context={{
                ...props.context,
                domNodeId,
                kind: "uninitialized",
              }}
              kind="uninitialized"
              foreignMutations={{
                kind: "uninitialized",
              }}
            />
          </IdProvider>
        </>
      );
    }

    if (
      props.context.customFormState.selectedValue.sync.value.kind == "errors"
    ) {
      console.error(
        props.context.customFormState.selectedValue.sync.value.errors.join(
          "\n",
        ),
      );
      return <></>;
    }
    const syncValue =
      props.context.customFormState.selectedValue.sync.value.value;

    return (
      <>
        <IdProvider domNodeId={domNodeId}>
          <props.view
            {...props}
            kind="initialized"
            context={{
              ...props.context,
              kind: "initialized",
              domNodeId,
              value: syncValue,
              hasMoreValues:
                !!props.context.customFormState.stream.loadedElements.last()
                  ?.hasMoreValues,
              completeSerializedTypeHierarchy,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              kind: "initialized",
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
                      props.context.customFormState.stream.loadedElements.count() ==
                        0
                        ? OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
                            ValueInfiniteStreamState.Updaters.Template.loadMore(),
                          )
                        : id,
                    ),
                ),
              setStreamParam: (key: string, _) =>
                props.setState(
                  OneAbstractRendererState.Updaters.Template.streamParam(
                    key,
                    replaceWith(_),
                  ),
                ),
              loadMore: () =>
                props.setState(
                  OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
                    ValueInfiniteStreamState.Updaters.Template.loadMore(),
                  ),
                ),
              reload: () =>
                props.setState(
                  OneAbstractRendererState.Updaters.Core.customFormState.children.streamParams(
                    Debounced.Updaters.Template.value(
                      Value.Updaters.value(replaceWith(Map())),
                    ),
                  ),
                ),
              clear: () =>
                // See comment at top of file
                // TODO: test the reinitialization behaviour
                props.foreignMutations.clear &&
                (props.foreignMutations.clear(),
                props.setState(
                  OneAbstractRendererState.Updaters.Core.customFormState.children
                    .selectedValue(
                      Synchronized.Updaters.sync(
                        AsyncState.Updaters.toLoaded(
                          ValueOrErrors.Default.return<
                            ValueRecord | ValueUnit,
                            string
                          >(PredicateValue.Default.unit()),
                        ),
                      ),
                    )
                    .then(
                      OneAbstractRendererState.Updaters.Template.shouldReinitialize(
                        true,
                      ),
                    ),
                )),
              delete: (flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "OneDeleteValue",
                  flags,
                };
                props.foreignMutations.delete &&
                  (props.foreignMutations.delete(delta),
                  props.setState(
                    OneAbstractRendererState.Updaters.Core.customFormState.children
                      .selectedValue(
                        Synchronized.Updaters.sync(
                          AsyncState.Updaters.toLoaded(
                            ValueOrErrors.Default.return<
                              ValueRecord | ValueUnit,
                              string
                            >(PredicateValue.Default.unit()),
                          ),
                        ),
                      )
                      .then(
                        OneAbstractRendererState.Updaters.Template.shouldReinitialize(
                          true,
                        ),
                      ),
                  ));
              },
              select: (value, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "OneReplace",
                  replace: value,
                  flags,
                  type: props.context.type,
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

                props.setState(
                  OneAbstractRendererState.Updaters.Core.customFormState.children
                    .selectedValue(
                      Synchronized.Updaters.sync(
                        AsyncState.Updaters.toLoaded(
                          ValueOrErrors.Default.return<
                            ValueRecord | ValueUnit,
                            string
                          >(value),
                        ),
                      ),
                    )
                    .then(
                      OneAbstractRendererState.Updaters.Template.shouldReinitialize(
                        true,
                      ),
                    ),
                );
              },
              create: (value, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "OneCreateValue",
                  value,
                  flags,
                  type: props.context.type,
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

                props.setState(
                  OneAbstractRendererState.Updaters.Core.customFormState.children
                    .selectedValue(
                      Synchronized.Updaters.sync(
                        AsyncState.Updaters.toLoaded(
                          ValueOrErrors.Default.return<
                            ValueRecord | ValueUnit,
                            string
                          >(value),
                        ),
                      ),
                    )
                    .then(
                      OneAbstractRendererState.Updaters.Template.shouldReinitialize(
                        true,
                      ),
                    ),
                );
              },
            }}
            DetailsRenderer={embeddedDetailsRenderer}
            PreviewRenderer={embeddedPreviewRenderer}
          />
        </IdProvider>
      </>
    );
  }).any([
    typedInitializeOneRunner,
    typedReinitializeOneRunner,
    typedOneTableLoaderRunner,
    typedOneTableDebouncerRunner.mapContextFromProps((props) => {
      const local = props.context.bindings.get("local");
      if (local == undefined) {
        console.error(
          `local binding is undefined when intialising one\n...${props.context.domNodeAncestorPath + "[one]"}`,
        );
        return undefined;
      }

      if (!PredicateValue.Operations.IsRecord(local)) {
        console.error(
          `local binding is not a record when intialising one\n...${props.context.domNodeAncestorPath + "[one]"}`,
        );
        return undefined;
      }

      if (!local.fields.has("Id")) {
        console.error(
          `local binding is missing Id (check casing) when intialising one\n...${props.context.domNodeAncestorPath + "[one]"}`,
        );
        return undefined;
      }

      const Id = local.fields.get("Id")!; // safe because of above check;
      if (!PredicateValue.Operations.IsString(Id)) {
        console.error(
          `local Id is not a string when intialising one\n...${props.context.domNodeAncestorPath + "[one]"}`,
        );
        return undefined;
      }
      return {
        ...props.context,
        onDebounce: () => {
          props.setState(
            OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
              ValueInfiniteStreamState.Updaters.Template.reload(
                props.context.customFormState.getChunkWithParams(Id)(
                  props.context.customFormState.streamParams.value,
                ),
              ),
            ),
          );
        },
      };
    }),
  ]);
};
