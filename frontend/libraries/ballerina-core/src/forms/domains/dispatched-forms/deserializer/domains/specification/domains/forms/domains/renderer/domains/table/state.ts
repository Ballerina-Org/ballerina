import { Map } from "immutable";
import {
  DispatchIsObject,
  DispatchParsedType,
  TableType,
} from "../../../../../types/state";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  isString,
  Renderer,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";

import { NestedRenderer } from "../nestedRenderer/state";
import { TableCellRenderer } from "./domains/tableCellRenderer/state";

export type SerializedTableRenderer = {
  type: string;
  renderer: string;
  columns: Map<string, unknown>;
  detailsRenderer?: unknown;
};

export type TableRenderer<T> = {
  kind: "tableRenderer";
  type: TableType<T>;
  columns: Map<string, TableCellRenderer<T>>;
  concreteRenderer: string;
  detailsRenderer?: NestedRenderer<T>;
  api?: string;
};

export const TableRenderer = {
  Default: <T>(
    type: TableType<T>,
    columns: Map<string, TableCellRenderer<T>>,
    concreteRenderer: string,
    detailsRenderer?: NestedRenderer<T>,
    api?: string,
  ): TableRenderer<T> => ({
    kind: "tableRenderer",
    type,
    columns,
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
              : !TableRenderer.Operations.hasValidApi(_)
                ? ValueOrErrors.Default.throwOne(
                    "table form renderer has a non string api property",
                  )
                : ValueOrErrors.Default.return({
                    ..._,
                    columns: Map<string, unknown>(_.columns),
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
    ): ValueOrErrors<[TableRenderer<T>, Map<string, Renderer<T>>], string> =>
      api != undefined && Array.isArray(api)
        ? ValueOrErrors.Default.throwOne<
            [TableRenderer<T>, Map<string, Renderer<T>>],
            string
          >("lookup api not supported for table")
        : TableRenderer.Operations.tryAsValidTableForm(serialized)
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
                      .Then(([columnsMap, accumulatedAlreadyParsedForms]) =>
                        TableRenderer.Operations.DeserializeDetailsRenderer(
                          type,
                          validTableForm,
                          concreteRenderers,
                          types,
                          forms,
                          accumulatedAlreadyParsedForms,
                        ).Then(([detailsRenderer, finalAlreadyParsedForms]) =>
                          ValueOrErrors.Default.return<
                            [TableRenderer<T>, Map<string, Renderer<T>>],
                            string
                          >([
                            TableRenderer.Default(
                              type,
                              columnsMap,
                              validTableForm.renderer,
                              detailsRenderer,
                              api,
                            ),
                            finalAlreadyParsedForms,
                          ]),
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
