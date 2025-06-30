import { List, Map, Set } from "immutable";
import {
  BasicUpdater,
  id,
  PredicateValue,
  AbstractTableRendererState,
  TableLayout,
  ValueInfiniteStreamTemplate,
  Expr,
  PredicateVisibleColumns,
  DispatchDelta,
  ValueOrErrors,
  AbstractTableRendererReadonlyContext,
  replaceWith,
  ValueRecord,
  DispatchCommonFormState,
  FormLabel,
  Bindings,
  RecordAbstractRendererState,
  DispatchOnChange,
  IdWrapperProps,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  TableType,
  ValueTable,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import { ValueInfiniteStreamState } from "../../../../../../../value-infinite-data-stream/state";
import { TableReinitialiseRunner, TableRunner } from "./coroutines/runner";

const EmbeddedValueInfiniteStreamTemplate =
  ValueInfiniteStreamTemplate.mapContext<
    AbstractTableRendererReadonlyContext & AbstractTableRendererState
  >((_) => _.customFormState.stream)
    .mapState<AbstractTableRendererState>(
      AbstractTableRendererState.Updaters.Core.customFormState.children.stream,
    )
    .mapForeignMutationsFromProps<any>((props) => ({
      ...props.foreignMutations,
    }));

export const TableAbstractRenderer = <
  Context extends FormLabel & {
    bindings: Bindings;
    identifiers: { withLauncher: string; withoutLauncher: string };
  },
  ForeignMutationsExpected,
>(
  CellTemplates: Map<
    string,
    {
      template: Template<any, any, any, any>;
      label?: string;
      disabled?: Expr;
      GetDefaultValue: () => any;
      GetDefaultState: () => any;
    }
  >,
  DetailsRenderer: Template<any, any, any, any> | undefined,
  Layout: PredicateVisibleColumns,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
): Template<any, any, any, any> => {
  const embedCellTemplate =
    (column: string, cellTemplate: Template<any, any, any, any>) =>
    (chunkIndex: number) =>
    (rowId: string) =>
    (value: PredicateValue) =>
    (disabled: boolean) =>
      cellTemplate
        // TODO, more helpful typing
        .mapContext<any>((_: any) => {
          const rowState = _.customFormState.stream.chunkStates
            .get(chunkIndex)
            ?.get(rowId);

          const cellState =
            rowState?.get(column) ??
            CellTemplates.get(column)!.GetDefaultState();

          const rowValue = _.customFormState.stream.loadedElements
            .get(chunkIndex)
            ?.data.get(rowId);

          return {
            ..._,
            value,
            ...cellState,
            disabled: disabled || _.disabled,
            bindings: _.bindings.set("local", rowValue),
            extraContext: _.extraContext,
            identifiers: {
              withLauncher: _.identifiers.withLauncher.concat(
                `[${rowId}][${column}]`,
              ),
              withoutLauncher: _.identifiers.withoutLauncher.concat(
                `[${rowId}][${column}]`,
              ),
            },
          };
        })
        .mapState<AbstractTableRendererState>((_) =>
          AbstractTableRendererState.Updaters.Core.customFormState.children.stream(
            ValueInfiniteStreamState.Updaters.Template.updateChunkStateValueItem(
              chunkIndex,
              rowId,
              column,
              CellTemplates.get(column)!.GetDefaultState,
            )(_),
          ),
        )
        .mapForeignMutationsFromProps<{
          onChange: DispatchOnChange<PredicateValue>;
        }>((props) => ({
          onChange: (
            _: BasicUpdater<PredicateValue>,
            nestedDelta: DispatchDelta,
          ) => {
            props.setState(
              AbstractTableRendererState.Updaters.Core.customFormState.children
                .stream(
                  ValueInfiniteStreamState.Updaters.Template.updateChunkStateValueItem(
                    chunkIndex,
                    rowId,
                    column,
                    CellTemplates.get(column)!.GetDefaultState,
                  )((__) => ({
                    ...__,
                    commonFormState:
                      DispatchCommonFormState.Updaters.modifiedByUser(
                        replaceWith(true),
                      )(__.commonFormState),
                  })),
                )
                .then(
                  AbstractTableRendererState.Updaters.Core.commonFormState.children.modifiedByUser(
                    replaceWith(true),
                  ),
                )
                .then(
                  AbstractTableRendererState.Updaters.Core.customFormState.children.stream(
                    ValueInfiniteStreamState.Updaters.Template.updateChunkValueItem(
                      chunkIndex,
                      rowId,
                      column,
                    )(_),
                  ),
                ),
            );

            const nestedRecordDelta: DispatchDelta = {
              kind: "RecordField",
              field: [column, nestedDelta],
              recordType: props.context.type.args[0],
              isWholeEntityMutation: false,
            };

            const delta: DispatchDelta = {
              kind: "TableValue",
              id: rowId,
              nestedDelta: nestedRecordDelta,
              isWholeEntityMutation: false,
            };

            props.foreignMutations.onChange(id, delta);
          },
        }));

  const embedDetailsRenderer: Template<any, any, any, any> | undefined =
    DetailsRenderer?.mapContext<any>((_: any) => {
      const value = _.customFormState.stream.loadedElements
        .get(_.customFormState.selectedDetailRow[0])
        ?.data.get(_.customFormState.selectedDetailRow[1]);

      const rowState = _.customFormState.stream.chunkStates
        .get(_.customFormState.selectedDetailRow[0])
        ?.get(_.customFormState.selectedDetailRow[1]);

      const recordRowState = rowState
        ? RecordAbstractRendererState.Default.fieldState(rowState)
        : RecordAbstractRendererState.Default.fieldState(Map());

      return {
        value,
        ...recordRowState,
        disabled: _.disabled,
        bindings: _.bindings.set("local", value),
        extraContext: _.extraContext,
        identifiers: {
          withLauncher: _.identifiers.withLauncher.concat(
            `[${_.customFormState.selectedDetailRow[0]}][${_.customFormState.selectedDetailRow[1]}]`,
          ),
          withoutLauncher: _.identifiers.withoutLauncher.concat(
            `[${_.customFormState.selectedDetailRow[0]}][${_.customFormState.selectedDetailRow[1]}]`,
          ),
        },
      };
    })
      .mapStateFromProps<AbstractTableRendererState>(([props, updater]) => {
        return AbstractTableRendererState.Updaters.Core.customFormState.children.stream(
          ValueInfiniteStreamState.Updaters.Template.updateChunkStateValue(
            props.context.customFormState.selectedDetailRow[0],
            props.context.customFormState.selectedDetailRow[1],
          )((__) => {
            const temp = RecordAbstractRendererState.Default.fieldState(__);
            const updated = updater(temp);
            const newState = updated.fieldStates;
            return newState;
          }),
        );
      })
      .mapForeignMutationsFromProps<{
        onChange: DispatchOnChange<PredicateValue>;
      }>((props) => ({
        onChange: (
          _: BasicUpdater<ValueRecord>,
          nestedDelta: DispatchDelta,
        ) => {
          props.setState(
            AbstractTableRendererState.Updaters.Core.commonFormState.children
              .modifiedByUser(replaceWith(true))
              .then(
                AbstractTableRendererState.Updaters.Core.customFormState.children.stream(
                  ValueInfiniteStreamState.Updaters.Template.updateChunkValue(
                    props.context.customFormState.selectedDetailRow[0],
                    props.context.customFormState.selectedDetailRow[1],
                  )(_),
                ),
              ),
          );

          // TODO, different delta for details
          const delta: DispatchDelta = {
            kind: "TableValue",
            id: props.context.customFormState.selectedDetailRow,
            nestedDelta: nestedDelta,
            isWholeEntityMutation: false,
          };

          props.foreignMutations.onChange(id, delta);
        },
      }));

  const EmbeddedCellTemplates = CellTemplates.map((cellTemplate, column) =>
    embedCellTemplate(column, cellTemplate.template),
  );

  const ColumnLabels = CellTemplates.map((cellTemplate) => cellTemplate.label);

  return Template.Default<
    AbstractTableRendererReadonlyContext & AbstractTableRendererState,
    AbstractTableRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueTable>;
    },
    any
  >((props) => {
    if (!PredicateValue.Operations.IsTable(props.context.value)) {
      console.error(
        `TableValue expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering table field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Table value expected for table but got ${JSON.stringify(
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
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Error while computing visible columns, check console`}
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
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Error while computing disabled column keys, check console`}
        />
      );
    }

    const disabledColumnKeysSet = Set(
      disabledColumnKeys.value.filter((fieldName) => fieldName != null),
    );

    const tableData =
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

    // don't send to the renderer columns which do not exist in the embedded type
    const validColumns: ValueOrErrors<string[], string> =
      props.context.type.kind !== "table"
        ? ValueOrErrors.Default.throwOne(
            `"TableType" expected but got ${props.context.type.kind} when parsing table type valid columns`,
          )
        : props.context.type.args[0].kind !== "record"
          ? ValueOrErrors.Default.throwOne(
              `"RecordType" expected but got ${props.context.type.args[0].kind} when parsing table type valid columns`,
            )
          : ValueOrErrors.Default.return(
              props.context.type.args[0].fields.keySeq().toArray(),
            );

    if (validColumns.kind == "errors") {
      console.error(validColumns.errors.toArray().join("\n"));

      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Error while parsing type`}
        />
      );
    }

    const validVisibleColumns = visibleColumns.value.columns.filter(
      (_) => !validColumns.value.length || validColumns.value.includes(_),
    );

    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.identifiers.withoutLauncher,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              loadMore: () =>
                props.setState(
                  AbstractTableRendererState.Updaters.Template.loadMore(),
                ),
              selectDetailView: (rowId: string) => {
                const chunkIndex =
                  ValueInfiniteStreamState.Operations.getChunkIndexForValue(
                    props.context.customFormState.stream,
                    rowId,
                  );
                props.setState(
                  AbstractTableRendererState.Updaters.Core.customFormState.children.selectedDetailRow(
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
                  AbstractTableRendererState.Updaters.Core.customFormState.children.selectedDetailRow(
                    replaceWith<[number, string] | undefined>(undefined),
                  ),
                ),
              selectRow: (rowId: string) =>
                props.setState(
                  AbstractTableRendererState.Updaters.Core.customFormState.children.selectedRows(
                    (_) => (_.has(rowId) ? _.remove(rowId) : _.add(rowId)),
                  ),
                ),
              selectAllRows: () =>
                props.setState(
                  AbstractTableRendererState.Updaters.Core.customFormState.children.selectedRows(
                    replaceWith(Set(tableData.keySeq())),
                  ),
                ),
              clearRows: () =>
                props.setState(
                  AbstractTableRendererState.Updaters.Core.customFormState.children.selectedRows(
                    replaceWith(Set()),
                  ),
                ),
              add: !props.context.apiMethods.includes("add")
                ? undefined
                : () => {
                    const delta: DispatchDelta = {
                      kind: "TableAddEmpty",
                      isWholeEntityMutation: true,
                    };
                    props.foreignMutations.onChange(id, delta);
                    props.setState(
                      AbstractTableRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ).then(
                        AbstractTableRendererState.Updaters.Template.shouldReinitialize(
                          true,
                        ),
                      ),
                    );
                  },
              remove: !props.context.apiMethods.includes("remove")
                ? undefined
                : (k: string) => {
                    const delta: DispatchDelta = {
                      kind: "TableRemove",
                      id: k,
                      isWholeEntityMutation: true,
                    };
                    props.foreignMutations.onChange(id, delta);
                    props.setState(
                      AbstractTableRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ).then(
                        AbstractTableRendererState.Updaters.Template.shouldReinitialize(
                          true,
                        ),
                      ),
                    );
                  },
              moveTo: !props.context.apiMethods.includes("move")
                ? undefined
                : (k: string, to: string) => {
                    const delta: DispatchDelta = {
                      kind: "TableMoveTo",
                      id: k,
                      to,
                      isWholeEntityMutation: true,
                    };
                    props.foreignMutations.onChange(id, delta);
                    props.setState(
                      AbstractTableRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ).then(
                        AbstractTableRendererState.Updaters.Template.shouldReinitialize(
                          true,
                        ),
                      ),
                    );
                  },
              duplicate: !props.context.apiMethods.includes("duplicate")
                ? undefined
                : (k: string) => {
                    const delta: DispatchDelta = {
                      kind: "TableDuplicate",
                      id: k,
                      isWholeEntityMutation: true,
                    };
                    props.foreignMutations.onChange(id, delta);
                    props.setState(
                      AbstractTableRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ).then(
                        AbstractTableRendererState.Updaters.Template.shouldReinitialize(
                          true,
                        ),
                      ),
                    );
                  },
            }}
            TableHeaders={validVisibleColumns}
            ColumnLabels={ColumnLabels}
            EmbeddedTableData={tableData}
            DetailsRenderer={embedDetailsRenderer}
          />
        </IdProvider>
      </>
    );
  }).any([
    TableRunner,
    TableReinitialiseRunner,
    EmbeddedValueInfiniteStreamTemplate,
  ]);
};
