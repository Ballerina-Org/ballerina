import { OrderedMap } from "immutable";
import { SearchableInfiniteStreamAbstractRendererState } from "../searchable-infinite-stream/state";
import { InfiniteStreamMultiselectAbstractRendererView } from "./state";
import {
  AsyncState,
  CollectionReference,
  CoTypedFactory,
  Debounce,
  Debounced,
  DispatchDelta,
  FormLabel,
  id,
  IdWrapperProps,
  InfiniteStreamLoader,
  InfiniteStreamState,
  PredicateValue,
  replaceWith,
  SimpleCallback,
  Template,
  Value,
  ValueRecord,
  DispatchOnChange,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export const InfiniteMultiselectDropdownFormAbstractRenderer = <
  Context extends FormLabel & {
    identifiers: { withLauncher: string; withoutLauncher: string };
  },
  ForeignMutationsExpected,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const Co = CoTypedFactory<
    Context &
      Value<ValueRecord> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
      },
    SearchableInfiniteStreamAbstractRendererState
  >();
  const DebouncerCo = CoTypedFactory<
    Context & { onDebounce: SimpleCallback<void> } & Value<ValueRecord>,
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
    ForeignMutationsExpected & { onChange: DispatchOnChange<ValueRecord> }
  >(debouncer, {
    interval: 15,
    runFilter: (props) =>
      Debounced.Operations.shouldCoroutineRun(
        props.context.customFormState.searchText,
      ),
  });
  const loaderRunner = Co.Template<
    ForeignMutationsExpected & { onChange: DispatchOnChange<ValueRecord> }
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
      Value<ValueRecord> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    SearchableInfiniteStreamAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueRecord>;
    },
    InfiniteStreamMultiselectAbstractRendererView<
      Context,
      ForeignMutationsExpected
    >
  >((props) => {
    if (!PredicateValue.Operations.IsRecord(props.context.value)) {
      console.error(
        `Record expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering searchable infinite stream multiselect field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Record value expected for searchable infinite stream multiselect but got ${JSON.stringify(
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
              isLoading: AsyncState.Operations.isLoading(
                props.context.customFormState.stream.loadingMore,
              ),
              availableOptions:
                props.context.customFormState.stream.loadedElements
                  .valueSeq()
                  .flatMap((chunk) => chunk.data.valueSeq())
                  .toArray(),
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
              clearSelection: () => {
                const delta: DispatchDelta = {
                  kind: "SetReplace",
                  replace: PredicateValue.Default.record(OrderedMap()),
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                  isWholeEntityMutation: false,
                };
                props.foreignMutations.onChange(
                  ValueRecord.Updaters.clear(),
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
              replace: (_: ValueRecord) => {
                const delta: DispatchDelta = {
                  kind: "SetReplace",
                  replace: _,
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                  isWholeEntityMutation: false,
                };
                props.foreignMutations.onChange(replaceWith(_), delta);
              },
              toggleSelection: (elementRecord: ValueRecord) => {
                const updater = props.context.value.fields.has(
                  elementRecord.fields.get("Id")! as string,
                )
                  ? ValueRecord.Updaters.remove(
                      elementRecord.fields.get("Id")! as string,
                    )
                  : ValueRecord.Updaters.set(
                      elementRecord.fields.get("Id")! as string,
                      elementRecord,
                    );

                const delta: DispatchDelta = {
                  kind: "SetReplace",
                  // Maybe unsafe - check
                  replace: updater(props.context.value),
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                  isWholeEntityMutation: false,
                };
                props.foreignMutations.onChange(updater, delta);
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
