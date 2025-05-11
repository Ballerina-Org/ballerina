import { List, Map } from "immutable";
import {
  DispatchIsObject,
  DispatchParsedType,
  RecordType,
  TableType,
} from "../../../../../types/state";
import {
  ConcreteRendererKinds,
  isString,
  MapRepo,
  PredicateVisibleColumns,
  TableLayout,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";

import { NestedRenderer } from "../nestedRenderer/state";
import { TableCellRenderer } from "./domains/tableCellRenderer/state";
import { Renderer } from "../../state";

export type SerializedTableRenderer = {
  type: string;
  renderer: unknown;
  columns: Map<string, unknown>;
  detailsRenderer?: unknown;
  visibleColumns: unknown;
};

export type TableRenderer<T> = {
  kind: "tableRenderer";
  type: TableType<T>;
  columns: Map<string, TableCellRenderer<T>>;
  visibleColumns: PredicateVisibleColumns;
  renderer: Renderer<T>;
  detailsRenderer?: NestedRenderer<T>;
  api?: string;
};

export const TableRenderer = {
  Default: <T>(
    type: TableType<T>,
    columns: Map<string, TableCellRenderer<T>>,
    visibleColumns: PredicateVisibleColumns,
    renderer: Renderer<T>,
    detailsRenderer?: NestedRenderer<T>,
    api?: string,
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
    hasValidApi: (_: unknown): _ is { api?: string } =>
      DispatchIsObject(_) && (("api" in _ && isString(_.api)) || !("api" in _)),
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
        : !TableRenderer.Operations.hasType(_)
          ? ValueOrErrors.Default.throwOne(
              "table form renderer is missing or has invalid type property",
            )
          : !TableRenderer.Operations.hasRenderer(_)
            ? ValueOrErrors.Default.throwOne(
                "table form renderer is missing or has invalid renderer property",
              )
            : !TableRenderer.Operations.hasColumns(_)
              ? ValueOrErrors.Default.throwOne(
                  "table form renderer is missing or has invalid columns property",
                )
              : !TableRenderer.Operations.hasVisibleColumns(_)
                ? ValueOrErrors.Default.throwOne(
                    "table form renderer is missing or has invakid visible columns property",
                  )
                : !TableRenderer.Operations.hasValidApi(_)
                  ? ValueOrErrors.Default.throwOne(
                      "table form renderer has a non string api property",
                    )
                  : ValueOrErrors.Default.return({
                      ..._,
                      columns: Map<string, unknown>(_.columns),
                      visibleColumns: _.visibleColumns,
                      api: _?.api,
                    }),
    DeserializeDetailsRenderer: <T>(
      type: TableType<T>,
      serialized: SerializedTableRenderer,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<NestedRenderer<T> | undefined, string> =>
      serialized.detailsRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            serialized.detailsRenderer,
            concreteRenderers,
            "details renderer",
            types,
            typeof serialized.detailsRenderer == "object" &&
              "renderer" in serialized.detailsRenderer &&
              typeof serialized.detailsRenderer.renderer == "object",
          ),
    Deserialize: <T>(
      type: TableType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
      api?: string | string[],
    ): ValueOrErrors<TableRenderer<T>, string> =>
      api != undefined && Array.isArray(api)
        ? ValueOrErrors.Default.throwOne("lookup api not supported for table")
        : TableRenderer.Operations.tryAsValidTableForm(serialized)
            .Then((validTableForm) =>
              DispatchParsedType.Operations.AsResolvedType(
                type.args[0],
                types,
              ).Then((resolvedType) =>
                resolvedType.kind != "record"
                  ? ValueOrErrors.Default.throwOne<TableRenderer<T>, string>(
                      `table arg ${JSON.stringify(
                        resolvedType.kind,
                      )} is not a record type`,
                    )
                  : ValueOrErrors.Operations.All(
                      List<
                        ValueOrErrors<[string, TableCellRenderer<T>], string>
                      >(
                        validTableForm.columns
                          .toArray()
                          .map(([columnName, columnRenderer]) =>
                            DispatchParsedType.Operations.ResolveLookupType(
                              columnName,
                              resolvedType.fields,
                            ).Then((columnType) =>
                              TableCellRenderer.Operations.Deserialize(
                                columnType,
                                columnRenderer,
                                concreteRenderers,
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
                        TableRenderer.Operations.DeserializeDetailsRenderer(
                          type,
                          validTableForm,
                          concreteRenderers,
                          types,
                        ).Then((detailsRenderer) =>
                          Renderer.Operations.Deserialize(
                            type,
                            validTableForm.renderer,
                            concreteRenderers,
                            types,
                          ).Then((renderer) =>
                            ValueOrErrors.Default.return(
                              TableRenderer.Default(
                                type,
                                Map<string, TableCellRenderer<T>>(columns),
                                layout,
                                renderer,
                                detailsRenderer,
                                api,
                              ),
                            ),
                          ),
                        ),
                      ),
                    ),
              ),
            )
            .MapErrors((errors) =>
              errors.map(
                (error) => `${error}\n...When parsing as TableForm renderer`,
              ),
            ),
  },
};
