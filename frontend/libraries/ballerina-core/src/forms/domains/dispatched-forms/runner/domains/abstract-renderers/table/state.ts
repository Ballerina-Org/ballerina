import { Map, OrderedMap, Set } from "immutable";

import {
  simpleUpdater,
  BasicUpdater,
  Updater,
  SimpleCallback,
  simpleUpdaterWithChildren,
  MapRepo,
  ValueOrErrors,
  PredicateValue,
  ValueRecord,
  ValueTable,
  replaceWith,
  DispatchTableApiSource,
  DispatchOnChange,
  Unit,
  ValueCallbackWithOptionalFlags,
  VoidCallbackWithOptionalFlags,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  TableType,
  RecordAbstractRendererState,
  TableMethod,
  CommonAbstractRendererViewOnlyReadonlyContext,
  RecordType,
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";
import { BasicFun } from "../../../../../../../fun/state";
import { Template, View } from "../../../../../../../template/state";

import { ValueInfiniteStreamState } from "../../../../../../../value-infinite-data-stream/state";

export type TableAbstractRendererReadonlyContext<
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  TableType<any>,
  ValueTable,
  CustomPresentationContext,
  ExtraContext
> & {
  tableApiSource: DispatchTableApiSource;
  fromTableApiParser: (value: unknown) => ValueOrErrors<PredicateValue, string>;
  tableHeaders: string[];
  columnLabels: Map<string, string | undefined>;
  apiMethods: Array<TableMethod>;
};

export type TableAbstractRendererState = CommonAbstractRendererState & {
  customFormState: {
    selectedRows: Set<string>;
    rowStates: Map<string, RecordAbstractRendererState>;
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
export const TableAbstractRendererState = {
  Default: (): TableAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
    customFormState: {
      initializationStatus: "not initialized",
      selectedRows: Set(),
      selectedDetailRow: undefined,
      streamParams: Debounced.Default(Map()),
      rowStates: Map(),
      // TODO: replace with sum
      getChunkWithParams: undefined as any,
      stream: undefined as any,
      previousRemoteEntityVersionIdentifier: "",
      shouldReinitialize: false,
    },
  }),
  Updaters: {
    Core: {
      ...simpleUpdaterWithChildren<TableAbstractRendererState>()({
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "getChunkWithParams",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "stream",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "streamParams",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "initializationStatus",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "selectedDetailRow",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "selectedRows",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "previousRemoteEntityVersionIdentifier",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "shouldReinitialize",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "rowStates",
        ),
      })("customFormState"),
      ...simpleUpdaterWithChildren<TableAbstractRendererState>()({
        ...simpleUpdater<TableAbstractRendererState["commonFormState"]>()(
          "modifiedByUser",
        ),
      })("commonFormState"),
    },
    Template: {
      searchText: (
        key: string,
        _: BasicUpdater<string>,
      ): Updater<TableAbstractRendererState> =>
        TableAbstractRendererState.Updaters.Core.customFormState.children.streamParams(
          Debounced.Updaters.Template.value(
            MapRepo.Updaters.upsert(key, () => "", _),
          ),
        ),
      loadMore: (): Updater<TableAbstractRendererState> =>
        TableAbstractRendererState.Updaters.Core.customFormState.children.stream(
          ValueInfiniteStreamState.Updaters.Template.loadMore(),
        ),
      shouldReinitialize: (_: boolean) =>
        TableAbstractRendererState.Updaters.Core.customFormState.children.shouldReinitialize(
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
            return [
              key.toString(),
              PredicateValue.Default.record(OrderedMap()),
            ];
          }
          if (!PredicateValue.Operations.IsRecord(parsedRow.value)) {
            console.error("Expected a record");
            return [
              key.toString(),
              PredicateValue.Default.record(OrderedMap()),
            ];
          }
          if (!parsedRow.value.fields.has("Id")) {
            console.error("Expected a record with 'Id' field");
            return [
              key.toString(),
              PredicateValue.Default.record(OrderedMap()),
            ];
          }
          if (typeof parsedRow.value.fields.get("Id")! !== "string") {
            console.error("Id must be a string");
            return [
              key.toString(),
              PredicateValue.Default.record(OrderedMap()),
            ];
          }
          return [parsedRow.value.fields.get("Id")! as string, parsedRow.value];
        }),
      ),
  },
};

export type TableAbstractRendererForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueTable, Flags>;
};

export type TableAbstractRendererViewForeignMutationsExpected<Flags = Unit> = {
  loadMore: SimpleCallback<void>;
  selectDetailView: SimpleCallback<string>;
  clearDetailView: SimpleCallback<void>;
  selectRow: SimpleCallback<string>;
  selectAllRows: SimpleCallback<void>;
  clearRows: SimpleCallback<void>;
  onChange: DispatchOnChange<ValueTable, Flags>;
  add: VoidCallbackWithOptionalFlags<Flags> | undefined;
  remove: ValueCallbackWithOptionalFlags<string, Flags> | undefined;
  moveTo:
    | ((key: string, to: string, flags: Flags | undefined) => void)
    | undefined;
  duplicate: ValueCallbackWithOptionalFlags<string, Flags> | undefined;
};

export type TableAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  TableAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    TableAbstractRendererState & {
      hasMoreValues: boolean;
      tableEntityType: RecordType<any>;
    } & CommonAbstractRendererViewOnlyReadonlyContext,
  TableAbstractRendererState,
  TableAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    TableData: OrderedMap<
      string,
      OrderedMap<
        string,
        (
          flags: Flags | undefined,
        ) => Template<
          TableAbstractRendererReadonlyContext<
            CustomPresentationContext,
            ExtraContext
          > &
            TableAbstractRendererState,
          TableAbstractRendererState,
          TableAbstractRendererForeignMutationsExpected<Flags>
        >
      >
    >;
    DetailsRenderer?: (
      flags: Flags | undefined,
    ) => Template<
      TableAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      > &
        TableAbstractRendererState,
      TableAbstractRendererState,
      TableAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
