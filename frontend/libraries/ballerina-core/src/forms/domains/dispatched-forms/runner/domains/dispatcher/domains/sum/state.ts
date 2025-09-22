import {
  DispatchInjectablesTypes,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";

import { SumAbstractRenderer } from "../../../abstract-renderers/sum/template";
import { SumRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/sum/state";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { SumUnitDateRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/sumUnitDate/state";
import { DispatcherContextWithApiSources } from "../../../../coroutines/runner";

export const SumDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: SumRenderer<T> | SumUnitDateRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
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
            return renderer.kind == "sumUnitDateRenderer"
              ? dispatcherContext
                  .getConcreteRenderer("sumUnitDate", renderer.concreteRenderer)
                  .Then((concreteRenderer) =>
                    ValueOrErrors.Default.return(
                      SumAbstractRenderer(
                        dispatcherContext.IdProvider,
                        dispatcherContext.ErrorRenderer,
                        leftForm,
                        rightForm,
                      ).withView(concreteRenderer),
                    ),
                  )
              : dispatcherContext
                  .getConcreteRenderer("sum", renderer.concreteRenderer)
                  .Then((concreteRenderer) =>
                    ValueOrErrors.Default.return(
                      SumAbstractRenderer(
                        dispatcherContext.IdProvider,
                        dispatcherContext.ErrorRenderer,
                        leftForm,
                        rightForm,
                      ).withView(concreteRenderer),
                    ),
                  );
          }),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested sum`),
        ),
  },
};
