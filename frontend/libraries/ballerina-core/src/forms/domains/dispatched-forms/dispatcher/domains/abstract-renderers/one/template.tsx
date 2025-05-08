import { Map } from "immutable";
import React from "react";
import {
  AbstractTableRendererState,
  AsyncState,
  BasicUpdater,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
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
  Unit,
  unit,
  ValueInfiniteStreamState,
  ValueOption,
  ValueOrErrors,
  ValueRecord,
} from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";
import {
  OneAbstractRendererReadonlyContext,
  OneAbstractRendererState,
  OneAbstractRendererView,
} from "./state";
import {
  initializeOneRunner,
  oneTableDebouncerRunner,
  oneTableLoaderRunner,
} from "./coroutines/runner";

export const OneAbstractRenderer = (
  DetailsRenderer: Template<
    CommonAbstractRendererReadonlyContext<RecordType<any>, ValueRecord>,
    RecordAbstractRendererState,
    any,
    any
  >,
  PreviewRenderer:
    | Template<
        CommonAbstractRendererReadonlyContext<RecordType<any>, ValueRecord>,
        RecordAbstractRendererState,
        any,
        any
      >
    | undefined,
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

    if (
      !PredicateValue.Operations.IsRecord(
        _.customFormState.selectedValue.sync.value.value.value,
      )
    ) {
      console.error(
        "Expected Details renderer to be of record type, but received something else",
      );
      return undefined;
    }
    const value = _.customFormState.selectedValue.sync.value.value.value;
    const state =
      _.customFormState?.detailsState ??
      RecordAbstractRendererState.Default.zero();
    return {
      value,
      ...state,
      disabled: false, // to do think about
      bindings: _.bindings,
      extraContext: _.extraContext,
      identifiers: {
        withLauncher: _.identifiers.withLauncher.concat(`[details]`),
        withoutLauncher: _.identifiers.withoutLauncher.concat(`[details]`),
      },
      // this is not correct, type is a lookup -- todo, resolve in the dispatcher
      type: _.type.args[0] as RecordType<any>,
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
                !__.customFormState.selectedValue.sync.value.value.isSome ||
                !PredicateValue.Operations.IsRecord(
                  __.customFormState.selectedValue.sync.value.value.value,
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
                        value: {
                          ...__.customFormState.selectedValue.sync.value.value,
                          value: _(
                            __.customFormState.selectedValue.sync.value.value
                              .value,
                          ),
                        },
                      },
                    },
                  },
                },
              };
            }),
        );

        // TODO, must return the ID in the delta,
        const delta: DispatchDelta = {
          kind: "OptionValue",
          value: nestedDelta,
        };

        props.foreignMutations.onChange(id, delta);
      },
    }));

  const embeddedPreviewRenderer = (previewValue: ValueRecord) =>
    PreviewRenderer?.mapContext<
      OneAbstractRendererReadonlyContext & OneAbstractRendererState
    >((_) => {
      // const value = _.customFormState.selectedValue.sync.value.value.value;
      const state =
        _.customFormState?.detailsState ??
        RecordAbstractRendererState.Default.zero();
      return {
        ..._,
        ...state,
        value: previewValue,
        disabled: false, // to do think about
        bindings: _.bindings,
        extraContext: _.extraContext,
        identifiers: {
          withLauncher: _.identifiers.withLauncher.concat(`[preview]`),
          withoutLauncher: _.identifiers.withoutLauncher.concat(`[preview]`),
        },
        type: _.type.args[0] as RecordType<any>,
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
                  !__.customFormState.selectedValue.sync.value.value.isSome ||
                  !PredicateValue.Operations.IsRecord(
                    __.customFormState.selectedValue.sync.value.value.value,
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
                          value: {
                            ...__.customFormState.selectedValue.sync.value
                              .value,
                            value: _(
                              __.customFormState.selectedValue.sync.value.value
                                .value,
                            ),
                          },
                        },
                      },
                    },
                  },
                };
              }),
          );

          // TODO, must return the ID in the delta,
          const delta: DispatchDelta = {
            kind: "OptionValue",
            value: nestedDelta,
          };

          props.foreignMutations.onChange(id, delta);
        },
      }));

  return Template.Default<
    OneAbstractRendererReadonlyContext,
    OneAbstractRendererState,
    {
      onChange: DispatchOnChange<ValueOption>;
    },
    OneAbstractRendererView
  >((props) => {
    if (
      !PredicateValue.Operations.IsOption(props.context.value) &&
      !PredicateValue.Operations.IsUnit(props.context.value)
    ) {
      console.error(
        `Option or unit  expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering "one" field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <></>
        // <p>
        //   {props.context.label && `${props.context.label}: `}RENDER ERROR: Option
        //   value expected for "one" field but got something else
        // </p>
      );
    }
    return (
      <span
        className={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
      >
        <props.view
          {...props}
          context={{
            ...props.context,
            hasMoreValues: !(
              props.context.customFormState.stream.loadedElements.last()
                ?.hasMoreValues == false
            ),
          }}
          // TO DO: Deltas here are on the whole One (selection)
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
                    props.context.customFormState.stream.loadedElements.count() ==
                      0
                      ? OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
                          ValueInfiniteStreamState.Updaters.Template.loadMore(),
                        )
                      : id,
                  ),
              ),
            // clearSelection: () => {
            //   const delta: DispatchDelta = {
            //     kind: "OptionReplace",
            //     replace: PredicateValue.Default.option(
            //       false,
            //       PredicateValue.Default.unit(),
            //     ),
            //     state: {
            //       commonFormState: props.context.commonFormState,
            //       customFormState: props.context.customFormState,
            //     },
            //     type: props.context.type,
            //   };
            //   props.foreignMutations.onChange(id, delta);
            //   props.setState(
            //     OneAbstractRendererState.Updaters.Core.customFormState.children.selectedValue(
            //       Synchronized.Updaters.sync(
            //         AsyncState.Updaters.toLoaded(unit),
            //       ),
            //     ),
            //   );
            // },
            setSearchText: (_) =>
              props.setState(
                OneAbstractRendererState.Updaters.Template.searchText(
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
                OneAbstractRendererState.Updaters.Template.searchText(
                  replaceWith(""),
                ),
              ),
            select: (_) => {
              const delta: DispatchDelta = {
                kind: "OptionReplace",
                replace: _,
                state: {
                  commonFormState: props.context.commonFormState,
                  customFormState: props.context.customFormState,
                },
                type: props.context.type,
              };
              props.setState(
                OneAbstractRendererState.Updaters.Core.customFormState.children.selectedValue(
                  Synchronized.Updaters.sync(
                    AsyncState.Updaters.toLoaded(
                      ValueOrErrors.Default.return(_),
                    ),
                  ),
                ),
              );
              props.foreignMutations.onChange(id, delta);
            },
          }}
          DetailsRenderer={embeddedDetailsRenderer}
          PreviewRenderer={embeddedPreviewRenderer}
        />
      </span>
    );
  }).any([
    initializeOneRunner,
    oneTableLoaderRunner,
    oneTableDebouncerRunner.mapContextFromProps((props) => ({
      ...props.context,
      onDebounce: () =>
        props.setState(
          OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
            ValueInfiniteStreamState.Updaters.Template.reload(
              props.context.customFormState.getChunkWithParams(
                props.context.customFormState.searchText.value,
              )(Map()),
            ),
          ),
        ),
    })),
  ]);
};
