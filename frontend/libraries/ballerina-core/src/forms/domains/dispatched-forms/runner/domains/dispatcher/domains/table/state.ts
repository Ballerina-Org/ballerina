import {
  Expr,
  DispatchParsedType,
  MapRepo,
  TableType,
  Template,
  ValueOrErrors,
  TableAbstractRenderer,
} from "../../../../../../../../../main";

import { DispatchTableApiSource } from "../../../../../../../../../main";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { DispatcherContext } from "../../../../../deserializer/state";
import { List, Map } from "immutable";
import { TableRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/table/state";

export const TableDispatcher = {
  Operations: {
    GetApi: (
      api: string | string[] | undefined,
      dispatcherContext: DispatcherContext<any>,
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
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      renderer: TableRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<undefined | Template<any, any, any, any>, string> =>
      renderer.detailsRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedDispatcher.Operations.DispatchAs(
            renderer.detailsRenderer,
            dispatcherContext,
            "table details renderer",
            "details",
          ),
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: TableType<T>,
      renderer: TableRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
      api: string | string[] | undefined,
      isNested: boolean,
      launcherName?: string,
      formName?: string,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      !isNested && (!launcherName || !formName)
        ? ValueOrErrors.Default.throwOne<Template<any, any, any, any>, string>(
            `no launcher name or form name provided for top level table form`,
          )
        : DispatchParsedType.Operations.ResolveLookupType(
            type.args[0].typeName,
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
                            ).Then((template) =>
                              dispatcherContext
                                .defaultState(
                                  columnType,
                                  columnRenderer.renderer,
                                )
                                .Then((defaultState) =>
                                  dispatcherContext
                                    .defaultValue(
                                      columnType,
                                      columnRenderer.renderer,
                                    )
                                    .Then((defaultValue) =>
                                      ValueOrErrors.Default.return([
                                        columnName,
                                        {
                                          template,
                                          disabled: columnRenderer.disabled,
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
                    ).Then((detailsRenderer) =>
                      renderer.renderer.kind != "lookupRenderer"
                        ? ValueOrErrors.Default.throwOne<
                            Template<any, any, any, any>,
                            string
                          >(
                            `received non lookup renderer kind for table concrete renderer`,
                          )
                        : dispatcherContext
                            .getConcreteRenderer(
                              "table",
                              renderer.renderer.renderer,
                              isNested,
                            )
                            .Then((concreteRenderer) =>
                              TableDispatcher.Operations.GetApi(
                                renderer.api ?? api,
                                dispatcherContext,
                              ).Then((tableApiSource) =>
                                ValueOrErrors.Default.return(
                                  TableAbstractRenderer(
                                    Map(cellTemplates),
                                    detailsRenderer,
                                    renderer.visibleColumns,
                                    dispatcherContext.IdWrapper,
                                    dispatcherContext.ErrorRenderer,
                                  )
                                    .mapContext((_: any) => ({
                                      ..._,
                                      ...(!isNested && launcherName
                                        ? {
                                            identifiers: {
                                              withLauncher: `[${launcherName}][${formName}]`,
                                              withoutLauncher: `[${formName}]`,
                                            },
                                          }
                                        : {}),
                                      tableApiSource,
                                      fromTableApiParser:
                                        dispatcherContext.parseFromApiByType(
                                          renderer.type.args[0],
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
              errors.map(
                (error) => `${error}\n...When dispatching as table form`,
              ),
            ),
  },
};
