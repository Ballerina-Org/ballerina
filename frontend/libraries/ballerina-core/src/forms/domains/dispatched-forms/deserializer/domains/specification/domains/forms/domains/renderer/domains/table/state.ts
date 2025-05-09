import { List, Map } from "immutable";
import {
  DispatchIsObject,
  DispatchParsedType,
  TableType,
} from "../../../../../types/state";
import {
  isString,
  MapRepo,
  PredicateVisibleColumns,
  TableLayout,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";

import { NestedRenderer } from "../nestedRenderer/state";
import { RecordRenderer } from "../record/state";
import { TableCellRenderer } from "./domains/tableCellRenderer/state";
import { Renderer } from "../../state";

export type SerializedTableRenderer = {
  type: string;
  renderer: unknown;
  columns: Map<string, unknown>;
  detailsRenderer?: unknown;
  visibleColumns: unknown;
  api: string;
};

export type TableRenderer<T> = {
  kind: "tableRenderer";
  type: TableType<T>;
  columns: Map<string, TableCellRenderer<T>>;
  visibleColumns: PredicateVisibleColumns;
  renderer: Renderer<T>;
  detailsRenderer?: NestedRenderer<T>;
  api: string;
};

export const TableFormRenderer = {
  Default: <T>(
    type: TableType<T>,
    columns: Map<string, TableCellRenderer<T>>,
    visibleColumns: PredicateVisibleColumns,
    renderer: Renderer<T>,
    api: string,
    detailsRenderer?: NestedRenderer<T>,
  ): TableRenderer<T> => ({
    kind: "tableRenderer",
    type,
    columns,
    visibleColumns,
    renderer,
    detailsRenderer,
    api,
  }),
  Operations: {
    hasType: (_: unknown): _ is { type: string } =>
      DispatchIsObject(_) && "type" in _ && isString(_.type),
    hasRenderer: (_: unknown): _ is { renderer: string } =>
      DispatchIsObject(_) && "renderer" in _ && isString(_.renderer),
    hasColumns: (_: unknown): _ is { columns: Map<string, unknown> } =>
      DispatchIsObject(_) && "columns" in _ && DispatchIsObject(_.columns),
    hasVisibleColumns: (
      _: unknown,
    ): _ is { visibleColumns: object | Array<unknown> } =>
      DispatchIsObject(_) &&
      "visibleColumns" in _ &&
      (DispatchIsObject(_.visibleColumns) || Array.isArray(_.visibleColumns)),
    tryAsValidTableForm: (
      _: unknown,
    ): ValueOrErrors<SerializedTableRenderer, string> =>
      !DispatchIsObject(_)
        ? ValueOrErrors.Default.throwOne("table form renderer not an object")
        : !TableFormRenderer.Operations.hasType(_)
        ? ValueOrErrors.Default.throwOne(
            "table form renderer is missing or has invalid type property",
          )
        : !TableFormRenderer.Operations.hasRenderer(_)
        ? ValueOrErrors.Default.throwOne(
            "table form renderer is missing or has invalid renderer property",
          )
        : !TableFormRenderer.Operations.hasColumns(_)
        ? ValueOrErrors.Default.throwOne(
            "table form renderer is missing or has invalid columns property",
          )
        : !TableFormRenderer.Operations.hasVisibleColumns(_)
        ? ValueOrErrors.Default.throwOne(
            "table form renderer is missing or has invakid visible columns property",
          )
        : !("api" in _) || typeof _.api != "string"
        ? ValueOrErrors.Default.throwOne(
            "table form renderer is missing or has non string api property",
          )
        : ValueOrErrors.Default.return({
            ..._,
            columns: Map<string, unknown>(_.columns),
            visibleColumns: _.visibleColumns,
            api: _.api,
          }),
    DeserializeDetailsRenderer: <T>(
      type: TableType<T>,
      serialized: SerializedTableRenderer,
      fieldViews: any,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<NestedRenderer<T> | undefined, string> =>
      serialized.detailsRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedRenderer.Operations.DeserializeAs(
            type.args[0], // TODO check it should be type.args[0]
            serialized.detailsRenderer,
            fieldViews,
            "details renderer",
            types,
          ),
    Deserialize: <T>(
      type: TableType<T>,
      serialized: SerializedTableRenderer,
      types: Map<string, DispatchParsedType<T>>,
      fieldViews: any,
    ): ValueOrErrors<TableRenderer<T>, string> =>
      TableFormRenderer.Operations.tryAsValidTableForm(serialized).Then(
        (validTableForm) =>
          MapRepo.Operations.tryFindWithError(
            type.typeName,
            types,
            () => `cannot find table type ${type.typeName} in types`,
          ).Then((tableType) =>
            ValueOrErrors.Operations.All(
              List<ValueOrErrors<[string, TableCellRenderer<T>], string>>(
                validTableForm.columns
                  .toArray()
                  .map(([columnName, columnRenderer]) =>
                    tableType.kind != "record" // need to support other types, move to an operation
                      ? ValueOrErrors.Default.throwOne(
                          `table type ${type.typeName} is not a record`,
                        )
                      : MapRepo.Operations.tryFindWithError(
                          columnName,
                          tableType.fields,
                          () =>
                            `cannot find column ${columnName} in table type ${type.typeName}`,
                        ).Then((columnType) =>
                          TableCellRenderer.Operations.Deserialize(
                            columnType,
                            columnRenderer,
                            fieldViews,
                            types,
                            columnName,
                          ).Then((renderer) =>
                            ValueOrErrors.Default.return<
                              [string, TableCellRenderer<T>],
                              string
                            >([columnName, renderer]),
                          ),
                        ),
                  ),
              ),
            ).Then((columns) =>
              TableLayout.Operations.ParseLayout(
                validTableForm.visibleColumns,
              ).Then((layout) =>
                TableFormRenderer.Operations.DeserializeDetailsRenderer(
                  type,
                  validTableForm,
                  fieldViews,
                  types,
                ).Then((detailsRenderer) =>
                  Renderer.Operations.Deserialize(
                    type,
                    validTableForm.renderer,
                    fieldViews,
                    types,
                  ).Then((renderer) =>
                    ValueOrErrors.Default.return(
                      TableFormRenderer.Default(
                        type,
                        Map<string, TableCellRenderer<T>>(columns),
                        layout,
                        renderer,
                        validTableForm.api,
                        detailsRenderer,
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
      ),
    // TODO - detail view
  },
};
