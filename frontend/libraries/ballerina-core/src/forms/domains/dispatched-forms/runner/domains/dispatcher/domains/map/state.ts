import { ValueOrErrors } from "../../../../../../../../collections/domains/valueOrErrors/state";
import { MapAbstractRenderer, Template } from "../../../../../../../../../main";
import { MapRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/map/state";
import { MapType } from "../../../../../deserializer/domains/specification/domains/types/state";
import { DispatcherContext } from "../../../../../deserializer/state";
import { NestedDispatcher } from "../nestedDispatcher/state";

export const MapDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: MapType<T>,
      renderer: MapRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.DispatchAs(
        renderer.keyRenderer,
        dispatcherContext,
        "key",
        "key",
      )
        .Then((keyTemplate) =>
          dispatcherContext
            .defaultState(type.args[0], renderer.keyRenderer.renderer)
            .Then((defaultKeyState) =>
              dispatcherContext
                .defaultValue(type.args[0], renderer.keyRenderer.renderer)
                .Then((defaultKeyValue) =>
                  NestedDispatcher.Operations.DispatchAs(
                    renderer.valueRenderer,
                    dispatcherContext,
                    "value",
                    "value",
                  ).Then((valueTemplate) =>
                    dispatcherContext
                      .defaultState(
                        type.args[1],
                        renderer.valueRenderer.renderer,
                      )
                      .Then((defaultValueState) =>
                        dispatcherContext
                          .defaultValue(
                            type.args[1],
                            renderer.valueRenderer.renderer,
                          )
                          .Then((defaultValueValue) =>
                            renderer.renderer.kind != "lookupRenderer"
                              ? ValueOrErrors.Default.throwOne<
                                  Template<any, any, any, any>,
                                  string
                                >(
                                  `received non lookup renderer kind "${renderer.renderer.kind}" when resolving defaultState for map`,
                                )
                              : dispatcherContext
                                  .getConcreteRenderer(
                                    "map",
                                    renderer.renderer.renderer,
                                  )
                                  .Then((concreteRenderer) =>
                                    ValueOrErrors.Default.return(
                                      MapAbstractRenderer(
                                        () => defaultKeyState,
                                        () => defaultKeyValue,
                                        () => defaultValueState,
                                        () => defaultValueValue,
                                        keyTemplate,
                                        valueTemplate,
                                        dispatcherContext.IdProvider,
                                        dispatcherContext.ErrorRenderer,
                                      ).withView(concreteRenderer),
                                    ),
                                  ),
                          ),
                      ),
                  ),
                ),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested map`),
        ),
  },
};
