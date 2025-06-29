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
      ExtraContext,
    >(
      renderer: SumRenderer<T> | SumUnitDateRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      (renderer.kind == "sumRenderer"
        ? NestedDispatcher.Operations.DispatchAs(
            renderer.leftRenderer,
            dispatcherContext,
            "left",
            isInlined,
            tableApi,
          )
        : ValueOrErrors.Default.return<undefined, string>(undefined)
      )
        .Then((leftForm) =>
          (renderer.kind == "sumRenderer"
            ? NestedDispatcher.Operations.DispatchAs(
                renderer.rightRenderer,
                dispatcherContext,
                "right",
                isInlined,
                tableApi,
              )
            : ValueOrErrors.Default.return<undefined, string>(undefined)
          ).Then((rightForm) => {
            const serializedType = SumType.SerializeToString([
              leftForm?.[1] ??
                DispatchParsedType.Operations.SerializeToString(
                  renderer.type.args[0],
                ),
              rightForm?.[1] ??
                DispatchParsedType.Operations.SerializeToString(
                  renderer.type.args[1],
                ),
            ]);
            return renderer.kind == "sumUnitDateRenderer"
              ? dispatcherContext
                  .getConcreteRenderer("sumUnitDate", renderer.concreteRenderer)
                  .Then((concreteRenderer) =>
                    ValueOrErrors.Default.return<
                      [Template<any, any, any, any>, StringSerializedType],
                      string
                    >([
                      SumAbstractRenderer(
                        dispatcherContext.IdProvider,
                        dispatcherContext.ErrorRenderer,
                        serializedType,
                        leftForm?.[0],
                        rightForm?.[0],
                      ).withView(concreteRenderer),
                      serializedType,
                    ]),
                  )
              : dispatcherContext
                  .getConcreteRenderer("sum", renderer.concreteRenderer)
                  .Then((concreteRenderer) =>
                    ValueOrErrors.Default.return<
                      [Template<any, any, any, any>, StringSerializedType],
                      string
                    >([
                      SumAbstractRenderer(
                        dispatcherContext.IdProvider,
                        dispatcherContext.ErrorRenderer,
                        serializedType,
                        leftForm?.[0],
                        rightForm?.[0],
                      ).withView(concreteRenderer),
                      serializedType,
                    ]),
                  );
          }),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested sum`),
        ),
  },
};
