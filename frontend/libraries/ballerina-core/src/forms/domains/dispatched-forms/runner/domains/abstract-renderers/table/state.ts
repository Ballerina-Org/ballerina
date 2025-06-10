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
  DomNodeIdReadonlyContext,
  DispatchParsedType,
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";
import { BasicFun } from "../../../../../../../fun/state";
import { Template, View } from "../../../../../../../template/state";
import { Value } from "../../../../../../../value/state";

import { ValueInfiniteStreamState } from "../../../../../../../value-infinite-data-stream/state";

export type AbstractTableRendererReadonlyContext = {
  tableApiSource: DispatchTableApiSource;
  fromTableApiParser: (value: any) => ValueOrErrors<PredicateValue, string>;
  type: DispatchParsedType<any>;
  bindings: Bindings;
  value: ValueTable;
  identifiers: { withLauncher: string; withoutLauncher: string };
  label?: string;
  remoteEntityVersionIdentifier: string;
};

export type AbstractTableRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: {
    selectedRows: Set<string>;
    selectedDetailRow: [number, string] | undefined;
    initializationStatus: "not initialized" | "initialized" | "reinitializing";
    streamParams: Debounced<Map<string, string>>;
    stream: ValueInfiniteStreamState;
    getChunkWithParams: BasicFun<
      Map<string, string>,
      ValueInfiniteStreamState["getChunk"]
    >;
    previousRemoteEntityVersionIdentifier: string;
    shouldReinitialize: boolean;
  };
};
export const AbstractTableRendererState = {
  Default: (): AbstractTableRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {
      initializationStatus: "not initialized",
      selectedRows: Set(),
      selectedDetailRow: undefined,
      streamParams: Debounced.Default(Map()),
      // TODO: replace with su
      getChunkWithParams: undefined as any,
      stream: undefined as any,
      previousRemoteEntityVersionIdentifier: "",
      shouldReinitialize: false,
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
          "initializationStatus",
        ),
        ...simpleUpdater<AbstractTableRendererState["customFormState"]>()(
          "selectedDetailRow",
        ),
        ...simpleUpdater<AbstractTableRendererState["customFormState"]>()(
          "selectedRows",
        ),
        ...simpleUpdater<AbstractTableRendererState["customFormState"]>()(
          "previousRemoteEntityVersionIdentifier",
        ),
        ...simpleUpdater<AbstractTableRendererState["customFormState"]>()(
          "shouldReinitialize",
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
      shouldReinitialize: (_: boolean) =>
        AbstractTableRendererState.Updaters.Core.customFormState.children.shouldReinitialize(
          replaceWith(_),
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
            return [key, PredicateValue.Default.record(OrderedMap())];
          }
          if (!PredicateValue.Operations.IsRecord(parsedRow.value)) {
            console.error("Expected a record");
            return [key, PredicateValue.Default.record(OrderedMap())];
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
    DomNodeIdReadonlyContext &
    AbstractTableRendererState & {
      hasMoreValues: boolean;
      disabled: boolean;
      identifiers: { withLauncher: string; withoutLauncher: string };
    },
  AbstractTableRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<PredicateValue>;
    toggleOpen: SimpleCallback<void>;
    setStreamParam: SimpleCallback<string>;
    select: SimpleCallback<ValueOption>;
    loadMore: SimpleCallback<void>;
    reload: SimpleCallback<void>;
    selectDetailView: SimpleCallback<string>;
    clearDetailView: SimpleCallback<void>;
    selectRow: SimpleCallback<string>;
    selectAllRows: SimpleCallback<void>;
    clearRows: SimpleCallback<void>;
    add: SimpleCallback<void>;
    remove: SimpleCallback<string>;
    moveTo: (key: string, to: string) => void;
    duplicate: SimpleCallback<string>;
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
