import { Map, OrderedMap } from "immutable";
import {
  SimpleCallback,
  Debounce,
  InfiniteStreamLoader,
  id,
  replaceWith,
  AsyncState,
  BasicFun,
  ValidateRunner,
  ValueRecord,
  PredicateValue,
  Delta,
  ParsedType,
  ParsedApplicationType,
} from "../../../../../../main";
import { CoTypedFactory } from "../../../../../coroutines/builder";
import { Debounced } from "../../../../../debounced/state";
import { InfiniteStreamState } from "../../../../../infinite-data-stream/state";
import { Template } from "../../../../../template/state";
import { Value } from "../../../../../value/state";
import { CollectionReference } from "../../../collection/domains/reference/state";
import { FormLabel } from "../../../singleton/domains/form-label/state";
import {
  FieldValidation,
  FieldValidationWithPath,
  OnChange,
} from "../../../singleton/state";
import { SearchableInfiniteStreamState } from "../searchable-infinite-stream/state";
import { InfiniteStreamMultiselectView } from "./state";

export const InfiniteMultiselectDropdownForm = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>(
  validation?: BasicFun<ValueRecord, Promise<FieldValidation>>,
) => {
  const Co = CoTypedFactory<
    Context &
      Value<ValueRecord> & {
        disabled: boolean;
        visible: boolean;
        type: ParsedType<any>;
      },
    SearchableInfiniteStreamState
  >();
  const DebouncerCo = CoTypedFactory<
    Context & { onDebounce: SimpleCallback<void> } & Value<ValueRecord>,
    SearchableInfiniteStreamState
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
        SearchableInfiniteStreamState().Updaters.Core.customFormState.children
          .searchText,
      ),
      DebouncerCo.Wait(0),
    ]),
  );
  const debouncerRunner = DebouncerCo.Template<
    ForeignMutationsExpected & { onChange: OnChange<ValueRecord> }
  >(debouncer, {
    interval: 15,
    runFilter: (props) =>
      Debounced.Operations.shouldCoroutineRun(
        props.context.customFormState.searchText,
      ),
  });
  const loaderRunner = Co.Template<
    ForeignMutationsExpected & { onChange: OnChange<ValueRecord> }
  >(
    InfiniteStreamLoader<CollectionReference>().embed(
      (_) => _.customFormState.stream,
      SearchableInfiniteStreamState().Updaters.Core.customFormState.children
        .stream,
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
        visible: boolean;
        type: ParsedType<any>;
      },
    SearchableInfiniteStreamState,
    ForeignMutationsExpected & {
      onChange: OnChange<ValueRecord>;
    },
    InfiniteStreamMultiselectView<Context, ForeignMutationsExpected>
  >((props) => (
    <>
      <props.view
        {...props}
        context={{
          ...props.context,
          hasMoreValues: !(
            props.context.customFormState.stream.loadedElements.last()
              ?.hasMoreValues == false
          ),
          isLoading: AsyncState.Operations.isLoading(
            props.context.customFormState.stream.loadingMore,
          ),
          availableOptions: props.context.customFormState.stream.loadedElements
            .valueSeq()
            .flatMap((chunk) => chunk.data.valueSeq())
            .toArray(),
        }}
        foreignMutations={{
          ...props.foreignMutations,
          toggleOpen: () =>
            props.setState(
              SearchableInfiniteStreamState()
                .Updaters.Core.customFormState.children.status(
                  replaceWith(
                    props.context.customFormState.status == "closed"
                      ? "open"
                      : "closed",
                  ),
                )
                .then(
                  props.context.customFormState.stream.loadedElements.count() ==
                    0
                    ? SearchableInfiniteStreamState().Updaters.Core.customFormState.children.stream(
                        InfiniteStreamState<CollectionReference>().Updaters.Template.loadMore(),
                      )
                    : id,
                ),
            ),
          clearSelection: () => {
            const delta: Delta = {
              kind: "SetReplace",
              replace: PredicateValue.Default.record(OrderedMap()),
              state: {
                commonFormState: props.context.commonFormState,
                customFormState: props.context.customFormState,
              },
              type: props.context.type,
            };
            props.foreignMutations.onChange(
              ValueRecord.Updaters.clear(),
              delta,
            );
          },
          setSearchText: (_) =>
            props.setState(
              SearchableInfiniteStreamState().Updaters.Template.searchText(
                replaceWith(_),
              ),
            ),
          loadMore: () =>
            props.setState(
              SearchableInfiniteStreamState().Updaters.Core.customFormState.children.stream(
                InfiniteStreamState<CollectionReference>().Updaters.Template.loadMore(),
              ),
            ),
          reload: () =>
            props.setState(
              SearchableInfiniteStreamState().Updaters.Template.searchText(
                replaceWith(""),
              ),
            ),
          toggleSelection: (elementRecord: ValueRecord) => {
            // TODO - updater logic is using old value rather than inside an updater
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

            const delta: Delta = {
              kind: "SetReplace",
              // TODO: Maybe unsafe - check
              replace: updater(props.context.value),
              state: {
                commonFormState: props.context.commonFormState,
                customFormState: props.context.customFormState,
              },
              type: props.context.type,
            };
            props.foreignMutations.onChange(updater, delta);
          },
        }}
      />
    </>
  )).any([
    loaderRunner,
    debouncerRunner.mapContextFromProps((props) => ({
      ...props.context,
      onDebounce: () =>
        props.setState(
          SearchableInfiniteStreamState().Updaters.Core.customFormState.children.stream(
            InfiniteStreamState<CollectionReference>().Updaters.Template.reload(
              props.context.customFormState.getChunk(
                props.context.customFormState.searchText.value,
              ),
            ),
          ),
        ),
    })),
    ValidateRunner<
      Context & { disabled: boolean; visible: boolean; type: ParsedType<any> },
      SearchableInfiniteStreamState,
      ForeignMutationsExpected,
      ValueRecord
    >(
      validation
        ? (_) =>
            validation(_).then(
              FieldValidationWithPath.Default.fromFieldValidation,
            )
        : undefined,
    ),
  ]);
};
