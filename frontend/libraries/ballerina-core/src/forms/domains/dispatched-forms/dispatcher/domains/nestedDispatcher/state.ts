import { Template } from "../../../../../../template/state";
import { NestedPrimitiveDispatcher } from "./domains/nestedPrimitiveDispatcher/state";
import { NestedSumDispatcher } from "./domains/nestedSumDispatcher/state";
import { NestedTupleDispatcher } from "./domains/nestedTupleDispatcher/state";
import { NestedListDispatcher } from "./domains/nestedListDispatcher/state";
import { NestedMapDispatcher } from "./domains/nestedMapDispatcher/state";
// import { NestedUnionDispatcher } from "./domains/nestedUnionDispatcher/state";
import { NestedRenderer } from "../../../deserializer/domains/specification/domains/form/domains/renderers/domains/nestedRenderer/state";
import { RecordFieldRenderer } from "../../../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/domains/recordFieldRenderer/state";
import {
  DispatchParsedType,
  DispatchPrimitiveType,
  ListType,
  MapType,
  SumType,
  TupleType,
} from "../../../deserializer/domains/specification/domains/types/state";
import { DispatcherContext } from "../../../deserializer/state";
import {
  NestedLookupDispatcher,
  NestedMultiSelectionDispatcher,
  NestedSingleSelectionDispatcher,
  ValueOrErrors,
} from "../../../../../../../main";

export const NestedDispatcher = {
  Operations: {
    DispatchAsPrimitiveRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      type: DispatchPrimitiveType<T>,
      renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "recordFieldPrimitiveRenderer" &&
      renderer.kind != "nestedPrimitiveRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "recordFieldPrimitiveRenderer" or "nestedPrimitiveRenderer" but got ${renderer.kind}`,
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
      renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "nestedEnumRenderer" &&
      renderer.kind != "nestedStreamRenderer" &&
      renderer.kind != "recordFieldEnumRenderer" &&
      renderer.kind != "recordFieldStreamRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "nestedEnumRenderer" or "nestedStreamRenderer" or "recordFieldEnumRenderer" or "recordFieldStreamRenderer" but got ${renderer.kind}`,
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
      renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "nestedEnumRenderer" &&
      renderer.kind != "nestedStreamRenderer" &&
      renderer.kind != "recordFieldEnumRenderer" &&
      renderer.kind != "recordFieldStreamRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "nestedEnumRenderer" or "nestedStreamRenderer" or "recordFieldEnumRenderer" or "recordFieldStreamRenderer" but got ${renderer.kind}`,
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
      renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "recordFieldSumRenderer" &&
      renderer.kind != "nestedSumRenderer" &&
      renderer.kind != "recordFieldSumUnitDateRenderer" &&
      renderer.kind != "nestedSumUnitDateRenderer"
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
      renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "recordFieldTupleRenderer" &&
      renderer.kind != "nestedTupleRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind of "tupleRecordFieldRenderer" or "nestedTupleRenderer" but got ${renderer.kind}`,
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
      renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "recordFieldListRenderer" &&
      renderer.kind != "nestedListRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "recordFieldListRenderer" or "nestedListRenderer" but got ${renderer.kind}`,
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
      renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.kind != "recordFieldMapRenderer" &&
      renderer.kind != "nestedMapRenderer"
        ? ValueOrErrors.Default.throwOne(
            `expected renderer.kind == "recordFieldMapRenderer" or "nestedMapRenderer" but got ${renderer.kind}`,
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
    // DispatchAsUnionRenderer: <
    //   T extends { [key in keyof T]: { type: any; state: any } },
    // >(
    //   type: UnionType<T>,
    //   viewKind: string,
    //   renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
    //   rendererName: string,
    //   dispatcherContext: DispatcherContext<T>,
    // ): ValueOrErrors<Template<any, any, any, any>, string> => {
    //   if (viewKind != "union") {
    //     return ValueOrErrors.Default.throwOne(
    //       `expected viewKind == "union" but got ${viewKind}`,
    //     );
    //   }
    //   if (type.kind != "union") {
    //     return ValueOrErrors.Default.throwOne(
    //       `expected type.kind == "union" but got ${type.kind}`,
    //     );
    //   }
    //   return NestedUnionDispatcher.Dispatch(
    //     type,
    //     viewKind,
    //     renderer,
    //     rendererName,
    //     dispatcherContext,
    //   );
    // },

    DispatchAsLookupRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> => {
      // TODO -- attach views (inc optional override)
      if (
        renderer.kind != "recordFieldLookupRenderer" &&
        renderer.kind != "nestedLookupRenderer"
      ) {
        return ValueOrErrors.Default.throwOne(
          `expected renderer.kind == "recordFieldLookupRenderer" or "nestedLookupRenderer" but got ${renderer.kind}`,
        );
      }
      const form = dispatcherContext.forms.get(renderer.lookupRendererName);
      if (form == undefined) {
        return ValueOrErrors.Default.throwOne(
          `cannot find form ${renderer.lookupRendererName}`,
        );
      }
      return NestedLookupDispatcher.Operations.Dispatch(
        form,
        renderer,
        dispatcherContext,
      );
    },
    DispatchAs: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: DispatchParsedType<T>,
      renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
      as: string,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.Dispatch(
        type,
        renderer,
        dispatcherContext,
      ).MapErrors((errors) =>
        errors.map(
          (error) => `${error}\n...When dispatching nested renderer as: ${as}`,
        ),
      ),
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: DispatchParsedType<T>,
      renderer: RecordFieldRenderer<T> | NestedRenderer<T>,
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
                      : ValueOrErrors.Default.throwOne(
                          `unknown type kind: ${type.kind}`,
                        );

      return result.MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When dispatching nested renderer: ${
              renderer.kind == "recordFieldLookupRenderer" ||
              renderer.kind == "nestedLookupRenderer"
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
