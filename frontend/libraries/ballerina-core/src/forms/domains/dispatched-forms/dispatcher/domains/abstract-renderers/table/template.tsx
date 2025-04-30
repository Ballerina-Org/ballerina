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
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import { ValueInfiniteStreamState } from "../../../../../../../value-infinite-data-stream/state";
import { TableRunner } from "./coroutines/runner";
import { DispatchOnChange } from "../../../state";

const EmbeddedValueInfiniteStreamTemplate =
  ValueInfiniteStreamTemplate.mapContext<
    AbstractTableRendererReadonlyContext & AbstractTableRendererState
  >((_) => _.customFormState.stream).mapState<AbstractTableRendererState>(
    AbstractTableRendererState.Updaters.Core.customFormState.children.stream,
  );

export const TableAbstractRenderer = (
  CellTemplates: Map<
    string,
    {
      template: Template<any, any, any, any>;
      disabled?: Expr;
      GetDefaultValue: () => any;
      GetDefaultState: () => any;
    }
  >,
  Layout: PredicateVisibleColumns,
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
          // can be passed in
          const rowState = (
            _.customFormState.stream.chunkStates.get(chunkIndex)
              ?.state as Record<string, any>
          )?.[rowId];

          // can be passed in
          const cellState =
            rowState?.fields?.get(column) ??
            CellTemplates.get(column)!.GetDefaultState();

          return {
            value,
            commonFormState: _.commonFormState,
            customFormState: cellState.customFormState,
            disabled,
            bindings: _.bindings,
            extraContext: _.extraContext,
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
                    commonFormState: {
                      ...__.commonFormState,
                      modifiedByUser: true,
                    },
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

            // TODO, check the nesting
            const delta: DispatchDelta = {
              kind: "TableValue",
              id: rowId,
              nestedDelta: nestedDelta,
              tableType: props.context.type,
            };

            props.foreignMutations.onChange(id, delta);
          },
        }));

  const EmbeddedCellTemplates = CellTemplates.map((cellTemplate, column) =>
    embedCellTemplate(column, cellTemplate.template),
  );

  return Template.Default<
    AbstractTableRendererReadonlyContext & AbstractTableRendererState,
    AbstractTableRendererState,
    any,
    any
  >((props) => {
    const updatedBindings = props.context.bindings.set(
      "local",
      props.context.value,
    );

    const visibleColumns = TableLayout.Operations.ComputeLayout(
      updatedBindings,
      Layout,
    );

    // TODO -- set error template up top
    if (visibleColumns.kind == "errors") {
      console.error(visibleColumns.errors.map((error) => error).join("\n"));
      return <></>;
    }

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
      return <></>;
    }

    const disabledColumnKeysSet = Set(
      disabledColumnKeys.value.filter((fieldName) => fieldName != null),
    );

    if (!props.context.customFormState.isInitialized) {
      return <></>;
    }

    const tableData =
      props.context.customFormState.stream.loadedElements.flatMap(
        (chunk, chunkIndex) =>
          chunk.data.map((rowData, rowId) =>
            rowData.fields
              .filter((_, column) =>
                visibleColumns.value.columns.includes(column),
              )
              .map((_, column) =>
                EmbeddedCellTemplates.get(column)!(chunkIndex)(rowId)(
                  rowData.fields.get(column)!,
                )(disabledColumnKeysSet.has(column)),
              ),
          ),
      );

    console.log(props.context);
    return (
      <>
        <props.view
          {...props}
          context={{
            ...props.context,
          }}
          foreignMutations={{
            ...props.foreignMutations,
            loadMore: () =>
              props.setState(
                AbstractTableRendererState.Updaters.Template.loadMore(),
              ),
          }}
          TableHeaders={visibleColumns.value.columns}
          EmbeddedTableData={tableData}
        />
      </>
    );
  }).any([TableRunner, EmbeddedValueInfiniteStreamTemplate]);
};
