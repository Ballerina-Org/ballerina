import {
  DispatcherContext,
  DispatchInjectablesTypes,
  Template,
  ValueOrErrors,
  StringSerializedType,
  SumType,
  DispatchParsedType,
} from "../../../../../../../../../main";

import { SumAbstractRenderer } from "../../../abstract-renderers/sum/template";
import { SumRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/sum/state";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { SumUnitDateRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/sumUnitDate/state";

export const SumDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: SumRenderer<T> | SumUnitDateRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts
      >,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      (renderer.kind == "sumRenderer"
        ? NestedDispatcher.Operations.DispatchAs(
            renderer.leftRenderer,
            dispatcherContext,
            "left",
            "left",
          )
        : ValueOrErrors.Default.return<undefined, string>(undefined)
      )
        .Then((leftForm) =>
          (renderer.kind == "sumRenderer"
            ? NestedDispatcher.Operations.DispatchAs(
                renderer.rightRenderer,
                dispatcherContext,
                "right",
                "right",
              )
            : ValueOrErrors.Default.return<undefined, string>(undefined)
          ).Then((rightForm) =>
            renderer.kind == "sumUnitDateRenderer" &&
            renderer.renderer.kind == "lookupRenderer" &&
            renderer.renderer.renderer.kind == "concreteLookup"
              ? dispatcherContext
                  .getConcreteRenderer(
                    "sumUnitDate",
                    renderer.renderer.renderer.renderer,
                  )
                  .Then((concreteRenderer) =>
                    ValueOrErrors.Default.return<
                      [Template<any, any, any, any>, StringSerializedType],
                      string
                    >([
                      SumAbstractRenderer(
                        dispatcherContext.IdProvider,
                        dispatcherContext.ErrorRenderer,
                        leftForm?.[0],
                        rightForm?.[0],
                      ).withView(concreteRenderer),
                      SumType.SerializeToString([
                        leftForm?.[1] ??
                          DispatchParsedType.Operations.SerializeToString(
                            renderer.type.args[0],
                          ),
                        rightForm?.[1] ??
                          DispatchParsedType.Operations.SerializeToString(
                            renderer.type.args[1],
                          ),
                      ]),
                    ]),
                  )
              : renderer.renderer.kind == "lookupRenderer" &&
                renderer.renderer.renderer.kind == "concreteLookup"
                ? dispatcherContext
                    .getConcreteRenderer(
                      "sum",
                      renderer.renderer.renderer.renderer,
                    )
                    .Then((concreteRenderer) =>
                      ValueOrErrors.Default.return<
                        [Template<any, any, any, any>, StringSerializedType],
                        string
                      >([
                        SumAbstractRenderer(
                          dispatcherContext.IdProvider,
                          dispatcherContext.ErrorRenderer,
                          leftForm?.[0],
                          rightForm?.[0],
                        ).withView(concreteRenderer),
                        SumType.SerializeToString([
                          leftForm?.[1] ??
                            DispatchParsedType.Operations.SerializeToString(
                              renderer.type.args[0],
                            ),
                          rightForm?.[1] ??
                            DispatchParsedType.Operations.SerializeToString(
                              renderer.type.args[1],
                            ),
                        ]),
                      ]),
                    )
                : ValueOrErrors.Default.throwOne<
                    [Template<any, any, any, any>, StringSerializedType],
                    string
                  >(
                    `received non lookup renderer kind for sum concrete renderer`,
                  ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested sum`),
        ),
  },
};
