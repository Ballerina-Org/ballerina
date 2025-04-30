import { Template } from "../../../../../../template/state";
import { NestedPrimitiveDispatcher } from "./domains/nestedPrimitiveDispatcher/state";
import { NestedSumDispatcher } from "./domains/nestedSumDispatcher/state";
import { NestedTupleDispatcher } from "./domains/nestedTupleDispatcher/state";
import { NestedListDispatcher } from "./domains/nestedListDispatcher/state";
import { NestedMapDispatcher } from "./domains/nestedMapDispatcher/state";
// import { NestedUnionDispatcher } from "./domains/nestedUnionDispatcher/state";
import {
  DispatchParsedType,
  DispatchPrimitiveType,
  ListType,
  MapType,
  SumType,
  TableType,
  TupleType,
} from "../../../deserializer/domains/specification/domains/types/state";
import { DispatcherContext } from "../../../deserializer/state";
import {
  MapRepo,
  NestedLookupDispatcher,
  NestedMultiSelectionDispatcher,
  NestedSingleSelectionDispatcher,
  NestedTableDispatcher,
  ValueOrErrors,
} from "../../../../../../../main";
import { BaseRenderer } from "../../../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/state";
import { BaseTableRenderer } from "../../../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/domains/table/state";

export const NestedDispatcher = {
  Operations: {
    DispatchAsPrimitiveRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      type: DispatchPrimitiveType<T>,
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "basePrimitiveRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "basePrimitiveRenderer" but got ${renderer.kind}`,
          )
        : dispatcherContext
            .getConcreteRendererKind(renderer.concreteRendererName)
            .Then((viewKind) =>
              NestedPrimitiveDispatcher.Dispatch(
                type,
                viewKind,
                renderer,
                dispatcherContext,
              ),
            ),
    DispatchAsSingleSelectionRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "baseEnumRenderer" &&
      renderer.kind != "baseStreamRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "baseEnumRenderer" or "baseStreamRenderer" but got ${renderer.kind}`,
          )
        : dispatcherContext
            .getConcreteRendererKind(renderer.concreteRendererName)
            .Then((viewKind) =>
              viewKind != "enumSingleSelection" &&
              viewKind != "streamSingleSelection"
                ? ValueOrErrors.Default.throwOne(
                    `expected viewKind == "enumSingleSelection" or "streamSingleSelection" but got ${viewKind}`,
                  )
                : NestedSingleSelectionDispatcher.Operations.Dispatch(
                    viewKind,
                    renderer,
                    dispatcherContext,
                  ),
            ),
    DispatchAsMultiSelectionRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "baseEnumRenderer" &&
      renderer.kind != "baseStreamRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "baseEnumRenderer" or "baseStreamRenderer" but got ${renderer.kind}`,
          )
        : dispatcherContext
            .getConcreteRendererKind(renderer.concreteRendererName)
            .Then((viewKind) =>
              viewKind != "enumMultiSelection" &&
              viewKind != "streamMultiSelection"
                ? ValueOrErrors.Default.throwOne(
                    `expected viewKind == "enumMultiSelection" or "streamMultiSelection" but got ${viewKind}`,
                  )
                : NestedMultiSelectionDispatcher.Operations.Dispatch(
                    viewKind,
                    renderer,
                    dispatcherContext,
                  ),
            ),
    DispatchAsSumRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      type: SumType<T>,
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "baseSumRenderer" &&
      renderer.kind != "baseSumUnitDateRenderer"
        ? ValueOrErrors.Default.throwOne(
            `type is kind "sum" but renderer is not compatible, kind "${renderer.kind}"`,
          )
        : dispatcherContext
            .getConcreteRendererKind(renderer.concreteRendererName)
            .Then((viewKind) =>
              viewKind != "sum" && viewKind != "sumUnitDate"
                ? ValueOrErrors.Default.throwOne(
                    `expected viewKind == "sum" or "sumUnitDate" but got ${viewKind}`,
                  )
                : NestedSumDispatcher.Dispatch(
                    type,
                    renderer,
                    dispatcherContext,
                  ),
            ),
    DispatchAsTupleRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      type: TupleType<T>,
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "baseTupleRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind of "baseTupleRenderer" but got ${renderer.kind}`,
          )
        : dispatcherContext
            .getConcreteRendererKind(renderer.concreteRendererName)
            .Then((viewKind) =>
              viewKind != "tuple"
                ? ValueOrErrors.Default.throwOne(
                    `expected viewKind == "tuple" but got ${viewKind}`,
                  )
                : NestedTupleDispatcher.Dispatch(
                    type,
                    renderer,
                    dispatcherContext,
                  ),
            ),
    DispatchAsListRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      type: ListType<T>,
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "baseListRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "baseListRenderer" but got ${renderer.kind}`,
          )
        : dispatcherContext
            .getConcreteRendererKind(renderer.concreteRendererName)
            .Then((viewKind) =>
              viewKind != "list"
                ? ValueOrErrors.Default.throwOne(
                    `expected viewKind == "list" but got ${viewKind}`,
                  )
                : NestedListDispatcher.Dispatch(
                    type,
                    renderer,
                    dispatcherContext,
                  ),
            ),
    DispatchAsMapRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      type: MapType<T>,
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "baseMapRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "recordFieldMapRenderer" or "baseMapRenderer" but got ${renderer.kind}`,
          )
        : dispatcherContext
            .getConcreteRendererKind(renderer.concreteRendererName)
            .Then((viewKind) =>
              viewKind != "map"
                ? ValueOrErrors.Default.throwOne(
                    `expected viewKind == "map" but got ${viewKind}`,
                  )
                : NestedMapDispatcher.Dispatch(
                    type,
                    renderer,
                    dispatcherContext,
                  ),
            ),
    DispatchAsTableRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "baseTableRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "baseTableRenderer" but got ${renderer.kind}`,
          )
        : MapRepo.Operations.tryFindWithError(
            renderer.lookupRendererName,
            dispatcherContext.forms,
            () => `cannot find form ${renderer.lookupRendererName}`,
          ).Then((form) =>
            NestedTableDispatcher.Operations.Dispatch(
              form,
              renderer,
              dispatcherContext,
            ),
          ),
    DispatchAsLookupRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "baseLookupRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "baseLookupRenderer" but got ${renderer.kind}`,
          )
        : MapRepo.Operations.tryFindWithError(
            renderer.lookupRendererName,
            dispatcherContext.forms,
            () => `cannot find form ${renderer.lookupRendererName}`,
          ).Then((form) =>
            NestedLookupDispatcher.Operations.Dispatch(
              form,
              renderer,
              dispatcherContext,
            ),
          ),
    DispatchAs: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: DispatchParsedType<T>,
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
      as: string,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.Dispatch(
        type,
        renderer,
        dispatcherContext,
      ).MapErrors((errors) =>
        errors.map(
          (error) => `${error}\n...When dispatching base renderer as: ${as}`,
        ),
      ),
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: DispatchParsedType<T>,
      renderer: BaseRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> => {
      const result: ValueOrErrors<
        Template<any, any, any, any>,
        string
      > = type.kind == "primitive"
        ? NestedDispatcher.Operations.DispatchAsPrimitiveRenderer(
            type,
            renderer,
            dispatcherContext,
          )
        : type.kind == "singleSelection"
          ? NestedDispatcher.Operations.DispatchAsSingleSelectionRenderer(
              renderer,
              dispatcherContext,
            )
          : type.kind == "multiSelection"
            ? NestedDispatcher.Operations.DispatchAsMultiSelectionRenderer(
                renderer,
                dispatcherContext,
              )
            : type.kind == "sum"
              ? NestedDispatcher.Operations.DispatchAsSumRenderer(
                  type,
                  renderer,
                  dispatcherContext,
                )
              : type.kind == "tuple"
                ? NestedDispatcher.Operations.DispatchAsTupleRenderer(
                    type,
                    renderer,
                    dispatcherContext,
                  )
                : type.kind == "list"
                  ? NestedDispatcher.Operations.DispatchAsListRenderer(
                      type,
                      renderer,
                      dispatcherContext,
                    )
                  : type.kind == "map"
                    ? NestedDispatcher.Operations.DispatchAsMapRenderer(
                        type,
                        renderer,
                        dispatcherContext,
                      )
                    : type.kind == "lookup"
                      ? NestedDispatcher.Operations.DispatchAsLookupRenderer(
                          renderer,
                          dispatcherContext,
                        )
                      : type.kind == "table"
                        ? NestedDispatcher.Operations.DispatchAsTableRenderer(
                            renderer,
                            dispatcherContext,
                          )
                        : ValueOrErrors.Default.throwOne(
                            `unknown type kind "${type.kind}"`,
                          );

      return result.MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When dispatching base renderer: ${
              renderer.kind == "baseLookupRenderer" ||
              renderer.kind == "baseTableRenderer"
                ? renderer.lookupRendererName
                : renderer.concreteRendererName
            }`,
        ),
      );
    },
    // if (viewKind == "union") {
    //   return NestedUnionDispatcher.Dispatch(
    //     type,
    //     viewKind,
    //     renderer,
    //     rendererName,
    //     dispatcherContext,
    //   );
    // }

    // TODO tables

    // const result: ValueOrErrors<
    //   Template<any, any, any, any>,
    //   string
    // > = (() => {
    //   if (
    //     renderer.kind == "lookupRecordField" ||
    //     renderer.kind == "nestedLookupRenderer"
    //   ) {
    //     return NestedDispatcher.Operations.DispatchByViewKind(
    //       rendererName,
    //       renderer,
    //       "lookup",
    //       type,
    //       dispatcherContext,
    //     ).Then((form) =>
    //       ValueOrErrors.Default.return(
    //         // TODO - optional override
    //         form.withView(dispatcherContext.nestedContainerFormView),
    //       ),
    //     );
    //   }

    //   return dispatcherContext
    //     .getViewKind(renderer.concreteRendererName)
    //     .Then((viewKind) => {
    //       return NestedDispatcher.Operations.DispatchByViewKind(
    //         rendererName,
    //         renderer,
    //         viewKind,
    //         type,
    //         dispatcherContext,
    //       ).Then((form) =>
    //         ValueOrErrors.Default.return(
    //           form.withView(
    //             dispatcherContext.fieldViews[viewKind][
    //               renderer.concreteRendererName
    //             ],
    //           ),
    //         ),
    //       );
    //     });
    // })();
    // return result.MapErrors((errors) =>
    //   errors.map(
    //     (error) =>
    //       `${error}\n...When dispatching nested renderer: ${rendererName}`,
    //   ),
    // );
  },
};
