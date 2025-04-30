import {
  DispatcherContext,
  DispatchTableApiSource,
  MapRepo,
  NestedDispatcher,
  TableAbstractRenderer,
  Template,
} from "../../../../../../../main";
import { ValueOrErrors } from "../../../../../../collections/domains/valueOrErrors/state";
import { List, Map } from "immutable";
import { Expr } from "../../../../parser/domains/predicates/state";
import { TableType } from "../../../deserializer/domains/specification/domains/types/state";
import { TableFormRenderer } from "../../../deserializer/domains/specification/domains/form/domains/renderers/domains/tableFormRenderer/state";

export const TableFormDispatcher = {
  Operations: {
    GetApi: (
      api: string | string[],
      dispatcherContext: DispatcherContext<any>,
    ): ValueOrErrors<DispatchTableApiSource, string> =>
      typeof api == "string"
        ? dispatcherContext.tableApiSources == undefined
          ? ValueOrErrors.Default.throwOne("table api sources are not defined")
          : dispatcherContext.tableApiSources(api)
        : //TODO
          ValueOrErrors.Default.throwOne(
            "look up table api source not yet supported",
          ),
    DispatchDetailsRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      renderer: TableFormRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<undefined | Template<any, any, any, any>, string> =>
      renderer.detailsRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedDispatcher.Operations.Dispatch(
            renderer.type,
            renderer.detailsRenderer,
            dispatcherContext,
          ),
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: TableType<T>,
      renderer: TableFormRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
      api: string | string[],
      isNested: boolean = false,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      MapRepo.Operations.tryFindWithError(
        type.typeName,
        dispatcherContext.types,
        () => `cannot find type "${renderer.type.typeName}" in types`,
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
                          columnType,
                          columnRenderer,
                          dispatcherContext,
                          `table column ${columnName}`,
                        ).Then((template) =>
                          dispatcherContext
                            .defaultState(columnType, columnRenderer)
                            .Then((defaultState) =>
                              dispatcherContext
                                .defaultValue(columnType, columnRenderer)
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
                TableFormDispatcher.Operations.DispatchDetailsRenderer(
                  renderer,
                  dispatcherContext,
                ).Then((detailsRenderer) =>
                  dispatcherContext
                    .getConcreteRenderer(
                      "table",
                      renderer.concreteRendererName,
                      isNested,
                    )
                    .Then((concreteRenderer) =>
                      TableFormDispatcher.Operations.GetApi(
                        api,
                        dispatcherContext,
                      ).Then((tableApiSource) =>
                        ValueOrErrors.Default.return(
                          TableAbstractRenderer(
                            Map(cellTemplates),
                            detailsRenderer,
                            renderer.visibleColumns,
                          )
                            .mapContext((_: any) => ({
                              ..._,
                              type: renderer.type,
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
          errors.map((error) => `${error}\n...When dispatching as table form`),
        ),
  },
};
