import {
  Expr,
  DispatchParsedType,
  MapRepo,
  Template,
  ValueOrErrors,
  TableAbstractRenderer,
  DispatchInjectablesTypes,
  PredicateValue,
  LookupTypeAbstractRenderer,
  Dispatcher,
  Value,
  SumNType,
} from "../../../../../../../../../main";

import { DispatchTableApiSource } from "../../../../../../../../../main";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { TableRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/table/state";
import { DispatcherContextWithApiSources } from "../../../../coroutines/runner";
import { List, Map } from "immutable";

export const TableDispatcher = {
  Operations: {
    GetApi: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      api: string | undefined,
      dispatcherContext: DispatcherContextWithApiSources<
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
      dispatcherContext: DispatcherContextWithApiSources<
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
      dispatcherContext: DispatcherContextWithApiSources<
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
                ).Then((detailsRenderer) => {
                  const api = renderer.api ?? tableApi;
                  const filtering =
                    api == undefined
                      ? undefined
                      : (dispatcherContext.specApis.tables?.get(api!)
                          ?.filtering ?? undefined);

                  // TODO - this can be significantly improved, also to provide an error if something fails
                  const AllowedFilters: Map<
                    string,
                    {
                      template: Template<any, any, any, any>;
                      filters: SumNType<any>;
                      type: DispatchParsedType<any>;
                      GetDefaultValue: () => PredicateValue;
                      GetDefaultState: () => any;
                    }
                  > = (() => {
                    if (filtering == undefined) {
                      return Map([]);
                    }
                    return filtering
                      .map((columnFilters) => ({
                        template: Dispatcher.Operations.DispatchAs(
                          columnFilters.displayRenderer,
                          dispatcherContext,
                          "table column filter renderer",
                          false,
                          isInlined,
                          tableApi,
                        ),
                        filters: columnFilters.filters,
                        type: columnFilters.displayType,
                        GetDefaultValue: () =>
                          dispatcherContext.defaultValue(
                            columnFilters.displayType,
                            columnFilters.displayRenderer,
                          ),
                        GetDefaultState: () =>
                          dispatcherContext.defaultState(
                            columnFilters.displayType,
                            columnFilters.displayRenderer,
                          ),
                      }))
                      .filter(
                        (dispatchedFilterRenderer) =>
                          dispatchedFilterRenderer.template.kind == "value" &&
                          dispatchedFilterRenderer.GetDefaultValue().kind ==
                            "value" &&
                          dispatchedFilterRenderer.GetDefaultState().kind ==
                            "value",
                      )
                      .map((dispatchedFilterRenderer) => ({
                        template: (
                          dispatchedFilterRenderer.template as Value<
                            Template<any, any, any, any>
                          >
                        ).value,
                        filters: dispatchedFilterRenderer.filters,
                        type: dispatchedFilterRenderer.type,
                        GetDefaultValue: () =>
                          (
                            dispatchedFilterRenderer.GetDefaultValue() as Value<PredicateValue>
                          ).value,
                        GetDefaultState: () =>
                          (
                            dispatchedFilterRenderer.GetDefaultState() as Value<any>
                          ).value,
                      }));
                  })();

                  return dispatcherContext
                    .getConcreteRenderer("table", renderer.concreteRenderer)
                    .Then((concreteRenderer) =>
                      TableDispatcher.Operations.GetApi(
                        renderer.api ?? tableApi,
                        dispatcherContext,
                      ).Then((tableApiSource) => {
                        const highlightedFilters =
                          api == undefined
                            ? []
                            : (dispatcherContext.specApis.tables
                                ?.get(api!)
                                ?.highlightedFilters?.toArray() ?? []);

                        const sorting =
                          api == undefined
                            ? []
                            : (dispatcherContext.specApis.tables?.get(api!)
                                ?.sorting ?? []);
                        return ValueOrErrors.Default.return(
                          TableAbstractRenderer(
                            Map(cellTemplates),
                            detailsRenderer,
                            renderer.visibleColumns,
                            dispatcherContext.IdProvider,
                            dispatcherContext.ErrorRenderer,
                            tableEntityType,
                            AllowedFilters,
                            dispatcherContext.parseToApiByType,
                            dispatcherContext.parseFromApiByType,
                            dispatcherContext.parseFromApiByType(
                              renderer.type.arg,
                            ),
                            tableApiSource,
                          )
                            .mapContext((_: any) => ({
                              ..._,
                              type: renderer.type,
                              apiMethods:
                                api == undefined
                                  ? []
                                  : (dispatcherContext.specApis.tables?.get(
                                      api!,
                                    )?.methods ?? []),
                              sorting,
                              highlightedFilters,
                            }))
                            .withView(concreteRenderer),
                        );
                      }),
                    );
                }),
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
