import { List, Map, OrderedMap, Set } from "immutable";
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
  StringSerializedType,
  RecordType,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import { ValueInfiniteStreamState } from "../../../../../../../value-infinite-data-stream/state";
import { TableReinitialiseRunner, TableRunner } from "./coroutines/runner";

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
  SerializedType: StringSerializedType,
  TableEntityType: RecordType<any>,
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
              ...When rendering table field\n
              ...${SerializedType}`,
            );
            return undefined;
          }

          return {
            value,
            ...cellState,
            disabled: disabled || _.disabled,
            bindings: _.bindings.set("local", rowValue),
            extraContext: _.extraContext,
            type: TableEntityType.fields.get(column)!,
            customPresentationContext: _.customPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            serializedTypeHierarchy: [SerializedType].concat(
              _.serializedTypeHierarchy,
            ),
            domNodeAncestorPath:
              _.domNodeAncestorPath + `[table][cell][${rowId}][${column}]`,
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
            };

            const delta: DispatchDelta<Flags> = {
              kind: "TableValue",
              id: rowId,
              nestedDelta: nestedRecordDelta,
              flags,
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
          if (_.customFormState.selectedDetailRow == undefined) {
            console.error(
              `Selected detail row is undefined\n
              ...When rendering table field\n
              ...${SerializedType}`,
            );
            return undefined;
          }

          const value = _.customFormState.stream.loadedElements
            .get(_.customFormState.selectedDetailRow[0])
            ?.data.get(_.customFormState.selectedDetailRow[1]);

          if (value == undefined) {
            console.error(
              `Value is undefined for selected detail row\n
              ...When rendering table field\n
              ...${SerializedType}`,
            );
            return undefined;
          }

          const rowState =
            _.customFormState.rowStates.get(
              _.customFormState.selectedDetailRow[1],
            ) ?? RecordAbstractRendererState.Default.fieldState(Map());

          return {
            value,
            ...rowState,
            disabled: _.disabled,
            bindings: _.bindings.set("local", value),
            extraContext: _.extraContext,
            type: TableEntityType,
            customPresentationContext: _.customPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            serializedTypeHierarchy: [SerializedType].concat(
              _.serializedTypeHierarchy,
            ),
            domNodeAncestorPath: _.domNodeAncestorPath + "[table][details]",
          };
        })
          .mapStateFromProps<TableAbstractRendererState>(([props, updater]) => {
            if (props.context.customFormState.selectedDetailRow == undefined) {
              console.error(
                `Selected detail row is undefined\n
                ...When rendering table detail view \n
                ...${SerializedType}`,
              );
              return id;
            }

            return TableAbstractRendererState.Updaters.Core.customFormState.children.rowStates(
              MapRepo.Updaters.upsert(
                props.context.customFormState.selectedDetailRow[1],
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
              if (
                props.context.customFormState.selectedDetailRow == undefined
              ) {
                console.error(
                  `Selected detail row is undefined\n
                  ...When rendering table field\n
                  ...${SerializedType}`,
                );
                return id;
              }
              props.setState(
                TableAbstractRendererState.Updaters.Core.commonFormState.children
                  .modifiedByUser(replaceWith(true))
                  .then(
                    TableAbstractRendererState.Updaters.Core.customFormState.children.stream(
                      ValueInfiniteStreamState.Updaters.Template.updateChunkValue(
                        props.context.customFormState.selectedDetailRow[0],
                        props.context.customFormState.selectedDetailRow[1],
                      )(_.kind == "r" ? _.value : id),
                    ),
                  ),
              );

              // TODO, different delta for details
              const delta: DispatchDelta<Flags> = {
                kind: "TableValue",
                id: props.context.customFormState.selectedDetailRow[1],
                nestedDelta: nestedDelta,
                flags,
              };

              props.foreignMutations.onChange(Option.Default.none(), delta);
            },
          }))
    : undefined;

  const EmbeddedCellTemplates = CellTemplates.map((cellTemplate, column) =>
    embedCellTemplate(column, cellTemplate.template),
  );

  const ColumnLabels = CellTemplates.map((cellTemplate) => cellTemplate.label);

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
    const completeSerializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

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

    if (props.context.customFormState.initializationStatus !== "initialized") {
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
          message={`${SerializedType}: Error while computing visible columns, check console`}
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
          message={`${SerializedType}: Error while computing disabled column keys, check console`}
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
                  console.error(
                    `Cannot find column ${column} in fields ${rowData.fields.keySeq().toArray()}, this is likely due to a mismatch between the data we are trying to parse and the specs.`,
                  );
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
              completeSerializedTypeHierarchy,
              tableEntityType: TableEntityType,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              loadMore: () =>
                props.setState(
                  TableAbstractRendererState.Updaters.Template.loadMore(),
                ),
              selectDetailView: (rowId: string) => {
                const chunkIndex =
                  ValueInfiniteStreamState.Operations.getChunkIndexForValue(
                    props.context.customFormState.stream,
                    rowId,
                  );
                props.setState(
                  TableAbstractRendererState.Updaters.Core.customFormState.children.selectedDetailRow(
                    chunkIndex.kind == "value"
                      ? replaceWith<[number, string] | undefined>([
                          chunkIndex.value,
                          rowId,
                        ])
                      : id,
                  ),
                );
              },
              clearDetailView: () =>
                props.setState(
                  TableAbstractRendererState.Updaters.Core.customFormState.children.selectedDetailRow(
                    replaceWith<[number, string] | undefined>(undefined),
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
                      ).then(
                        TableAbstractRendererState.Updaters.Template.shouldReinitialize(
                          true,
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
                      ).then(
                        TableAbstractRendererState.Updaters.Template.shouldReinitialize(
                          true,
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
                      ).then(
                        TableAbstractRendererState.Updaters.Template.shouldReinitialize(
                          true,
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
                      ).then(
                        TableAbstractRendererState.Updaters.Template.shouldReinitialize(
                          true,
                        ),
                      ),
                    );
                  },
            }}
            DetailsRenderer={embedDetailsRenderer}
            TableData={embeddedTableData}
          />
        </IdProvider>
      </>
    );
  }).any([
    InstantiatedTableRunner,
    InstantiatedTableReinitialiseRunner,
    InstantiatedEmbeddedValueInfiniteStreamTemplate,
  ]);
};
