import { Map } from "immutable";
import React from "react";
import {
  AbstractTableRendererState,
  AsyncState,
  BasicUpdater,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  DispatchDelta,
  DispatchParsedType,
  id,
  PredicateValue,
  RecordAbstractRendererState,
  RecordType,
  replaceWith,
  Synchronized,
  Template,
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
  OneTableAbstractRendererView,
} from "./state";
import {
  initializeOneRunner,
  oneTableDebouncerRunner,
  oneTableLoaderRunner,
} from "./coroutines/runner";

export const OneAbstractRenderer = (
  GetDefaultState: () => any,
  DetailsRenderer: Template<
    CommonAbstractRendererReadonlyContext<RecordType<any>, ValueRecord>,
    CommonAbstractRendererState,
    any,
    any
  >,
  PreviewRenderer:
    | Template<
        CommonAbstractRendererReadonlyContext<RecordType<any>, ValueRecord>,
        CommonAbstractRendererState,
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
    if (!_.customFormState.selectedValue.sync.value.value.isSome) {
      return undefined;
    }
    if (
      !PredicateValue.Operations.IsRecord(
        _.customFormState.selectedValue.sync.value.value,
      ) ||
      _.type.args[0].kind !== "record"
    ) {
      console.error(
        "Expected Details renderer to be of record type, but received something else",
      );
      return undefined;
    }
    const state = _.customFormState?.detailsState ?? GetDefaultState();
    return {
      value: _.customFormState.selectedValue.sync.value.value,
      ...state,
      disabled: false, // to do think about
      bindings: _.bindings,
      extraContext: _.extraContext,
      identifiers: {
        withLauncher: _.identifiers.withLauncher.concat(`[details]`),
        withoutLauncher: _.identifiers.withoutLauncher.concat(`[details]`),
      },
      type: _.type.args[0],
    };
  })
    // TO DO: TEST
    .mapState(
      OneAbstractRendererState.Updaters.Core.customFormState.children
        .detailsState,
    )
    .mapForeignMutationsFromProps<{
      onChange: DispatchOnChange<PredicateValue>;
    }>((props) => ({
      onChange: (
        _: BasicUpdater<PredicateValue>,
        nestedDelta: DispatchDelta,
      ) => {
        props.setState(
          OneAbstractRendererState.Updaters.Core.commonFormState.children
            .modifiedByUser(replaceWith(true))
            .then(
              OneAbstractRendererState.Updaters.Core.customFormState.children.detailsState(
                CommonAbstractRendererState.Updaters.Core.commonFormState.children.modifiedByUser(
                  replaceWith(true),
                ),
              ),
            ),
        );

        // TODO, must return the ID in the delta,
        const delta: DispatchDelta = {
          kind: "OptionValue",
          value: nestedDelta,
        };

        props.foreignMutations.onChange(id, delta);
      },
    }));

  const embeddedPreviewRenderer = PreviewRenderer?.mapContext<
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
    if (!_.customFormState.selectedValue.sync.value.value.isSome) {
      return undefined;
    }
    if (
      !PredicateValue.Operations.IsRecord(
        _.customFormState.selectedValue.sync.value.value,
      ) ||
      _.type.args[0].kind !== "record"
    ) {
      return undefined;
    }
    return {
      ..._,
      value: _.customFormState.selectedValue.sync.value.value,
      identifiers: {
        withLauncher: _.identifiers.withLauncher.concat(`[details]`),
        withoutLauncher: _.identifiers.withoutLauncher.concat(`[details]`),
      },
      type: _.type.args[0],
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
        _: BasicUpdater<PredicateValue>,
        nestedDelta: DispatchDelta,
      ) => {},
    }));

  return Template.Default<
    OneAbstractRendererReadonlyContext,
    OneAbstractRendererState,
    {
      onChange: DispatchOnChange<ValueOption>;
    },
    OneTableAbstractRendererView
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
            clearSelection: () => {
              const delta: DispatchDelta = {
                kind: "OptionReplace",
                replace: PredicateValue.Default.option(
                  false,
                  PredicateValue.Default.unit(),
                ),
                state: {
                  commonFormState: props.context.commonFormState,
                  customFormState: props.context.customFormState,
                },
                type: props.context.type,
              };
              props.foreignMutations.onChange(id, delta);
              props.setState(
                OneAbstractRendererState.Updaters.Core.customFormState.children.selectedValue(
                  Synchronized.Updaters.sync(
                    AsyncState.Updaters.toLoaded(unit),
                  ),
                ),
              );
            },
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
