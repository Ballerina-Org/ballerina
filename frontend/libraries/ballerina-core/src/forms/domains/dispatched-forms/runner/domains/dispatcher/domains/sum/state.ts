import {
  DispatcherContext,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";

import { SumAbstractRenderer } from "../../../abstract-renderers/sum/template";
import { SumRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/sum/state";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { SumUnitDateRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/sumUnitDate/state";

export const SumDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      renderer: SumRenderer<T> | SumUnitDateRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
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
            renderer.renderer.kind == "lookupRenderer"
              ? dispatcherContext
                  .getConcreteRenderer(
                    "sumUnitDate",
                    renderer.renderer.renderer,
                  )
                  .Then((concreteRenderer) =>
                    ValueOrErrors.Default.return(
                      SumAbstractRenderer(
                        dispatcherContext.IdWrapper,
                        dispatcherContext.ErrorRenderer,
                        leftForm,
                        rightForm,
                      ).withView(concreteRenderer),
                    ),
                  )
              : renderer.renderer.kind == "lookupRenderer"
                ? dispatcherContext
                    .getConcreteRenderer("sum", renderer.renderer.renderer)
                    .Then((concreteRenderer) =>
                      ValueOrErrors.Default.return(
                        SumAbstractRenderer(
                          dispatcherContext.IdWrapper,
                          dispatcherContext.ErrorRenderer,
                          leftForm,
                          rightForm,
                        ).withView(concreteRenderer),
                      ),
                    )
                : ValueOrErrors.Default.throwOne<
                    Template<any, any, any, any>,
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
