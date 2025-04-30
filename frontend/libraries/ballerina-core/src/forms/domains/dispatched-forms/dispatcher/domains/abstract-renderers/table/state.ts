import { Map, OrderedMap } from "immutable";

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
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";
import { BasicFun } from "../../../../../../../fun/state";
import { Template, View } from "../../../../../../../template/state";
import { Value } from "../../../../../../../value/state";

import { ValueInfiniteStreamState } from "../../../../../../../value-infinite-data-stream/state";
import { DispatchOnChange } from "../../../state";

export type AbstractTableRendererReadonlyContext = {
  tableApiSource: TableApiSource;
  fromTableApiParser: (value: any) => ValueOrErrors<PredicateValue, string>;
  type: ParsedType<any>;
  bindings: Bindings;
  value: ValueTable;
};

export type AbstractTableRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: {
    isInitialized: boolean;
    streamParams: Debounced<Map<string, string>>;
    stream: ValueInfiniteStreamState;
    getChunkWithParams: BasicFun<
      Map<string, string>,
      ValueInfiniteStreamState["getChunk"]
    >;
  };
};
export const AbstractTableRendererState = {
  Default: (): AbstractTableRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {
      isInitialized: false,
      streamParams: Debounced.Default(Map()),
      getChunkWithParams: undefined as any,
      stream: undefined as any,
    },
  }),
  Updaters: {
    Core: {
      ...simpleUpdaterWithChildren<AbstractTableRendererState>()({
        ...simpleUpdater<AbstractTableRendererState["customFormState"]>()(
          "getChunkWithParams",
        ),
        ...simpleUpdater<AbstractTableRendererState["customFormState"]>()(
          "stream",
        ),
        ...simpleUpdater<AbstractTableRendererState["customFormState"]>()(
          "streamParams",
        ),
        ...simpleUpdater<AbstractTableRendererState["customFormState"]>()(
          "isInitialized",
        ),
      })("customFormState"),
      ...simpleUpdaterWithChildren<AbstractTableRendererState>()({
        ...simpleUpdater<AbstractTableRendererState["commonFormState"]>()(
          "modifiedByUser",
        ),
      })("commonFormState"),
    },
    Template: {
      searchText: (
        key: string,
        _: BasicUpdater<string>,
      ): Updater<AbstractTableRendererState> =>
        AbstractTableRendererState.Updaters.Core.customFormState.children.streamParams(
          Debounced.Updaters.Template.value(
            MapRepo.Updaters.upsert(key, () => "", _),
          ),
        ),
      loadMore: (): Updater<AbstractTableRendererState> =>
        AbstractTableRendererState.Updaters.Core.customFormState.children.stream(
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
};
export type AbstractTableRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<ValueOption> &
    AbstractTableRendererState & {
      hasMoreValues: boolean;
      disabled: boolean;
    },
  AbstractTableRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<PredicateValue>;
    toggleOpen: SimpleCallback<void>;
    setStreamParam: SimpleCallback<string>;
    select: SimpleCallback<ValueOption>;
    loadMore: SimpleCallback<void>;
    reload: SimpleCallback<void>;
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
  }
>;
