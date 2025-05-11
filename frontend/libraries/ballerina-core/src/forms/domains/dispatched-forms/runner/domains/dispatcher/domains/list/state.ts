import { ListType } from "../../../../../deserializer/domains/specification/domains/types/state";
import { DispatcherContext } from "../../../../../deserializer/state";
import {
  ListAbstractRenderer,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { ListRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/list/state";
import { NestedDispatcher } from "../nestedDispatcher/state";

//TODO check type
export const ListDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: ListType<T>,
      renderer: ListRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.DispatchAs(
        renderer.elementRenderer,
        dispatcherContext,
        "listElement",
        "listElement",
      )
        .Then((elementTemplate) =>
          dispatcherContext
            .defaultState(type.args[0], renderer.elementRenderer.renderer)
            .Then((defaultElementState) =>
              dispatcherContext
                .defaultValue(type.args[0], renderer.elementRenderer.renderer)
                .Then((defaultElementValue) =>
                  renderer.renderer.kind != "lookupRenderer"
                    ? ValueOrErrors.Default.throwOne<
                        Template<any, any, any, any>,
                        string
                      >(
                        `received non lookup renderer kind "${renderer.renderer.kind}" when resolving defaultState for list`,
                      )
                    : dispatcherContext
                        .getConcreteRenderer("list", renderer.renderer.renderer)
                        .Then((concreteRenderer) =>
                          ValueOrErrors.Default.return(
                            ListAbstractRenderer(
                              () => defaultElementState,
                              () => defaultElementValue,
                              elementTemplate,
                            ).withView(concreteRenderer),
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
