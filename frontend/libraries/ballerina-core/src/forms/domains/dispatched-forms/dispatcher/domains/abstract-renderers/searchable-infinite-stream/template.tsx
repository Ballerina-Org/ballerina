import React from "react";
import {
  CollectionReference,
  CoTypedFactory,
  Debounce,
  Debounced,
  Delta,
  DispatchDelta,
  id,
  InfiniteStreamLoader,
  InfiniteStreamState,
  PredicateValue,
  replaceWith,
  SimpleCallback,
  Template,
  Value,
  ValueOption,
} from "../../../../../../../../main";
import { FormLabel } from "../../../../../singleton/domains/form-label/state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchOnChange } from "../../../state";
import {
  SearchableInfiniteStreamAbstractRendererState,
  SearchableInfiniteStreamAbstractRendererView,
} from "./state";

export const SearchableInfiniteStreamAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>() => {
  const Co = CoTypedFactory<
    Context &
      Value<ValueOption> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    SearchableInfiniteStreamAbstractRendererState
  >();
  const DebouncerCo = CoTypedFactory<
    Context & { onDebounce: SimpleCallback<void> } & Value<ValueOption>,
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
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueOption>;
    }
  >(debouncer, {
    interval: 15,
    runFilter: (props) =>
      Debounced.Operations.shouldCoroutineRun(
        props.context.customFormState.searchText,
      ),
  });
  const loaderRunner = Co.Template<
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueOption>;
    }
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
    Context &
      Value<ValueOption> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    SearchableInfiniteStreamAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueOption>;
    },
    SearchableInfiniteStreamAbstractRendererView<
      Context,
      ForeignMutationsExpected
    >
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
        <p>
          {props.context.label && `${props.context.label}: `}RENDER ERROR:
          Option value expected for searchable infinite stream but got something
          else
        </p>
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
                          InfiniteStreamState<CollectionReference>().Updaters.Template.loadMore(),
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
              props.foreignMutations.onChange(
                replaceWith(
                  PredicateValue.Default.option(
                    false,
                    PredicateValue.Default.unit(),
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
              props.foreignMutations.onChange(replaceWith(_), delta);
            },
          }}
        />
      </span>
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
