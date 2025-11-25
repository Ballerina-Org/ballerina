import {
  DispatchInjectablesTypes,
  ListAbstractRenderer,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { ListRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/list/state";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { DispatcherContextWithApiSources } from "../../../../state";

//TODO check type
export const ListDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: ListRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      isInlined: boolean,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.DispatchAs(
        renderer.elementRenderer,
        dispatcherContext,
        "listElement",
        isInlined,
      )
        .Then((elementTemplate) =>
          dispatcherContext
            .defaultState(
              renderer.type.args[0],
              renderer.elementRenderer.renderer,
            )
            .Then((defaultElementState) =>
              dispatcherContext
                .defaultValue(
                  renderer.type.args[0],
                  renderer.elementRenderer.renderer,
                )
                .Then((defaultElementValue) =>
                  dispatcherContext
                    .getConcreteRenderer("list", renderer.concreteRenderer)
                    .Then((concreteRenderer) =>
                      ValueOrErrors.Default.return(
                        ListAbstractRenderer(
                          () => defaultElementState,
                          () => defaultElementValue,
                          elementTemplate,
                          renderer.elementRenderer,
                          renderer.methods ?? [],
                          dispatcherContext.IdProvider,
                          dispatcherContext.ErrorRenderer,
                        )
                          .mapContext((_: any) => ({
                            ..._,
                            type: renderer.type,
                          }))
                          .withView(concreteRenderer),
                      ),
                    ),
                ),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested list`),
        ),
  },
};
