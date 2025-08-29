import { Map, OrderedMap, Set, List } from "immutable";

import {
  simpleUpdater,
  Updater,
  SimpleCallback,
  simpleUpdaterWithChildren,
  ValueOrErrors,
  PredicateValue,
  ValueRecord,
  ValueTable,
  replaceWith,
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
  FilterType,
  MapRepo,
  ValueFilter,
  DispatchParsedType,
  CommonAbstractRendererForeignMutationsExpected,
  Value,
  SumNType,
  ValueSumN,
  ValueUnit,
} from "../../../../../../../../main";
import { Template, View } from "../../../../../../../template/state";

export type TableAbstractRendererReadonlyContext<
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  TableType<any>,
  ValueTable,
  CustomPresentationContext,
  ExtraContext
> & {
  tableHeaders: string[];
  columnLabels: Map<string, string | undefined>;
  apiMethods: Array<TableMethod>;
  sorting: Array<string>;
  highlightedFilters: Array<string>;
};

export type TableAbstractRendererSelectedDetailRow =
  | string
  | undefined
  | ValueUnit;

export type TableAbstractRendererState = CommonAbstractRendererState & {
  customFormState: {
    loadingState: "loading" | "loaded" | "error" | "reload from 0";
    loadMore:
      | "load more"
      | "loading more"
      | "don't load more"
      | "error loading more";
    isFilteringInitialized: boolean;
    selectedRows: Set<string>;
    rowStates: Map<string, RecordAbstractRendererState>;
    selectedDetailRow: TableAbstractRendererSelectedDetailRow;
    filters: Map<string, List<ValueFilter>>;
    sorting: Map<string, "Ascending" | "Descending" | undefined>;
    filterAndSortParam: string;
    filterStates: Map<string, List<any>>;
  };
};
export const TableAbstractRendererState = {
  Default: (): TableAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
    customFormState: {
      loadingState: "loading",
      loadMore: "don't load more",
      isFilteringInitialized: false,
      selectedRows: Set(),
      selectedDetailRow: undefined,
      filterAndSortParam: "",
      rowStates: Map(),
      filters: Map(),
      sorting: Map(),
      filterStates: Map(),
    },
  }),
  Updaters: {
    Core: {
      ...simpleUpdaterWithChildren<TableAbstractRendererState>()({
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "filterAndSortParam",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "loadMore",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "filters",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "sorting",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "selectedDetailRow",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "selectedRows",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "rowStates",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "filterStates",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "isFilteringInitialized",
        ),
        ...simpleUpdater<TableAbstractRendererState["customFormState"]>()(
          "loadingState",
        ),
      })("customFormState"),
      ...simpleUpdaterWithChildren<TableAbstractRendererState>()({
        ...simpleUpdater<TableAbstractRendererState["commonFormState"]>()(
          "modifiedByUser",
        ),
      })("commonFormState"),
    },
    Template: {
      updateFilters: (
        filters: Map<string, List<ValueFilter>>,
        filterTypes: Map<string, SumNType<any>>,
        toApiRaw: (
          type: DispatchParsedType<any>,
          value: PredicateValue,
          state: any,
        ) => ValueOrErrors<any, string>,
      ): Updater<TableAbstractRendererState> =>
        Updater((_) =>
          TableAbstractRendererState.Updaters.Core.customFormState.children
            .filters(replaceWith(filters))
            .then(
              TableAbstractRendererState.Updaters.Core.customFormState.children.filterAndSortParam(
                replaceWith(
                  TableAbstractRendererState.Operations.parseFiltersAndSortingToBase64String(
                    filters,
                    filterTypes,
                    _.customFormState.sorting,
                    toApiRaw,
                    _.customFormState.isFilteringInitialized,
                  ),
                ),
              ),
            )
            .then(
              TableAbstractRendererState.Updaters.Core.customFormState.children.loadingState(
                replaceWith<
                  TableAbstractRendererState["customFormState"]["loadingState"]
                >("reload from 0"),
              ),
            )(_),
        ),
      addSorting: (
        columnName: string,
        direction: "Ascending" | "Descending" | undefined,
        filterTypes: Map<string, SumNType<any>>,
        toApiRaw: (
          type: DispatchParsedType<any>,
          value: PredicateValue,
          state: any,
        ) => ValueOrErrors<any, string>,
      ): Updater<TableAbstractRendererState> => {
        const sortingUpdater = MapRepo.Updaters.upsert(
          columnName,
          () => undefined,
          replaceWith(direction),
        );
        return Updater<TableAbstractRendererState>((_) =>
          TableAbstractRendererState.Updaters.Core.customFormState.children
            .sorting(sortingUpdater)
            .then(
              TableAbstractRendererState.Updaters.Core.customFormState.children.filterAndSortParam(
                replaceWith(
                  TableAbstractRendererState.Operations.parseFiltersAndSortingToBase64String(
                    _.customFormState.filters,
                    filterTypes,
                    sortingUpdater(_.customFormState.sorting),
                    toApiRaw,
                    _.customFormState.isFilteringInitialized,
                  ),
                ),
              ),
            )
            .then(
              TableAbstractRendererState.Updaters.Core.customFormState.children.loadingState(
                replaceWith<
                  TableAbstractRendererState["customFormState"]["loadingState"]
                >("reload from 0"),
              ),
            )(_),
        );
      },
      removeSorting: (
        columnName: string,
        filterTypes: Map<string, SumNType<any>>,
        toApiRaw: (
          type: DispatchParsedType<any>,
          value: PredicateValue,
          state: any,
        ) => ValueOrErrors<any, string>,
      ): Updater<TableAbstractRendererState> => {
        const sortingUpdater = MapRepo.Updaters.remove<
          string,
          "Ascending" | "Descending" | undefined
        >(columnName);
        return Updater<TableAbstractRendererState>((_) =>
          TableAbstractRendererState.Updaters.Core.customFormState.children
            .sorting(sortingUpdater)
            .then(
              TableAbstractRendererState.Updaters.Core.customFormState.children.filterAndSortParam(
                replaceWith(
                  TableAbstractRendererState.Operations.parseFiltersAndSortingToBase64String(
                    _.customFormState.filters,
                    filterTypes,
                    sortingUpdater(_.customFormState.sorting),
                    toApiRaw,
                    _.customFormState.isFilteringInitialized,
                  ),
                ),
              ),
            )
            .then(
              TableAbstractRendererState.Updaters.Core.customFormState.children.loadingState(
                replaceWith<
                  TableAbstractRendererState["customFormState"]["loadingState"]
                >("reload from 0"),
              ),
            )(_),
        );
      },
      loadMore: (): Updater<TableAbstractRendererState> =>
        Updater((_) =>
          _.customFormState.loadMore == "loading more"
            ? _
            : TableAbstractRendererState.Updaters.Core.customFormState.children.loadMore(
                replaceWith<
                  TableAbstractRendererState["customFormState"]["loadMore"]
                >("load more"),
              )(_),
        ),
      reloadFrom0: (): Updater<TableAbstractRendererState> =>
        TableAbstractRendererState.Updaters.Core.customFormState.children.loadingState(
          replaceWith<
            TableAbstractRendererState["customFormState"]["loadingState"]
          >("reload from 0"),
        ),
    },
  },
  // TODO: clean up the streams to accept data as a value or errors
  Operations: {
    parseFiltersAndSortingToBase64String: (
      filterValues: Map<string, List<ValueFilter>>,
      sumNFilterTypes: Map<string, SumNType<any>>,
      sorting: Map<string, "Ascending" | "Descending" | undefined>,
      toApiRaw: (
        type: DispatchParsedType<any>,
        value: PredicateValue,
        state: any,
      ) => ValueOrErrors<any, string>,
      isFilteringInitialized: boolean,
    ): string => {
      if (
        !isFilteringInitialized ||
        (filterValues.valueSeq().every((filters) => filters.size == 0) &&
          sorting.size == 0)
      ) {
        return "";
      }
      const filterTypes = sumNFilterTypes.map(
        (sumNType) => sumNType.args as Array<FilterType<any>>,
      );
      const parsedFilters = filterValues.map((filters, columnName) =>
        filters.map((filter) => {
          const filterTypeIndex = filterTypes
            .get(columnName)
            ?.findIndex((f) => f.kind == filter.kind);
          if (filterTypeIndex == undefined || filterTypeIndex < 0) {
            console.error(
              `filter ${filter.kind} type not found for column ${columnName}`,
            );
            return ValueOrErrors.Default.throwOne(
              `filter ${filter.kind} type not found for column ${columnName}`,
            );
          }
          const filterType = sumNFilterTypes.get(columnName);

          if (!filterType) {
            console.error(
              `filter ${filter.kind} type not found for column ${columnName}`,
            );
            return ValueOrErrors.Default.throwOne(
              `filter ${filter.kind} type not found for column ${columnName}`,
            );
          }

          const sumValue = ValueSumN.Default(
            filterTypeIndex,
            filterType.args.length,
            filter,
          );

          return toApiRaw(filterType, sumValue, {});
        }),
      );
      if (
        parsedFilters.some((filter) => filter.some((f) => f.kind == "errors"))
      ) {
        console.error(
          "error parsing filters to api",
          parsedFilters
            .filter((filter) => filter.some((f) => f.kind == "errors"))
            .toJS(),
        );
        return "";
      }

      // TODO: Deal with this monadically
      const parsedFiltersValues = parsedFilters
        .map((filter) => filter.map((f) => (f as Value<PredicateValue>).value))
        .toJS();

      const valueSorting = sorting
        .entrySeq()
        .filter(([_, direction]) => direction != undefined)
        .map(([columnName, direction]) =>
          PredicateValue.Default.tuple(List([columnName, direction as string])),
        );

      const parsedValueSorting = ValueOrErrors.Operations.All(
        valueSorting
          .map((sorting) =>
            toApiRaw(
              DispatchParsedType.Default.tuple([
                {
                  kind: "primitive",
                  name: "string",
                  asString: () => "string",
                },
                {
                  kind: "primitive",
                  name: "string",
                  asString: () => "string",
                },
              ]),
              sorting,
              {},
            ),
          )
          .toList(),
      );

      const finalSorting =
        parsedValueSorting.kind == "errors"
          ? []
          : parsedValueSorting.value.toArray();

      const params = {
        Filters: parsedFiltersValues,
        Sorting: finalSorting,
      };
      return btoa(JSON.stringify(params));
    },
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
  updateFilters: (filters: Map<string, List<ValueFilter>>) => void;
  addSorting: (
    columnName: string,
    direction: "Ascending" | "Descending" | undefined,
  ) => void;
  removeSorting: (columnName: string) => void;
  reloadFrom0: () => void;
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
    AllowedFilters: Map<
      string,
      {
        template: (
          index: number,
        ) => Template<
          CommonAbstractRendererReadonlyContext<
            DispatchParsedType<any>,
            PredicateValue,
            CustomPresentationContext,
            ExtraContext
          > &
            TableAbstractRendererState,
          TableAbstractRendererState,
          CommonAbstractRendererForeignMutationsExpected
        >;
        type: DispatchParsedType<any>;
        filters: Array<FilterType<any>>;
        GetDefaultValue: () => PredicateValue;
        GetDefaultState: () => CommonAbstractRendererState;
      }
    >;
    AllowedSorting: Array<string>;
    HighlightedFilters: Array<string>;
    isFilteringSortAndLoadingEnabled: boolean;
  }
>;
