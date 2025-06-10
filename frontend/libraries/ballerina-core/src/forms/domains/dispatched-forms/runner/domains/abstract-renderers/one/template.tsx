import { Map } from "immutable";
import React from "react";
import {
  AsyncState,
  BasicUpdater,
  CommonAbstractRendererReadonlyContext,
  DispatchCommonFormState,
  DispatchDelta,
  DispatchParsedType,
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
  DispatchOnChange,
  IdWrapperProps,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  Debounced,
  MapRepo,
  Value,
} from "../../../../../../../../main";
import {
  OneAbstractRendererReadonlyContext,
  OneAbstractRendererState,
  OneAbstractRendererView,
} from "./state";
import {
  initializeOneRunner,
  oneTableDebouncerRunner,
  oneTableLoaderRunner,
  reinitializeOneRunner,
} from "./coroutines/runner";

export const OneAbstractRenderer = <Context,>(
  DetailsRenderer: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue
    >,
    RecordAbstractRendererState,
    any,
    any
  >,
  PreviewRenderer:
    | Template<
        CommonAbstractRendererReadonlyContext<
          DispatchParsedType<any>,
          PredicateValue
        >,
        RecordAbstractRendererState,
        any,
        any
      >
    | undefined,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const embeddedDetailsRenderer = DetailsRenderer.mapContext<
    OneAbstractRendererReadonlyContext & OneAbstractRendererState
  >((_) => {
    if (!AsyncState.Operations.hasValue(_.customFormState.selectedValue.sync)) {
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
    if (PredicateValue.Operations.IsUnit(_.value)) {
      return undefined;
    }

    const value = _.customFormState.selectedValue.sync.value.value;
    const state =
      _.customFormState?.detailsState ??
      RecordAbstractRendererState.Default.zero();
    return {
      ..._,
      value,
      ...state,
      disabled: _.disabled,
      bindings: _.bindings,
      extraContext: _.extraContext,
      identifiers: {
        withLauncher: _.identifiers.withLauncher.concat(`[details]`),
        withoutLauncher: _.identifiers.withoutLauncher.concat(`[details]`),
      },
      // this is not correct, type is a lookup -- todo, resolve in the dispatcher
      type: _.type.args as RecordType<any>,
    };
  })
    .mapState((_) =>
      OneAbstractRendererState.Updaters.Core.customFormState.children.detailsState(
        _,
      ),
    )
    .mapForeignMutationsFromProps<{
      onChange: DispatchOnChange<PredicateValue>;
    }>((props) => ({
      onChange: (_: BasicUpdater<ValueRecord>, nestedDelta: DispatchDelta) => {
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
                __.customFormState.selectedValue.sync.value.kind == "errors" ||
                !PredicateValue.Operations.IsRecord(
                  __.customFormState.selectedValue.sync.value.value,
                )
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
                        value: _(
                          __.customFormState.selectedValue.sync.value.value,
                        ),
                      },
                    },
                  },
                },
              };
            }),
        );

        const delta: DispatchDelta = {
          kind: "OneValue",
          nestedDelta,
          isWholeEntityMutation: false,
        };

        props.foreignMutations.onChange(id, delta);
      },
    }));

  const embeddedPreviewRenderer = PreviewRenderer
    ? (value: ValueRecord) =>
        PreviewRenderer.mapContext<
          OneAbstractRendererReadonlyContext & OneAbstractRendererState
        >((_) => {
          const state =
            _.customFormState?.detailsState ??
            RecordAbstractRendererState.Default.zero();
          return {
            ..._,
            ...state,
            value,
            disabled: _.disabled,
            bindings: _.bindings,
            extraContext: _.extraContext,
            identifiers: {
              withLauncher: _.identifiers.withLauncher.concat(`[preview]`),
              withoutLauncher:
                _.identifiers.withoutLauncher.concat(`[preview]`),
            },
            type: _.type.args,
          };
        })
          .mapState(
            OneAbstractRendererState.Updaters.Core.customFormState.children
              .detailsState,
          )
          .mapForeignMutationsFromProps<{
            onChange: DispatchOnChange<PredicateValue>;
          }>((props) => ({
            onChange: (
              _: BasicUpdater<ValueRecord>,
              nestedDelta: DispatchDelta,
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
                      )
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
                              value: _(
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

              const delta: DispatchDelta = {
                kind: "OneValue",
                nestedDelta,
                isWholeEntityMutation: false,
              };

              props.foreignMutations.onChange(id, delta);
            },
          }))
    : undefined;

  return Template.Default<
    OneAbstractRendererReadonlyContext,
    OneAbstractRendererState,
    {
      onChange: DispatchOnChange<ValueOption>;
    },
    OneAbstractRendererView<Context>
  >((props) => {
    const value = props.context.value;
    if (
      !PredicateValue.Operations.IsUnit(value) &&
      (!PredicateValue.Operations.IsOption(value) ||
        (PredicateValue.Operations.IsOption(value) &&
          value.isSome &&
          !PredicateValue.Operations.IsRecord(value.value)))
    ) {
      <ErrorRenderer
        message={`${getLeafIdentifierFromIdentifier(
          props.context.identifiers.withoutLauncher,
        )}: Option of record or unit expected but got ${JSON.stringify(
          props.context.value,
        )}`}
      />;
    }

    const local = props.context.bindings.get("local");
    if (local == undefined) {
      console.error(
        `local binding is undefined when intialising one\n...${props.context.identifiers.withLauncher}`,
      );
      return (
        <ErrorRenderer
          message={`local binding is undefined when intialising one\n...${props.context.identifiers.withLauncher}`}
        />
      );
    }

    if (!PredicateValue.Operations.IsRecord(local)) {
      console.error(
        `local binding is not a record when intialising one\n...${props.context.identifiers.withLauncher}`,
      );
      return (
        <ErrorRenderer
          message={`local binding is not a record when intialising one\n...${props.context.identifiers.withLauncher}`}
        />
      );
    }

    if (!local.fields.has("Id")) {
      console.error(
        `local binding is missing Id (check casing) when intialising one\n...${props.context.identifiers.withLauncher}`,
      );
      return (
        <ErrorRenderer
          message={`local binding is missing Id (check casing) when intialising one\n...${props.context.identifiers.withLauncher}`}
        />
      );
    }

    const Id = local.fields.get("Id")!; // safe because of above check;
    if (!PredicateValue.Operations.IsString(Id)) {
      console.error(
        `local Id is not a string when intialising one\n...${props.context.identifiers.withLauncher}`,
      );
      return (
        <ErrorRenderer
          message={`local Id is not a string when intialising one\n...${props.context.identifiers.withLauncher}`}
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
          <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
            <props.view
              {...props}
              context={{
                ...props.context,
                domNodeId: props.context.identifiers.withoutLauncher,
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
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            kind="initialized"
            context={{
              ...props.context,
              kind: "initialized",
              domNodeId: props.context.identifiers.withoutLauncher,
              value: syncValue,
              hasMoreValues: !(
                props.context.customFormState.stream.loadedElements.last()
                  ?.hasMoreValues == false
              ),
            }}
            // TO DO: Deltas here are on the whole One (selection)
            foreignMutations={{
              ...props.foreignMutations,
              kind: "initialized",
              onChange: (
                _: BasicUpdater<ValueRecord | ValueUnit>,
                nestedDelta: DispatchDelta,
              ) => {
                props.foreignMutations.onChange(id, nestedDelta);
              },
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
              clear: () => {
                const delta: DispatchDelta = {
                  kind: "OneReplace",
                  replace: PredicateValue.Default.unit(),
                  type: props.context.type,
                  isWholeEntityMutation: false,
                };
                props.setState(
                  OneAbstractRendererState.Updaters.Core.customFormState.children
                    .selectedValue(
                      Synchronized.Updaters.sync(
                        AsyncState.Updaters.toLoaded(
                          ValueOrErrors.Default.return(
                            PredicateValue.Default.unit(),
                          ),
                        ),
                      ),
                    )
                    .then(
                      OneAbstractRendererState.Updaters.Template.shouldReinitialize(
                        true,
                      ),
                    ),
                );
                props.foreignMutations.onChange(id, delta);
              },
              select: (_) => {
                const delta: DispatchDelta = {
                  kind: "OneReplace",
                  replace: _,
                  type: props.context.type,
                  isWholeEntityMutation: false,
                };
                props.setState(
                  OneAbstractRendererState.Updaters.Core.customFormState.children
                    .selectedValue(
                      Synchronized.Updaters.sync(
                        AsyncState.Updaters.toLoaded(
                          ValueOrErrors.Default.return(_),
                        ),
                      ),
                    )
                    .then(
                      OneAbstractRendererState.Updaters.Template.shouldReinitialize(
                        true,
                      ),
                    ),
                );
                props.foreignMutations.onChange(id, delta);
              },
              create: (_) => {
                const delta: DispatchDelta = {
                  kind: "OneCreateValue",
                  value: _,
                  type: props.context.type,
                  isWholeEntityMutation: false,
                };
                props.setState(
                  OneAbstractRendererState.Updaters.Core.customFormState.children
                    .selectedValue(
                      Synchronized.Updaters.sync(
                        AsyncState.Updaters.toLoaded(
                          ValueOrErrors.Default.return<
                            ValueRecord | ValueUnit,
                            string
                          >(_),
                        ),
                      ),
                    )
                    .then(
                      OneAbstractRendererState.Updaters.Template.shouldReinitialize(
                        true,
                      ),
                    ),
                );
                props.foreignMutations.onChange(id, delta);
              },
              delete: () => {
                const delta: DispatchDelta = {
                  kind: "OneDeleteValue",
                  isWholeEntityMutation: true,
                };
                props.setState(
                  OneAbstractRendererState.Updaters.Core.customFormState.children
                    .selectedValue(
                      Synchronized.Updaters.sync(
                        AsyncState.Updaters.toLoaded(
                          ValueOrErrors.Default.return(
                            PredicateValue.Default.unit(),
                          ),
                        ),
                      ),
                    )
                    .then(
                      OneAbstractRendererState.Updaters.Template.shouldReinitialize(
                        true,
                      ),
                    ),
                );
                props.foreignMutations.onChange(id, delta);
              },
            }}
            DetailsRenderer={embeddedDetailsRenderer}
            PreviewRenderer={embeddedPreviewRenderer}
          />
        </IdProvider>
      </>
    );
  }).any([
    initializeOneRunner,
    reinitializeOneRunner,
    oneTableLoaderRunner,
    oneTableDebouncerRunner.mapContextFromProps((props) => {
      const local = props.context.bindings.get("local");
      if (local == undefined) {
        console.error(
          `local binding is undefined when intialising one\n...${props.context.identifiers.withLauncher}`,
        );
        return undefined;
      }

      if (!PredicateValue.Operations.IsRecord(local)) {
        console.error(
          `local binding is not a record when intialising one\n...${props.context.identifiers.withLauncher}`,
        );
        return undefined;
      }

      if (!local.fields.has("Id")) {
        console.error(
          `local binding is missing Id (check casing) when intialising one\n...${props.context.identifiers.withLauncher}`,
        );
        return undefined;
      }

      const Id = local.fields.get("Id")!; // safe because of above check;
      if (!PredicateValue.Operations.IsString(Id)) {
        console.error(
          `local Id is not a string when intialising one\n...${props.context.identifiers.withLauncher}`,
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
