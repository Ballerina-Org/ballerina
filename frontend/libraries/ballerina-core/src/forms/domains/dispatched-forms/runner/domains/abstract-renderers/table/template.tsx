import { List, Map, OrderedMap, Set } from "immutable";
import {
  BasicUpdater,
  id,
  PredicateValue,
  TableAbstractRendererState,
  TableLayout,
  Expr,
  PredicateVisibleColumns,
  DispatchDelta,
  ValueOrErrors,
  TableAbstractRendererReadonlyContext,
  replaceWith,
  ValueRecord,
  DispatchCommonFormState,
  RecordAbstractRendererState,
  IdWrapperProps,
  ErrorRendererProps,
  Option,
  Unit,
  TableAbstractRendererForeignMutationsExpected,
  DispatchParsedType,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererForeignMutationsExpected,
  TableAbstractRendererView,
  RecordAbstractRendererReadonlyContext,
  RecordAbstractRendererForeignMutationsExpected,
  MapRepo,
  ValueTable,
  RecordType,
  TableAbstractRendererSelectedDetailRow,
  ValueFilter,
  FilterType,
  ListRepo,
  Updater,
  SumNType,
  DispatchTableApiSource,
  ValueUnit,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import {
  TableInfiniteLoaderRunner,
  TableInitialiseFiltersAndSortingRunner,
  TableInitialiseTableRunner,
} from "./coroutines/runner";

export const TableAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  CellTemplates: Map<
    string,
    {
      template: Template<
        CommonAbstractRendererReadonlyContext<
          DispatchParsedType<any>,
          PredicateValue,
          CustomPresentationContext,
          ExtraContext
        >,
        CommonAbstractRendererState,
        CommonAbstractRendererForeignMutationsExpected<Flags>
      >;
      label?: string;
      disabled?: Expr;
      GetDefaultValue: () => PredicateValue;
      GetDefaultState: () => CommonAbstractRendererState;
    }
  >,
  DetailsRenderer:
    | Template<
        RecordAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >,
        RecordAbstractRendererState,
        RecordAbstractRendererForeignMutationsExpected<Flags>
      >
    | undefined,
  Layout: PredicateVisibleColumns,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  TableEntityType: RecordType<any>,
  Filters: Map<
    string,
    {
      template: Template<
        CommonAbstractRendererReadonlyContext<
          DispatchParsedType<any>,
          PredicateValue,
          CustomPresentationContext,
          ExtraContext
        > &
          CommonAbstractRendererState,
        CommonAbstractRendererState,
        CommonAbstractRendererForeignMutationsExpected
      >;
      type: DispatchParsedType<any>;
      GetDefaultValue: () => PredicateValue;
      GetDefaultState: () => CommonAbstractRendererState;
      filters: SumNType<any>;
    }
  >,
  parseToApiByType: (
    type: DispatchParsedType<any>,
    value: PredicateValue,
    state: any,
  ) => ValueOrErrors<any, string>,
  parseFromApiByType: (
    type: DispatchParsedType<any>,
  ) => (raw: any) => ValueOrErrors<PredicateValue, string>,
  fromTableApiParser: (value: unknown) => ValueOrErrors<PredicateValue, string>,
  tableApiSource: DispatchTableApiSource,
): Template<
  TableAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    TableAbstractRendererState,
  TableAbstractRendererState,
  TableAbstractRendererForeignMutationsExpected<Flags>,
  TableAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
> => {
  const InstantiatedInfiniteLoaderRunner = TableInfiniteLoaderRunner<
    CustomPresentationContext,
    ExtraContext
  >(tableApiSource, fromTableApiParser);
  const InstantiatedInitialiseFiltersAndSortingRunner =
    TableInitialiseFiltersAndSortingRunner<
      CustomPresentationContext,
      ExtraContext
    >(
      Filters.map(({ filters }) => filters),
      tableApiSource,
      parseFromApiByType,
      parseToApiByType,
    );
  const InstantiatedInitialiseTableRunner = TableInitialiseTableRunner<
    CustomPresentationContext,
    ExtraContext
  >(tableApiSource, fromTableApiParser);

  const embedCellTemplate =
    (
      column: string,
      cellTemplate: Template<
        CommonAbstractRendererReadonlyContext<
          DispatchParsedType<any>,
          PredicateValue,
          CustomPresentationContext,
          ExtraContext
        >,
        CommonAbstractRendererState,
        CommonAbstractRendererForeignMutationsExpected<Flags>
      >,
    ) =>
    (rowId: string) =>
    (value: PredicateValue) =>
    (disabled: boolean) =>
    (flags: Flags | undefined) =>
      cellTemplate
        .mapContext<
          TableAbstractRendererReadonlyContext<
            CustomPresentationContext,
            ExtraContext
          > &
            TableAbstractRendererState
        >((_) => {
          const rowState = _.customFormState.rowStates.get(rowId);

          const cellState =
            rowState?.fieldStates.get(column) ??
            CellTemplates.get(column)!.GetDefaultState();

          const rowValue = _.value.data.get(rowId);

          if (rowValue == undefined) {
            console.error(
              `Row value is undefined for row ${rowId}\n
              ...When rendering table field ${column}\n
              ...${_.domNodeAncestorPath}`,
            );
            return undefined;
          }

          return {
            value,
            ...cellState,
            disabled: disabled || _.disabled,
            locked: _.locked,
            bindings: _.bindings.set("local", rowValue),
            extraContext: _.extraContext,
            type: TableEntityType.fields.get(column)!,
            customPresentationContext: _.customPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            typeAncestors: [_.type as DispatchParsedType<any>].concat(
              _.typeAncestors,
            ),
            domNodeAncestorPath:
              _.domNodeAncestorPath + `[table][cell][${rowId}][${column}]`,
            lookupTypeAncestorNames: _.lookupTypeAncestorNames,
          };
        })

        .mapState<TableAbstractRendererState>((updater) =>
          TableAbstractRendererState.Updaters.Core.customFormState.children.rowStates(
            MapRepo.Updaters.upsert(
              rowId,
              () => RecordAbstractRendererState.Default.fieldState(Map()),
              RecordAbstractRendererState.Updaters.Core.fieldStates(
                MapRepo.Updaters.upsert(
                  column,
                  () => CellTemplates.get(column)!.GetDefaultState(),
                  updater,
                ),
              ),
            ),
          ),
        )
        .mapForeignMutationsFromProps<
          TableAbstractRendererForeignMutationsExpected<Flags>
        >((props) => ({
          onChange: (
            nestedUpdater: Option<BasicUpdater<PredicateValue>>,
            nestedDelta: DispatchDelta<Flags>,
          ) => {
            props.setState(
              TableAbstractRendererState.Updaters.Core.customFormState.children
                .rowStates(
                  MapRepo.Updaters.upsert(
                    rowId,
                    () => RecordAbstractRendererState.Default.fieldState(Map()),
                    RecordAbstractRendererState.Updaters.Core.fieldStates(
                      MapRepo.Updaters.upsert(
                        column,
                        () => CellTemplates.get(column)!.GetDefaultState(),
                        (__) => ({
                          ...__,
                          commonFormState:
                            DispatchCommonFormState.Updaters.modifiedByUser(
                              replaceWith(true),
                            )(__.commonFormState),
                        }),
                      ),
                    ),
                  ),
                )
                .then(
                  TableAbstractRendererState.Updaters.Core.commonFormState.children.modifiedByUser(
                    replaceWith(true),
                  ),
                ),
            );

            const nestedRecordDelta: DispatchDelta<Flags> = {
              kind: "RecordField",
              field: [column, nestedDelta],
              recordType: TableEntityType,
              flags: undefined,
              sourceAncestorLookupTypeNames:
                nestedDelta.sourceAncestorLookupTypeNames,
            };

            const delta: DispatchDelta<Flags> = {
              kind: "TableValue",
              id: rowId,
              nestedDelta: nestedRecordDelta,
              flags,
              sourceAncestorLookupTypeNames:
                nestedDelta.sourceAncestorLookupTypeNames,
            };

            const updater =
              nestedUpdater.kind == "l"
                ? nestedUpdater
                : Option.Default.some(
                    ValueTable.Updaters.data(
                      MapRepo.Updaters.update(
                        rowId,
                        ValueRecord.Updaters.update(
                          column,
                          nestedUpdater.kind == "r" ? nestedUpdater.value : id,
                        ),
                      ),
                    ),
                  );

            props.foreignMutations.onChange(updater, delta);
          },
        }));

  const embedDetailsRenderer = DetailsRenderer
    ? (flags: Flags | undefined) =>
        DetailsRenderer.mapContext<
          TableAbstractRendererReadonlyContext<
            CustomPresentationContext,
            ExtraContext
          > &
            TableAbstractRendererState
        >((_) => {
          const { selectedDetailRow } = _.customFormState;

          if (selectedDetailRow == undefined) {
            console.error(
              `Selected detail row is undefined\n
              ...When rendering table field\n
              ...${_.domNodeAncestorPath}`,
            );
            return undefined;
          }

          if (
            !PredicateValue.Operations.IsString(selectedDetailRow) &&
            !PredicateValue.Operations.IsUnit(selectedDetailRow)
          ) {
            console.error(
              `Selected detail row is not a string or unit\n
              ...When rendering table field\n
              ...${_.domNodeAncestorPath}`,
            );
            return undefined;
          }

          const value = PredicateValue.Operations.IsUnit(selectedDetailRow)
            ? ValueUnit.Default()
            : _.value.data.get(selectedDetailRow);

          if (value == undefined) {
            console.error(
              `Value is undefined for selected detail row\n
              ...When rendering table field\n
              ...${_.domNodeAncestorPath}`,
            );
            return undefined;
          }

          const rowState = PredicateValue.Operations.IsString(selectedDetailRow)
            ? (_.customFormState.rowStates.get(selectedDetailRow) ??
              RecordAbstractRendererState.Default.fieldState(Map()))
            : RecordAbstractRendererState.Default.fieldState(Map());

          return {
            value,
            ...rowState,
            disabled: _.disabled,
            locked: _.locked,
            bindings: _.bindings.set("local", value),
            extraContext: _.extraContext,
            type: TableEntityType,
            customPresentationContext: _.customPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            typeAncestors: [_.type as DispatchParsedType<any>].concat(
              _.typeAncestors,
            ),
            domNodeAncestorPath: _.domNodeAncestorPath + "[table][details]",
            lookupTypeAncestorNames: _.lookupTypeAncestorNames,
          };
        })
          .mapStateFromProps<TableAbstractRendererState>(([props, updater]) => {
            const { selectedDetailRow } = props.context.customFormState;

            if (selectedDetailRow == undefined) {
              console.error(
                `Selected detail row is undefined\n
                ...When rendering table detail view \n
                ...${props.context.domNodeAncestorPath}`,
              );
              return id;
            }
            if (!PredicateValue.Operations.IsString(selectedDetailRow)) {
              return id;
            }
            return TableAbstractRendererState.Updaters.Core.customFormState.children.rowStates(
              MapRepo.Updaters.upsert(
                selectedDetailRow,
                () => RecordAbstractRendererState.Default.fieldState(Map()),
                updater,
              ),
            );
          })
          .mapForeignMutationsFromProps<
            TableAbstractRendererForeignMutationsExpected<Flags>
          >((props) => ({
            onChange: (
              nestedUpdater: Option<BasicUpdater<ValueRecord>>,
              nestedDelta: DispatchDelta<Flags>,
            ) => {
              const { selectedDetailRow } = props.context.customFormState;

              if (selectedDetailRow == undefined) {
                console.error(
                  `Selected detail row is undefined\n
                  ...When rendering table field\n
                  ...${props.context.domNodeAncestorPath}`,
                );
                return id;
              }

              if (
                !PredicateValue.Operations.IsString(selectedDetailRow) &&
                !PredicateValue.Operations.IsUnit(selectedDetailRow)
              ) {
                console.error(
                  `Selected detail row is not a string or unit\n
                    ...When rendering table field\n
                    ...${props.context.domNodeAncestorPath}`,
                );
                return id;
              }

              if (PredicateValue.Operations.IsString(selectedDetailRow)) {
                props.setState(
                  TableAbstractRendererState.Updaters.Core.commonFormState.children.modifiedByUser(
                    replaceWith(true),
                  ),
                );

                const delta: DispatchDelta<Flags> = {
                  kind: "TableValue",
                  id: selectedDetailRow,
                  nestedDelta: nestedDelta,
                  flags,
                  sourceAncestorLookupTypeNames:
                    nestedDelta.sourceAncestorLookupTypeNames,
                };

                const updater =
                  nestedUpdater.kind == "l"
                    ? nestedUpdater
                    : Option.Default.some(
                        ValueTable.Updaters.data(
                          MapRepo.Updaters.update(
                            selectedDetailRow,
                            nestedUpdater.value,
                          ),
                        ),
                      );

                props.foreignMutations.onChange(updater, delta);
              }
            },
          }))
    : undefined;

  const EmbeddedCellTemplates = CellTemplates.map((cellTemplate, column) =>
    embedCellTemplate(column, cellTemplate.template),
  );

  const ColumnLabels = CellTemplates.map((cellTemplate) => cellTemplate.label);

  const EmbeddedAllowedFilters = Filters.map((filter, columnName) => ({
    ...filter,
    filters: filter.filters.args as Array<FilterType<any>>,
    template: (index: number) =>
      filter.template
        .mapContext<
          CommonAbstractRendererReadonlyContext<
            DispatchParsedType<any>,
            PredicateValue,
            CustomPresentationContext,
            ExtraContext
          > &
            TableAbstractRendererState
        >((_) => ({
          value: _.value,
          locked: _.locked,
          disabled: _.disabled,
          bindings: _.bindings,
          extraContext: _.extraContext,
          type: filter.type,
          label: _.label,
          tooltip: _.tooltip,
          details: _.details,
          customPresentationContext: _.customPresentationContext,
          remoteEntityVersionIdentifier: "",
          domNodeAncestorPath: "",
          typeAncestors: [],
          lookupTypeAncestorNames: [],
          ...(_.customFormState.filterStates.get(columnName)?.get(index) ??
            filter.GetDefaultState()),
        }))
        .mapState<TableAbstractRendererState>((_) =>
          TableAbstractRendererState.Updaters.Core.customFormState.children.filterStates(
            MapRepo.Updaters.upsert(
              columnName,
              () => List([filter.GetDefaultState()]),
              ListRepo.Updaters.update(index, Updater(_)),
            ),
          ),
        ),
  }));

  return Template.Default<
    TableAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      TableAbstractRendererState,
    TableAbstractRendererState,
    TableAbstractRendererForeignMutationsExpected<Flags>,
    TableAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[table]";

    if (!PredicateValue.Operations.IsTable(props.context.value)) {
      console.error(
        `TableValue expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering table field\n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Table value expected but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    const updatedBindings = props.context.bindings.set(
      "local",
      props.context.value,
    );

    const visibleColumns = TableLayout.Operations.ComputeLayout(
      updatedBindings,
      Layout,
    );

    if (visibleColumns.kind == "errors") {
      console.error(visibleColumns.errors.map((error) => error).join("\n"));
      return (
        <ErrorRenderer
          message={`${domNodeId}: Error while computing visible columns, check console`}
        />
      );
    }
    // TODO we currently only calculated disabled status on a column basis, predicates will break if we
    // try to use their local binding (the local is the table).
    // Later we need to then calculate the disabled on a CELL level, by giving the calculations
    // the row local binding and calculating per row, not per column.
    const disabledColumnKeys = ValueOrErrors.Operations.All(
      List(
        CellTemplates.map(({ disabled }, fieldName) =>
          disabled == undefined
            ? ValueOrErrors.Default.return(null)
            : Expr.Operations.EvaluateAs("disabled predicate")(updatedBindings)(
                disabled,
              ).Then((value) =>
                ValueOrErrors.Default.return(
                  PredicateValue.Operations.IsBoolean(value) && value
                    ? fieldName
                    : null,
                ),
              ),
        ).valueSeq(),
      ),
    );

    // TODO -- set the top level state as error
    if (disabledColumnKeys.kind == "errors") {
      console.error(disabledColumnKeys.errors.map((error) => error).join("\n"));
      return (
        <ErrorRenderer
          message={`${domNodeId}: Error while computing disabled column keys, check console`}
        />
      );
    }

    const disabledColumnKeysSet = Set(
      disabledColumnKeys.value.filter((fieldName) => fieldName != null),
    );

    const hasMoreValues = props.context.value.hasMoreValues;

    const embeddedTableData =
      props.context.customFormState.loadingState != "loaded"
        ? OrderedMap<string, OrderedMap<string, any>>()
        : props.context.value.data.map((rowData, rowId) =>
            rowData.fields
              .filter((_, column) =>
                visibleColumns.value.columns.includes(column),
              )
              .map((_, column) => {
                return EmbeddedCellTemplates.get(column)!(rowId)(
                  rowData.fields.get(column)!,
                )(disabledColumnKeysSet.has(column));
              }),
          );

    const validVisibleColumns = visibleColumns.value.columns.filter((_) =>
      TableEntityType.fields.keySeq().toArray().includes(_),
    );

    if (props.context.customFormState.isFilteringInitialized == false) {
      return <></>;
    }

    // Detail row may have been deleted from outside the table
    const isSelectedDetailRowValid =
      props.context.customFormState.selectedDetailRow != undefined &&
      !PredicateValue.Operations.IsUnit(
        props.context.customFormState.selectedDetailRow,
      ) &&
      props.context.value.data.size > 0 &&
      props.context.value.data.has(
        props.context.customFormState.selectedDetailRow,
      );

    return (
      <>
        <IdProvider domNodeId={domNodeId}>
          <props.view
            {...props}
            context={{
              ...props.context,
              customFormState: {
                ...props.context.customFormState,
                selectedDetailRow: isSelectedDetailRowValid
                  ? props.context.customFormState.selectedDetailRow
                  : undefined,
              },
              domNodeId,
              tableHeaders: validVisibleColumns,
              columnLabels: ColumnLabels,
              hasMoreValues: !!hasMoreValues,
              tableEntityType: TableEntityType,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              loadMore: () => {
                if (props.context.customFormState.isFilteringInitialized) {
                  props.setState(
                    TableAbstractRendererState.Updaters.Template.loadMore(),
                  );
                }
              },
              selectDetailView: (rowId: string) => {
                props.setState(
                  TableAbstractRendererState.Updaters.Core.customFormState.children.selectedDetailRow(
                    replaceWith<TableAbstractRendererSelectedDetailRow>(rowId),
                  ),
                );
              },
              clearDetailView: () =>
                props.setState(
                  TableAbstractRendererState.Updaters.Core.customFormState.children.selectedDetailRow(
                    replaceWith<TableAbstractRendererSelectedDetailRow>(
                      undefined,
                    ),
                  ),
                ),
              selectRow: (rowId: string) =>
                props.setState(
                  TableAbstractRendererState.Updaters.Core.customFormState.children.selectedRows(
                    (_) => (_.has(rowId) ? _.remove(rowId) : _.add(rowId)),
                  ),
                ),
              selectAllRows: () =>
                props.setState(
                  TableAbstractRendererState.Updaters.Core.customFormState.children.selectedRows(
                    replaceWith(Set(embeddedTableData.keySeq())),
                  ),
                ),
              clearRows: () =>
                props.setState(
                  TableAbstractRendererState.Updaters.Core.customFormState.children.selectedRows(
                    replaceWith(Set()),
                  ),
                ),
              add: !props.context.apiMethods.includes("add")
                ? undefined
                : (flags: Flags | undefined) => {
                    const delta: DispatchDelta<Flags> = {
                      kind: "TableAddEmpty",
                      flags,
                      sourceAncestorLookupTypeNames:
                        props.context.lookupTypeAncestorNames,
                    };
                    props.foreignMutations.onChange(
                      Option.Default.none(),
                      delta,
                    );
                    props.setState(
                      TableAbstractRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ),
                    );
                  },
              remove: !props.context.apiMethods.includes("remove")
                ? undefined
                : (k: string, flags: Flags | undefined) => {
                    const delta: DispatchDelta<Flags> = {
                      kind: "TableRemove",
                      id: k,
                      flags,
                      sourceAncestorLookupTypeNames:
                        props.context.lookupTypeAncestorNames,
                    };
                    props.foreignMutations.onChange(
                      Option.Default.none(),
                      delta,
                    );
                    props.setState(
                      TableAbstractRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ),
                    );
                  },
              moveTo: !props.context.apiMethods.includes("move")
                ? undefined
                : (k: string, to: string, flags: Flags | undefined) => {
                    const delta: DispatchDelta<Flags> = {
                      kind: "TableMoveTo",
                      id: k,
                      to,
                      flags,
                      sourceAncestorLookupTypeNames:
                        props.context.lookupTypeAncestorNames,
                    };
                    props.foreignMutations.onChange(
                      Option.Default.none(),
                      delta,
                    );
                    props.setState(
                      TableAbstractRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ),
                    );
                  },
              duplicate: !props.context.apiMethods.includes("duplicate")
                ? undefined
                : (k: string, flags: Flags | undefined) => {
                    const delta: DispatchDelta<Flags> = {
                      kind: "TableDuplicate",
                      id: k,
                      flags,
                      sourceAncestorLookupTypeNames:
                        props.context.lookupTypeAncestorNames,
                    };
                    props.foreignMutations.onChange(
                      Option.Default.none(),
                      delta,
                    );
                    props.setState(
                      TableAbstractRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ),
                    );
                  },
              updateFilters: (filters: Map<string, List<ValueFilter>>) => {
                props.setState(
                  TableAbstractRendererState.Updaters.Template.updateFilters(
                    filters,
                    Filters.map(({ filters }) => filters),
                    parseToApiByType,
                  ),
                );
              },
              addSorting: (
                columnName: string,
                direction: "Ascending" | "Descending" | undefined,
              ) => {
                props.setState(
                  TableAbstractRendererState.Updaters.Template.addSorting(
                    columnName,
                    direction,
                    Filters.map(({ filters }) => filters),
                    parseToApiByType,
                  ),
                );
              },
              removeSorting: (columnName: string) => {
                props.setState(
                  TableAbstractRendererState.Updaters.Template.removeSorting(
                    columnName,
                    Filters.map(({ filters }) => filters),
                    parseToApiByType,
                  ),
                );
              },
              reloadFrom0: () =>
                props.setState(
                  TableAbstractRendererState.Updaters.Template.reloadFrom0(),
                ),
            }}
            DetailsRenderer={embedDetailsRenderer}
            TableData={embeddedTableData}
            AllowedFilters={EmbeddedAllowedFilters}
            AllowedSorting={props.context.sorting}
            HighlightedFilters={props.context.highlightedFilters}
            isFilteringSortAndLoadingEnabled={
              props.context.customFormState.isFilteringInitialized
            }
          />
        </IdProvider>
      </>
    );
  }).any([
    InstantiatedInfiniteLoaderRunner.mapContextFromProps((props) => ({
      ...props.context,
      onChange: props.foreignMutations.onChange,
    })),
    InstantiatedInitialiseFiltersAndSortingRunner,
    InstantiatedInitialiseTableRunner.mapContextFromProps((props) => ({
      ...props.context,
      onChange: props.foreignMutations.onChange,
    })),
  ]);
};
