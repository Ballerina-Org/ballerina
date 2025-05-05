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

import { BaseRenderer, SerializedBaseRenderer } from "../baseRenderer/state";
import { RecordFormRenderer } from "../recordFormRenderer/state";

export type SerializedTableFormRenderer = {
  type?: unknown;
  renderer?: unknown;
  columns?: unknown;
  detailsRenderer?: unknown;
  visibleColumns?: unknown;
  api?: unknown;
};

export type TableFormRenderer<T> = {
  kind: "tableForm";
  type: TableType<T>;
  columns: Map<
    string,
    BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>
  >;
  visibleColumns: PredicateVisibleColumns;
  concreteRendererName: string;
  detailsRenderer?:
    | BaseRenderer<T>
    | TableFormRenderer<T>
    | RecordFormRenderer<T>;
  inlinedApi?: string[];
  visible: undefined; // for parity with base renderers
  disabled: undefined;
};

export const TableFormRenderer = {
  Default: <T>(
    type: TableType<T>,
    columns: Map<
      string,
      BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>
    >,
    visibleColumns: PredicateVisibleColumns,
    concreteRendererName: string,
    detailsRenderer?:
      | BaseRenderer<T>
      | TableFormRenderer<T>
      | RecordFormRenderer<T>,
    inlinedApi?: string[],
  ): TableFormRenderer<T> => ({
    kind: "tableForm",
    type,
    columns,
    visibleColumns,
    concreteRendererName,
    detailsRenderer,
    inlinedApi,
    visible: undefined,
    disabled: undefined,
  }),
  Operations: {
    hasType: (_: unknown): _ is { type: string } =>
      DispatchIsObject(_) && "type" in _ && isString(_.type),
    hasRenderer: (_: unknown): _ is { renderer: string } =>
      DispatchIsObject(_) && "renderer" in _ && isString(_.renderer),
    hasColumns: (
      _: unknown,
    ): _ is { columns: Map<string, SerializedBaseRenderer> } => // this is a untruthful, better to do this in a "withColumn" operation, and better for deserialise to accept unknown and check
      DispatchIsObject(_) && "columns" in _ && DispatchIsObject(_.columns),
    hasVisibleColumns: (
      _: unknown,
    ): _ is { visibleColumns: object | Array<unknown> } =>
      DispatchIsObject(_) &&
      "visibleColumns" in _ &&
      (DispatchIsObject(_.visibleColumns) || Array.isArray(_.visibleColumns)),
    tryAsValidTableForm: (
      _: SerializedTableFormRenderer,
    ): ValueOrErrors<
      Omit<
        SerializedTableFormRenderer,
        "type" | "renderer" | "columns" | "visibleColumns"
      > & {
        type: string;
        renderer: string;
        columns: Map<string, SerializedBaseRenderer>;
        visibleColumns: object | Array<unknown>;
      },
      string
    > =>
      // TODO: consider using "withType" etc. which return value or errors
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
                : ValueOrErrors.Default.return({
                    ..._,
                    columns: Map<string, SerializedBaseRenderer>(_.columns),
                  }),
    DeserializeDetailsRenderer: <T>(
      type: TableType<T>,
      serialized: SerializedTableFormRenderer,
      fieldViews: any,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<
      | BaseRenderer<T>
      | TableFormRenderer<T>
      | RecordFormRenderer<T>
      | undefined,
      string
    > =>
      serialized.detailsRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : BaseRenderer.Operations.DeserializeAs(
            type.args[0], // TODO check it should be type.args[0]
            serialized.detailsRenderer,
            fieldViews,
            "nested",
            "details renderer",
            types,
          ),
    Deserialize: <T>(
      type: TableType<T>,
      serialized: SerializedTableFormRenderer,
      types: Map<string, DispatchParsedType<T>>,
      fieldViews: any,
    ): ValueOrErrors<TableFormRenderer<T>, string> =>
      TableFormRenderer.Operations.tryAsValidTableForm(serialized).Then(
        (validTableForm) =>
          MapRepo.Operations.tryFindWithError(
            type.typeName,
            types,
            () => `cannot find table type ${type.typeName} in types`,
          ).Then((tableType) =>
            ValueOrErrors.Operations.All(
              List<
                ValueOrErrors<
                  [
                    string,
                    (
                      | BaseRenderer<T>
                      | TableFormRenderer<T>
                      | RecordFormRenderer<T>
                    ),
                  ],
                  string
                >
              >(
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
                          BaseRenderer.Operations.DeserializeAs(
                            columnType,
                            columnRenderer,
                            fieldViews,
                            "tableColumn",
                            `column ${columnName}`,
                            types,
                          ).Then((renderer) =>
                            ValueOrErrors.Default.return<
                              [
                                string,
                                (
                                  | BaseRenderer<T>
                                  | TableFormRenderer<T>
                                  | RecordFormRenderer<T>
                                ),
                              ],
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
                  ValueOrErrors.Default.return(
                    TableFormRenderer.Default(
                      type,
                      Map<
                        string,
                        | BaseRenderer<T>
                        | TableFormRenderer<T>
                        | RecordFormRenderer<T>
                      >(columns),
                      layout,
                      validTableForm.renderer,
                      detailsRenderer,
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
