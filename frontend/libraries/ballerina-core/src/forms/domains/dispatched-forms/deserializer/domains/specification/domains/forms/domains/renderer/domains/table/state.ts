import { List, Map } from "immutable";
import {
  DispatchIsObject,
  DispatchParsedType,
  TableType,
} from "../../../../../types/state";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  ColumnsConfigSource,
  isString,
  Renderer,
  SpecVersion,
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
  disabledColumns: unknown;
};

export type TableRenderer<T> = {
  kind: "tableRenderer";
  type: TableType<T>;
  columns: Map<string, TableCellRenderer<T>>;
  columnsConfig: ColumnsConfigSource;
  concreteRenderer: string;
  detailsRenderer?: NestedRenderer<T>;
  api?: string;
};

export const TableRenderer = {
  Default: <T>(
    type: TableType<T>,
    columns: Map<string, TableCellRenderer<T>>,
    columnsConfig: ColumnsConfigSource,
    concreteRenderer: string,
    detailsRenderer?: NestedRenderer<T>,
    api?: string,
  ): TableRenderer<T> => ({
    kind: "tableRenderer",
    type,
    columns,
    columnsConfig,
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
      expectVisibleColumns: boolean,
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
              : expectVisibleColumns &&
                  !TableRenderer.Operations.hasVisibleColumns(_)
                ? ValueOrErrors.Default.throwOne(
                    "table form renderer is missing or has invalid visible columns property",
                  )
                : !TableRenderer.Operations.hasValidApi(_)
                  ? ValueOrErrors.Default.throwOne(
                      "table form renderer has a non string api property",
                    )
                  : ValueOrErrors.Default.return({
                      ..._,
                      columns: Map<string, unknown>(_.columns),
                      visibleColumns: expectVisibleColumns
                        ? // just to make the typechecker happy
                          TableRenderer.Operations.hasVisibleColumns(_)
                          ? _.visibleColumns
                          : undefined
                        : undefined,
                      disabledColumns:
                        "disabledColumns" in _ ? _.disabledColumns : undefined,
                      api: _?.api,
                    }),
    DeserializeDetailsRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      type: TableType<T>,
      serialized: SerializedTableRenderer,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
      forms: object,
      alreadyParsedForms: Map<string, Renderer<T>>,
      specVersionContext: SpecVersion,
    ): ValueOrErrors<
      [NestedRenderer<T> | undefined, Map<string, Renderer<T>>],
      string
    > =>
      serialized.detailsRenderer == undefined
        ? ValueOrErrors.Default.return<
            [NestedRenderer<T> | undefined, Map<string, Renderer<T>>],
            string
          >([undefined, alreadyParsedForms])
        : NestedRenderer.Operations.DeserializeAs(
            type.arg,
            serialized.detailsRenderer,
            concreteRenderers,
            "details renderer",
            types,
            forms,
            alreadyParsedForms,
            specVersionContext,
          ),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      type: TableType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
      api: string | undefined,
      forms: object,
      alreadyParsedForms: Map<string, Renderer<T>>,
      specVersionContext: SpecVersion,
    ): ValueOrErrors<[TableRenderer<T>, Map<string, Renderer<T>>], string> =>
      api != undefined && Array.isArray(api)
        ? ValueOrErrors.Default.throwOne<
            [TableRenderer<T>, Map<string, Renderer<T>>],
            string
          >("lookup api not supported for table")
        : TableRenderer.Operations.tryAsValidTableForm(
            serialized,
            specVersionContext.kind == "v1", // v1 spec version expects visibleColumns property
          )
            .Then((validTableForm) =>
              DispatchParsedType.Operations.ResolveLookupType(
                type.arg.name,
                types,
              ).Then((resolvedType) =>
                resolvedType.kind != "record"
                  ? ValueOrErrors.Default.throwOne<
                      [TableRenderer<T>, Map<string, Renderer<T>>],
                      string
                    >(
                      `table arg ${JSON.stringify(
                        resolvedType.kind,
                      )} is not a record type`,
                    )
                  : validTableForm.columns
                      .toArray()
                      .reduce<
                        ValueOrErrors<
                          [
                            Map<string, TableCellRenderer<T>>,
                            Map<string, Renderer<T>>,
                          ],
                          string
                        >
                      >(
                        (acc, [columnName, columnRenderer]) =>
                          acc.Then(
                            ([columnsMap, accumulatedAlreadyParsedForms]) =>
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
                                  forms,
                                  accumulatedAlreadyParsedForms,
                                  specVersionContext,
                                ).Then(([renderer, newAlreadyParsedForms]) =>
                                  ValueOrErrors.Default.return<
                                    [
                                      Map<string, TableCellRenderer<T>>,
                                      Map<string, Renderer<T>>,
                                    ],
                                    string
                                  >([
                                    columnsMap.set(columnName, renderer),
                                    newAlreadyParsedForms,
                                  ]),
                                ),
                              ),
                          ),
                        ValueOrErrors.Default.return<
                          [
                            Map<string, TableCellRenderer<T>>,
                            Map<string, Renderer<T>>,
                          ],
                          string
                        >([
                          Map<string, TableCellRenderer<T>>(),
                          alreadyParsedForms,
                        ]),
                      )
                      .Then(([columnsMap, accumulatedAlreadyParsedForms]) => {
                        const ComputeFieldConfigsSource =
                          // keep backward compatibility with v1 spec version
                          specVersionContext.kind == "v1"
                            ? TableLayout.Operations.ParseLayout(
                                validTableForm.visibleColumns,
                              ).Then((visibileColumnsLayout) =>
                                TableLayout.Operations.ParseLayout(
                                  validTableForm.disabledColumns,
                                ).Then((disabledColumnsLayout) =>
                                  ValueOrErrors.Default.return<
                                    ColumnsConfigSource,
                                    string
                                  >({
                                    kind: "raw",
                                    visiblePredicate: visibileColumnsLayout,
                                    disabledPredicate: disabledColumnsLayout,
                                  }),
                                ),
                              )
                            : // new v1-preprocessed spec version
                              ValueOrErrors.Default.return<
                                ColumnsConfigSource,
                                string
                              >({
                                kind: "preprocessed",
                                visiblePaths: specVersionContext.visiblePaths,
                                disabledPaths: specVersionContext.disabledPaths,
                              });

                        return ComputeFieldConfigsSource.Then(
                          (fieldConfigsSource) => {
                            return TableRenderer.Operations.DeserializeDetailsRenderer(
                              type,
                              validTableForm,
                              concreteRenderers,
                              types,
                              forms,
                              accumulatedAlreadyParsedForms,
                              specVersionContext,
                            ).Then(
                              ([detailsRenderer, finalAlreadyParsedForms]) =>
                                ValueOrErrors.Default.return<
                                  [TableRenderer<T>, Map<string, Renderer<T>>],
                                  string
                                >([
                                  TableRenderer.Default(
                                    type,
                                    columnsMap,
                                    fieldConfigsSource,
                                    validTableForm.renderer,
                                    detailsRenderer,
                                    api,
                                  ),
                                  finalAlreadyParsedForms,
                                ]),
                            );
                          },
                        );
                      }),
              ),
            )
            .MapErrors((errors) =>
              errors.map(
                (error) => `${error}\n...When parsing as TableForm renderer`,
              ),
            ),
  },
};
