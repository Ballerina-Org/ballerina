import {
  Expr,
  DispatchParsedType,
  MapRepo,
  TableType,
  Template,
  ValueOrErrors,
  TableAbstractRenderer,
  DispatchInjectablesTypes,
  PredicateValue,
  LookupType,
  LookupTypeAbstractRenderer,
} from "../../../../../../../../../main";

import { DispatchTableApiSource } from "../../../../../../../../../main";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { DispatcherContext } from "../../../../../deserializer/state";
import { List, Map } from "immutable";
import { TableRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/table/state";

export const TableDispatcher = {
  Operations: {
    GetApi: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      api: string | undefined,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
    ): ValueOrErrors<DispatchTableApiSource, string> =>
      api == undefined
        ? ValueOrErrors.Default.throwOne("internal error: api is not defined")
        : dispatcherContext.tableApiSources == undefined
          ? ValueOrErrors.Default.throwOne("table api sources are not defined")
          : Array.isArray(api)
            ? ValueOrErrors.Default.throwOne(
                "lookup api not supported for table",
              )
            : dispatcherContext.tableApiSources(api),
    DispatchDetailsRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: TableRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<undefined | Template<any, any, any, any>, string> =>
      renderer.detailsRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedDispatcher.Operations.DispatchAs(
            renderer.detailsRenderer,
            dispatcherContext,
            "table details renderer",
            isInlined,
            tableApi,
          ),
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: TableRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      tableApi: string | undefined,
      isInlined: boolean,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      DispatchParsedType.Operations.ResolveLookupType(
        renderer.type.arg.name,
        dispatcherContext.types,
      )
        .Then((tableEntityType) =>
          tableEntityType.kind == "record"
            ? ValueOrErrors.Operations.All(
                List<
                  ValueOrErrors<
                    [
                      string,
                      {
                        template: Template<any, any, any, any>;
                        disabled?: Expr;
                        GetDefaultState: () => any;
                        GetDefaultValue: () => any;
                      },
                    ],
                    string
                  >
                >(
                  renderer.columns
                    .entrySeq()
                    .toArray()
                    .map(([columnName, columnRenderer]) =>
                      MapRepo.Operations.tryFindWithError(
                        columnName,
                        tableEntityType.fields,
                        () =>
                          `cannot find column "${columnName}" in table entity type`,
                      ).Then((columnType) =>
                        NestedDispatcher.Operations.DispatchAs(
                          columnRenderer,
                          dispatcherContext,
                          `table column ${columnName}`,
                          isInlined,
                          tableApi,
                        ).Then((template) =>
                          dispatcherContext
                            .defaultState(columnType, columnRenderer.renderer)
                            .Then((defaultState) =>
                              dispatcherContext
                                .defaultValue(
                                  columnType,
                                  columnRenderer.renderer,
                                )
                                .Then((defaultValue) =>
                                  ValueOrErrors.Default.return<
                                    [
                                      string,
                                      {
                                        template: Template<any, any, any, any>;
                                        disabled?: Expr;
                                        label: string | undefined;
                                        GetDefaultState: () => any;
                                        GetDefaultValue: () => PredicateValue;
                                      },
                                    ],
                                    string
                                  >([
                                    columnName,
                                    {
                                      // Special attention - tables have a look up arg that represents the table entity type
                                      template: LookupTypeAbstractRenderer<
                                        CustomPresentationContexts,
                                        Flags,
                                        ExtraContext
                                      >(
                                        template,
                                        renderer.type.arg,
                                        dispatcherContext.IdProvider,
                                        dispatcherContext.ErrorRenderer,
                                      ).withView(
                                        dispatcherContext.lookupTypeRenderer(),
                                      ),
                                      disabled: columnRenderer.disabled,
                                      label: columnRenderer.label,
                                      GetDefaultState: () => defaultState,
                                      GetDefaultValue: () => defaultValue,
                                    },
                                  ]),
                                ),
                            ),
                        ),
                      ),
                    ),
                ),
              ).Then((cellTemplates) =>
                TableDispatcher.Operations.DispatchDetailsRenderer(
                  renderer,
                  dispatcherContext,
                  isInlined,
                  tableApi,
                ).Then((detailsRenderer) =>
                  dispatcherContext
                    .getConcreteRenderer("table", renderer.concreteRenderer)
                    .Then((concreteRenderer) =>
                      TableDispatcher.Operations.GetApi(
                        renderer.api ?? tableApi,
                        dispatcherContext,
                      ).Then((tableApiSource) =>
                        ValueOrErrors.Default.return(
                          TableAbstractRenderer(
                            Map(cellTemplates),
                            detailsRenderer,
                            renderer.visibleColumns,
                            dispatcherContext.IdProvider,
                            dispatcherContext.ErrorRenderer,
                            tableEntityType,
                          )
                            .mapContext((_: any) => ({
                              ..._,
                              type: renderer.type,
                              apiMethods:
                                tableApi == undefined
                                  ? []
                                  : (dispatcherContext.specApis.tables?.get(
                                      tableApi!,
                                    )?.methods ?? []),
                              tableApiSource,
                              fromTableApiParser:
                                dispatcherContext.parseFromApiByType(
                                  renderer.type.arg,
                                ),
                            }))
                            .withView(concreteRenderer),
                        ),
                      ),
                    ),
                ),
              )
            : ValueOrErrors.Default.throwOne<
                Template<any, any, any, any>,
                string
              >(
                `expected a record type, but got a ${tableEntityType.kind} type`,
              ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching as table form`),
        ),
  },
};
