import { OrderedMap } from "immutable";
import {
  SearchableInfiniteStreamMultiselectAbstractRendererForeignMutationsExpected,
  SearchableInfiniteStreamMultiselectAbstractRendererReadonlyContext,
  SearchableInfiniteStreamMultiselectAbstractRendererState,
  SearchableInfiniteStreamMultiselectAbstractRendererView,
} from "./state";
import {
  AsyncState,
  CollectionReference,
  CoTypedFactory,
  Debounce,
  Debounced,
  DispatchDelta,
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
  ErrorRendererProps,
  Option,
  Unit,
  StringSerializedType,
} from "../../../../../../../../main";

export const InfiniteMultiselectDropdownFormAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) => {
  const Co = CoTypedFactory<
    SearchableInfiniteStreamMultiselectAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    >,
    SearchableInfiniteStreamMultiselectAbstractRendererState
  >();
  const DebouncerCo = CoTypedFactory<
    SearchableInfiniteStreamMultiselectAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > & {
      onDebounce: SimpleCallback<void>;
    },
    SearchableInfiniteStreamMultiselectAbstractRendererState
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
        SearchableInfiniteStreamMultiselectAbstractRendererState.Updaters.Core
          .customFormState.children.searchText,
      ),
      DebouncerCo.Wait(0),
    ]),
  );
  const debouncerRunner = DebouncerCo.Template<
    SearchableInfiniteStreamMultiselectAbstractRendererForeignMutationsExpected<Flags>
  >(debouncer, {
    interval: 15,
    runFilter: (props) =>
      Debounced.Operations.shouldCoroutineRun(
        props.context.customFormState.searchText,
      ),
  });
  const loaderRunner = Co.Template<
    SearchableInfiniteStreamMultiselectAbstractRendererForeignMutationsExpected<Flags>
  >(
    InfiniteStreamLoader<CollectionReference>().embed(
      (_) => _.customFormState.stream,
      SearchableInfiniteStreamMultiselectAbstractRendererState.Updaters.Core
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
    SearchableInfiniteStreamMultiselectAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    >,
    SearchableInfiniteStreamMultiselectAbstractRendererState,
    SearchableInfiniteStreamMultiselectAbstractRendererForeignMutationsExpected<Flags>,
    SearchableInfiniteStreamMultiselectAbstractRendererView<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >
  >((props) => {
    const completeSerializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    const domNodeId =
      props.context.domNodeAncestorPath +
      "[searchableInfiniteStreamMultiselect]";

    if (!PredicateValue.Operations.IsRecord(props.context.value)) {
      console.error(
        `Record expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering searchable infinite stream multiselect field\n...${
          domNodeId
        }`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Record value expected but got ${JSON.stringify(
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
              completeSerializedTypeHierarchy,
              domNodeId,
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
                  SearchableInfiniteStreamMultiselectAbstractRendererState.Updaters.Core.customFormState.children
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
                        ? SearchableInfiniteStreamMultiselectAbstractRendererState.Updaters.Core.customFormState.children.stream(
                            InfiniteStreamState<CollectionReference>().Updaters.Template.loadInitial(),
                          )
                        : id,
                    ),
                ),
              clearSelection: (flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "SetReplace",
                  replace: PredicateValue.Default.record(OrderedMap()),
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(ValueRecord.Updaters.clear()),
                  delta,
                );
              },
              setSearchText: (value) =>
                props.setState(
                  SearchableInfiniteStreamMultiselectAbstractRendererState.Updaters.Template.searchText(
                    replaceWith(value),
                  ),
                ),
              loadMore: () =>
                props.setState(
                  SearchableInfiniteStreamMultiselectAbstractRendererState.Updaters.Core.customFormState.children.stream(
                    InfiniteStreamState<CollectionReference>().Updaters.Template.loadMore(),
                  ),
                ),
              reload: () =>
                props.setState(
                  SearchableInfiniteStreamMultiselectAbstractRendererState.Updaters.Template.searchText(
                    replaceWith(""),
                  ),
                ),
              replace: (value, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "SetReplace",
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
              toggleSelection: (elementRecord: ValueRecord, flags) => {
                props.context.value.fields.has(
                  elementRecord.fields.get("Id")! as string,
                )
                  ? props.foreignMutations.onChange(
                      Option.Default.some(
                        ValueRecord.Updaters.remove(
                          elementRecord.fields.get("Id")! as string,
                        ),
                      ),
                      {
                        kind: "SetRemove",
                        value: elementRecord,
                        state: {
                          commonFormState: props.context.commonFormState,
                          customFormState: props.context.customFormState,
                        },
                        type: props.context.type,
                        flags,
                      },
                    )
                  : props.foreignMutations.onChange(
                      Option.Default.some(
                        ValueRecord.Updaters.set(
                          elementRecord.fields.get("Id")! as string,
                          elementRecord,
                        ),
                      ),
                      {
                        kind: "SetAdd",
                        value: elementRecord,
                        state: {
                          commonFormState: props.context.commonFormState,
                          customFormState: props.context.customFormState,
                        },
                        type: props.context.type,
                        flags,
                      },
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
          SearchableInfiniteStreamMultiselectAbstractRendererState.Updaters.Core.customFormState.children.stream(
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
