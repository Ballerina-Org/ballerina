import { List, Map, Set } from "immutable";
import {
  BasicUpdater,
  id,
  PredicateValue,
  TableAbstractRendererState,
  TableLayout,
  ValueInfiniteStreamTemplate,
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
  unit,
  ValueUnit,
  TableAbstractRendererSelectedDetailRow,
  TableApiFiltering,
  ValueFilter,
  CommonAbstractRendererViewOnlyReadonlyContext,
  FilterType,
  ListRepo,
  Updater,
  SumNType,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import { ValueInfiniteStreamState } from "../../../../../../../value-infinite-data-stream/state";
import {
  TableInitialiseFiltersAndSortingRunner,
  TableReinitialiseRunner,
  TableRunner,
} from "./coroutines/runner";

const EmbeddedValueInfiniteStreamTemplate = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>() =>
  ValueInfiniteStreamTemplate.mapContext<
    TableAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      TableAbstractRendererState
  >((_) => _.customFormState.stream)
    .mapState<TableAbstractRendererState>(
      TableAbstractRendererState.Updaters.Core.customFormState.children.stream,
    )
    .mapForeignMutationsFromProps<
      TableAbstractRendererForeignMutationsExpected<Flags>
    >((props) => ({
      ...props.foreignMutations,
    }));

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
  const InstantiatedTableRunner = TableRunner<
    CustomPresentationContext,
    ExtraContext
  >();
  const InstantiatedTableReinitialiseRunner = TableReinitialiseRunner<
    CustomPresentationContext,
    ExtraContext
  >();
  const InstantiatedEmbeddedParseFromApiByTypeTemplate =
    TableInitialiseFiltersAndSortingRunner<
      CustomPresentationContext,
      ExtraContext
    >(Filters.map(({ filters }) => filters));
  const InstantiatedEmbeddedValueInfiniteStreamTemplate =
    EmbeddedValueInfiniteStreamTemplate<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >();

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
    (chunkIndex: number) =>
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

          const rowValue = _.customFormState.stream.loadedElements
            .get(chunkIndex)
            ?.data.get(rowId);

          if (rowValue == undefined) {
            console.error(
              `Row value is undefined for row ${rowId} in chunk ${chunkIndex}\n
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
                )
                .then(
                  TableAbstractRendererState.Updaters.Core.customFormState.children.stream(
                    ValueInfiniteStreamState.Updaters.Template.updateChunkValueItem(
                      chunkIndex,
                      rowId,
                      column,
                    )(nestedUpdater.kind == "r" ? nestedUpdater.value : id),
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
            !PredicateValue.Operations.IsTuple(selectedDetailRow) &&
            !PredicateValue.Operations.IsUnit(selectedDetailRow)
          ) {
            console.error(
              `Selected detail row is not a tuple or unit\n
              ...When rendering table field\n
              ...${_.domNodeAncestorPath}`,
            );
            return undefined;
          }

          const chunkIndex = PredicateValue.Operations.IsTuple(
            selectedDetailRow,
          )
            ? Number(selectedDetailRow.values.get(0))
            : undefined;
          const chunkValueKey = PredicateValue.Operations.IsTuple(
            selectedDetailRow,
          )
            ? selectedDetailRow.values.get(1)?.toString()
            : undefined;

          const value = PredicateValue.Operations.IsUnit(selectedDetailRow)
            ? ValueUnit.Default()
            : chunkIndex !== undefined && chunkValueKey !== undefined
              ? _.customFormState.stream.loadedElements
                  .get(chunkIndex)
                  ?.data.get(chunkValueKey)
              : undefined;

          if (value == undefined) {
            console.error(
              `Value is undefined for selected detail row\n
              ...When rendering table field\n
              ...${_.domNodeAncestorPath}`,
            );
            return undefined;
          }

          const rowState = chunkValueKey
            ? _.customFormState.rowStates.get(chunkValueKey)
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

            if (!PredicateValue.Operations.IsTuple(selectedDetailRow)) {
              return id;
            }

            const chunkValueKey = selectedDetailRow.values.get(1);

            if (chunkValueKey == undefined) {
              console.error(
                `Chunk value key is undefined for selected detail row\n
                ...When rendering table detail view \n
                ...${props.context.domNodeAncestorPath}`,
              );
              return id;
            }

            return TableAbstractRendererState.Updaters.Core.customFormState.children.rowStates(
              MapRepo.Updaters.upsert(
                chunkValueKey.toString(),
                () => RecordAbstractRendererState.Default.fieldState(Map()),
                updater,
              ),
            );
          })
          .mapForeignMutationsFromProps<
            TableAbstractRendererForeignMutationsExpected<Flags>
          >((props) => ({
            onChange: (
              _: Option<BasicUpdater<ValueRecord>>,
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
                !PredicateValue.Operations.IsTuple(selectedDetailRow) &&
                !PredicateValue.Operations.IsUnit(selectedDetailRow)
              ) {
                console.error(
                  `Selected detail row is not a tuple or unit\n
                  ...When rendering table field\n
                  ...${props.context.domNodeAncestorPath}`,
                );
                return id;
              }

              if (PredicateValue.Operations.IsTuple(selectedDetailRow)) {
                const chunkIndex = Number(selectedDetailRow.values.get(0));
                const chunkValueKey = selectedDetailRow.values.get(1);

                if (!chunkValueKey) {
                  console.error(
                    `Chunk value key is undefined for selected detail row\n
                    ...When rendering table field\n
                    ...${props.context.domNodeAncestorPath}`,
                  );
                  return id;
                }

                props.setState(
                  TableAbstractRendererState.Updaters.Core.commonFormState.children
                    .modifiedByUser(replaceWith(true))
                    .then(
                      TableAbstractRendererState.Updaters.Core.customFormState.children.stream(
                        ValueInfiniteStreamState.Updaters.Template.updateChunkValue(
                          chunkIndex,
                          chunkValueKey.toString(),
                        )(_.kind == "r" ? _.value : id),
                      ),
                    ),
                );

                // TODO, different delta for details
                const delta: DispatchDelta<Flags> = {
                  kind: "TableValue",
                  id: chunkValueKey.toString(),
                  nestedDelta: nestedDelta,
                  flags,
                  sourceAncestorLookupTypeNames:
                    nestedDelta.sourceAncestorLookupTypeNames,
                };

                props.foreignMutations.onChange(Option.Default.none(), delta);
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
          locked: false,
          disabled: false,
          bindings: _.bindings,
          extraContext: _.extraContext,
          type: filter.type,
          label: _.label,
          tooltip: undefined,
          details: undefined,
          customPresentationContext: undefined,
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

    if (
      props.context.customFormState.initializationStatus !== "initialized" &&
      props.context.customFormState.initializationStatus !== "reinitializing"
    ) {
      return <></>;
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

    const hasMoreValues =
      props.context.customFormState.stream.loadedElements.last()?.hasMoreValues;

    const embeddedTableData =
      props.context.customFormState.stream.loadedElements.flatMap(
        (chunk, chunkIndex) =>
          chunk.data.map((rowData, rowId) =>
            rowData.fields
              .filter((_, column) =>
                visibleColumns.value.columns.includes(column),
              )
              .flatMap((_, column) => {
                const EmbeddedCell = EmbeddedCellTemplates.get(column);
                if (EmbeddedCell == undefined) {
                  return [];
                }
                return [
                  [
                    column,
                    EmbeddedCell(chunkIndex)(rowId)(
                      rowData.fields.get(column)!,
                    )(disabledColumnKeysSet.has(column)),
                  ],
                ];
              }),
          ),
      );

    const validVisibleColumns = visibleColumns.value.columns.filter((_) =>
      TableEntityType.fields.keySeq().toArray().includes(_),
    );

    return (
      <>
        <IdProvider domNodeId={domNodeId}>
          <props.view
            {...props}
            context={{
              ...props.context,
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
                const chunkIndex =
                  ValueInfiniteStreamState.Operations.getChunkIndexForValue(
                    props.context.customFormState.stream,
                    rowId,
                  );
                props.setState(
                  TableAbstractRendererState.Updaters.Core.customFormState.children.selectedDetailRow(
                    chunkIndex.kind == "value"
                      ? replaceWith<TableAbstractRendererSelectedDetailRow>(
                          PredicateValue.Default.tuple(
                            List([chunkIndex.value, rowId]),
                          ),
                        )
                      : id,
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
              reinitialize: () =>
                props.setState(
                  TableAbstractRendererState.Updaters.Template.shouldReinitialize(
                    true,
                  ),
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
    InstantiatedTableRunner,
    InstantiatedTableReinitialiseRunner,
    InstantiatedEmbeddedValueInfiniteStreamTemplate,
    InstantiatedEmbeddedParseFromApiByTypeTemplate,
  ]);
};
