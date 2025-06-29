import { List, Map } from "immutable";
import {
  DispatchIsObject,
  DispatchParsedType,
  TableType,
} from "../../../../../types/state";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  isString,
  PredicateVisibleColumns,
  TableLayout,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";

import { NestedRenderer } from "../nestedRenderer/state";
import { TableCellRenderer } from "./domains/tableCellRenderer/state";

export type SerializedTableRenderer = {
  type: string;
  renderer: string;
  columns: Map<string, unknown>;
  detailsRenderer?: unknown;
  visibleColumns: unknown;
};

export type TableRenderer<T> = {
  kind: "tableRenderer";
  type: TableType<T>;
  columns: Map<string, TableCellRenderer<T>>;
  visibleColumns: PredicateVisibleColumns;
  concreteRenderer: string;
  detailsRenderer?: NestedRenderer<T>;
  api?: string;
};

export const TableRenderer = {
  Default: <T>(
    type: TableType<T>,
    columns: Map<string, TableCellRenderer<T>>,
    visibleColumns: PredicateVisibleColumns,
    concreteRenderer: string,
    detailsRenderer?: NestedRenderer<T>,
    api?: string,
  ): TableRenderer<T> => ({
    kind: "tableRenderer",
    type,
    columns,
    visibleColumns,
    concreteRenderer,
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
    DeserializeDetailsRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: TableType<T>,
      serialized: SerializedTableRenderer,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<NestedRenderer<T> | undefined, string> =>
      serialized.detailsRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedRenderer.Operations.DeserializeAs(
            type.arg,
            serialized.detailsRenderer,
            concreteRenderers,
            "details renderer",
            types,
          ),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: TableType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
      api: string | undefined,
    ): ValueOrErrors<TableRenderer<T>, string> =>
      api != undefined && Array.isArray(api)
        ? ValueOrErrors.Default.throwOne("lookup api not supported for table")
        : TableRenderer.Operations.tryAsValidTableForm(serialized)
            .Then((validTableForm) =>
              DispatchParsedType.Operations.ResolveLookupType(
                type.arg.name,
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
                          ValueOrErrors.Default.return(
                            TableRenderer.Default(
                              type,
                              Map<string, TableCellRenderer<T>>(columns),
                              layout,
                              validTableForm.renderer,
                              detailsRenderer,
                              api,
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
