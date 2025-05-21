import { Map, OrderedMap, Set } from "immutable";

import {
  simpleUpdater,
  BasicUpdater,
  Updater,
  SimpleCallback,
  simpleUpdaterWithChildren,
  ValueOption,
  MapRepo,
  ValueOrErrors,
  PredicateValue,
  TableApiSource,
  ParsedType,
  ValueRecord,
  DispatchCommonFormState,
  FormLabel,
  Bindings,
  ValueTable,
  replaceWith,
  DispatchTableApiSource,
  DispatchOnChange,
  Maybe,
  Option,
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";
import { BasicFun } from "../../../../../../../fun/state";
import { Template, View } from "../../../../../../../template/state";
import { Value } from "../../../../../../../value/state";

import { ValueInfiniteStreamState } from "../../../../../../../value-infinite-data-stream/state";

export type AbstractTableRendererReadonlyContext<FilteringAndSortingState> = {
  tableApiSource: DispatchTableApiSource<FilteringAndSortingState>;
  fromTableApiParser: (value: any) => ValueOrErrors<PredicateValue, string>;
  type: ParsedType<any>;
  bindings: Bindings;
  value: ValueTable;
  identifiers: { withLauncher: string; withoutLauncher: string };
  label?: string;
};

export type AbstractTableRendererState<FilteringAndSortingState> = {
  commonFormState: DispatchCommonFormState;
  customFormState: {
    selectedRows: Set<string>;
    selectedDetailRow: [number, string] | undefined;
    isInitialized: boolean;
    streamParams: Debounced<Value<Option<FilteringAndSortingState>>>;
    stream: ValueInfiniteStreamState;
    getChunkWithParams: BasicFun<
      FilteringAndSortingState,
      ValueInfiniteStreamState["getChunk"]
    >;
  };
};
export const AbstractTableRendererState = <FilteringAndSortingState>() => ({
  Default: (): AbstractTableRendererState<FilteringAndSortingState> => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {
      isInitialized: false,
      selectedRows: Set(),
      selectedDetailRow: undefined,
      streamParams: Debounced.Default(Value.Default(Option.Default.none())),
      // TODO: replace with su
      getChunkWithParams: undefined as any,
      stream: undefined as any,
    },
  }),
  Updaters: {
    Core: {
      ...simpleUpdaterWithChildren<
        AbstractTableRendererState<FilteringAndSortingState>
      >()({
        ...simpleUpdater<
          AbstractTableRendererState<FilteringAndSortingState>["customFormState"]
        >()("getChunkWithParams"),
        ...simpleUpdater<
          AbstractTableRendererState<FilteringAndSortingState>["customFormState"]
        >()("stream"),
        ...simpleUpdater<
          AbstractTableRendererState<FilteringAndSortingState>["customFormState"]
        >()("streamParams"),
        ...simpleUpdater<
          AbstractTableRendererState<FilteringAndSortingState>["customFormState"]
        >()("isInitialized"),
        ...simpleUpdater<
          AbstractTableRendererState<FilteringAndSortingState>["customFormState"]
        >()("selectedDetailRow"),
        ...simpleUpdater<
          AbstractTableRendererState<FilteringAndSortingState>["customFormState"]
        >()("selectedRows"),
      })("customFormState"),
      ...simpleUpdaterWithChildren<
        AbstractTableRendererState<FilteringAndSortingState>
      >()({
        ...simpleUpdater<
          AbstractTableRendererState<FilteringAndSortingState>["commonFormState"]
        >()("modifiedByUser"),
      })("commonFormState"),
    },
    Template: {
      // searchText: (
      //   key: string,
      //   _: BasicUpdater<string>,
      // ): Updater<AbstractTableRendererState<FilteringAndSortingState> =>
      //   AbstractTableRendererState<FilteringAndSortingState>().Updaters.Core.customFormState.children.streamParams(
      //     Debounced.Updaters.Template.value(
      //       Value.Updaters.value(MapRepo.Updaters.upsert(key, () => "", _)),
      //     ),
      //   ),
      loadMore: (): Updater<
        AbstractTableRendererState<FilteringAndSortingState>
      > =>
        AbstractTableRendererState<FilteringAndSortingState>().Updaters.Core.customFormState.children.stream(
          ValueInfiniteStreamState.Updaters.Template.loadMore(),
        ),
    },
  },
  // TODO: clean up the streams to accept data as a value or errors
  Operations: {
    tableValuesToValueRecord: (
      values: any,
      fromApiRaw: (value: any) => ValueOrErrors<PredicateValue, string>,
    ): OrderedMap<string, ValueRecord> =>
      OrderedMap(
        Object.entries(values).map(([key, _]) => {
          const parsedRow = fromApiRaw(_);
          if (parsedRow.kind == "errors") {
            console.error(parsedRow.errors.toJS());
            return [key, PredicateValue.Default.record(Map())];
          }
          if (!PredicateValue.Operations.IsRecord(parsedRow.value)) {
            console.error("Expected a record");
            return [key, PredicateValue.Default.record(Map())];
          }
          return [key, parsedRow.value];
        }),
      ),
  },
});
export type AbstractTableRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
  FilteringAndSortingState,
> = View<
  Context &
    Value<ValueOption> &
    AbstractTableRendererState<FilteringAndSortingState> & {
      hasMoreValues: boolean;
      disabled: boolean;
      identifiers: { withLauncher: string; withoutLauncher: string };
    },
  AbstractTableRendererState<FilteringAndSortingState>,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<PredicateValue>;
    toggleOpen: SimpleCallback<void>;
    setStreamParam: SimpleCallback<[string, string | undefined]>;
    select: SimpleCallback<ValueOption>;
    loadMore: SimpleCallback<void>;
    reload: SimpleCallback<void>;
    selectDetailView: SimpleCallback<string>;
    clearDetailView: SimpleCallback<void>;
    selectRow: SimpleCallback<string>;
    selectAllRows: SimpleCallback<void>;
    clearRows: SimpleCallback<void>;
  },
  {
    TableHeaders: string[];
    EmbeddedTableData: OrderedMap<
      string,
      OrderedMap<
        string,
        Template<
          any,
          any,
          {
            onChange: DispatchOnChange<PredicateValue>;
          },
          any
        >
      >
    >;
    DetailsRenderer: Template<any, any, any, any>;
  }
>;
