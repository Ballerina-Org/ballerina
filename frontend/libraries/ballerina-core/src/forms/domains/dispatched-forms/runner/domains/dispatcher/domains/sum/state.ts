import {
  DispatchInjectablesTypes,
  PredicateValue,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";

import { SumAbstractRenderer } from "../../../abstract-renderers/sum/template";
import { SumRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/sum/state";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { SumUnitDateRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/sumUnitDate/state";
import { DispatcherContextWithApiSources } from "../../../../state";

export const SumDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: SumRenderer<T> | SumUnitDateRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      isInlined: boolean,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      (renderer.kind == "sumRenderer"
        ? NestedDispatcher.Operations.DispatchAs(
            renderer.leftRenderer,
            dispatcherContext,
            "left",
            isInlined,
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
                        ValueOrErrors.Default.return(
                          PredicateValue.Default.unit(),
                        ),
                        ValueOrErrors.Default.return(
                          PredicateValue.Default.date(),
                        ),
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
                        dispatcherContext.defaultValue(
                          renderer.type.args[0],
                          renderer.leftRenderer.renderer,
                        ),
                        dispatcherContext.defaultValue(
                          renderer.type.args[1],
                          renderer.rightRenderer.renderer,
                        ),
                        leftForm,
                        rightForm,
                        renderer.leftRenderer,
                        renderer.rightRenderer,
                      )
                        .mapContext((_: any) => ({
                          ..._,
                          type: renderer.type,
                        }))
                        .withView(concreteRenderer),
                    ),
                  );
          }),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested sum`),
        ),
  },
};
