import React from "react";
import {
  CollectionReference,
  CoTypedFactory,
  Debounce,
  Debounced,
  DispatchDelta,
  id,
  InfiniteStreamLoader,
  InfiniteStreamState,
  PredicateValue,
  replaceWith,
  SimpleCallback,
  Template,
  Value,
  IdWrapperProps,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  Option,
  Unit,
} from "../../../../../../../../main";
import {
  SearchableInfiniteStreamAbstractRendererState,
  SearchableInfiniteStreamAbstractRendererView,
  SearchableInfiniteStreamAbstractRendererReadonlyContext,
  SearchableInfiniteStreamAbstractRendererForeignMutationsExpected,
} from "./state";

export const SearchableInfiniteStreamAbstractRenderer = <
  CustomContext = Unit,
  Flags = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const Co = CoTypedFactory<
    SearchableInfiniteStreamAbstractRendererReadonlyContext<CustomContext>,
    SearchableInfiniteStreamAbstractRendererState
  >();
  const DebouncerCo = CoTypedFactory<
    SearchableInfiniteStreamAbstractRendererReadonlyContext<CustomContext> & {
      onDebounce: SimpleCallback<void>;
    },
    SearchableInfiniteStreamAbstractRendererState
  >();
  const DebouncedCo = CoTypedFactory<
    { onDebounce: SimpleCallback<void> },
    Value<string>
  >();
  const debouncer = DebouncerCo.Repeat(
    DebouncerCo.Seq([
      Debounce<Value<string>, { onDebounce: SimpleCallback<void> }>(
        DebouncedCo.GetState()
          .then((current) => DebouncedCo.Do(() => current.onDebounce()))
          //.SetState(SearchNow.Updaters.reloadsRequested(_ => _ + 1))
          .then((_) => DebouncedCo.Return("success")),
        250,
      ).embed(
        (_) => ({ ..._, ..._.customFormState.searchText }),
        SearchableInfiniteStreamAbstractRendererState.Updaters.Core
          .customFormState.children.searchText,
      ),
      DebouncerCo.Wait(0),
    ]),
  );
  const debouncerRunner = DebouncerCo.Template<
    SearchableInfiniteStreamAbstractRendererForeignMutationsExpected<Flags>
  >(debouncer, {
    interval: 15,
    runFilter: (props) =>
      Debounced.Operations.shouldCoroutineRun(
        props.context.customFormState.searchText,
      ),
  });
  const loaderRunner = Co.Template<
    SearchableInfiniteStreamAbstractRendererForeignMutationsExpected<Flags>
  >(
    InfiniteStreamLoader<CollectionReference>().embed(
      (_) => _.customFormState.stream,
      SearchableInfiniteStreamAbstractRendererState.Updaters.Core
        .customFormState.children.stream,
    ),
    {
      interval: 15,
      runFilter: (props) =>
        InfiniteStreamState().Operations.shouldCoroutineRun(
          props.context.customFormState.stream,
        ),
    },
  );

  return Template.Default<
    SearchableInfiniteStreamAbstractRendererReadonlyContext<CustomContext> &
      SearchableInfiniteStreamAbstractRendererState,
    SearchableInfiniteStreamAbstractRendererState,
    SearchableInfiniteStreamAbstractRendererForeignMutationsExpected<Flags>,
    SearchableInfiniteStreamAbstractRendererView<CustomContext, Flags>
  >((props) => {
    if (!PredicateValue.Operations.IsOption(props.context.value)) {
      console.error(
        `Option expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering searchable infinite stream field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Option value expected for searchable infinite stream but got ${JSON.stringify(
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
              hasMoreValues: !(
                props.context.customFormState.stream.loadedElements.last()
                  ?.hasMoreValues == false
              ),
            }}
            foreignMutations={{
              ...props.foreignMutations,
              toggleOpen: () =>
                props.setState(
                  SearchableInfiniteStreamAbstractRendererState.Updaters.Core.customFormState.children
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
                        ? SearchableInfiniteStreamAbstractRendererState.Updaters.Core.customFormState.children.stream(
                            InfiniteStreamState<CollectionReference>().Updaters.Template.loadInitial(),
                          )
                        : id,
                    ),
                ),
              clearSelection: (flags) => {
                const delta: DispatchDelta<Flags> = {
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
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(
                    replaceWith(
                      PredicateValue.Default.option(
                        false,
                        PredicateValue.Default.unit(),
                      ),
                    ),
                  ),
                  delta,
                );
              },
              setSearchText: (_) =>
                props.setState(
                  SearchableInfiniteStreamAbstractRendererState.Updaters.Template.searchText(
                    replaceWith(_),
                  ),
                ),
              loadMore: () =>
                props.setState(
                  SearchableInfiniteStreamAbstractRendererState.Updaters.Core.customFormState.children.stream(
                    InfiniteStreamState<CollectionReference>().Updaters.Template.loadMore(),
                  ),
                ),
              reload: () =>
                props.setState(
                  SearchableInfiniteStreamAbstractRendererState.Updaters.Template.searchText(
                    replaceWith(""),
                  ),
                ),
              select: (value, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "OptionReplace",
                  replace: value,
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(replaceWith(value)),
                  delta,
                );
              },
            }}
          />
        </IdProvider>
      </>
    );
  }).any([
    loaderRunner,
    debouncerRunner.mapContextFromProps((props) => ({
      ...props.context,
      onDebounce: () =>
        props.setState(
          SearchableInfiniteStreamAbstractRendererState.Updaters.Core.customFormState.children.stream(
            InfiniteStreamState<CollectionReference>().Updaters.Template.reload(
              props.context.customFormState.getChunk(
                props.context.customFormState.searchText.value,
              ),
            ),
          ),
        ),
    })),
  ]);
};
