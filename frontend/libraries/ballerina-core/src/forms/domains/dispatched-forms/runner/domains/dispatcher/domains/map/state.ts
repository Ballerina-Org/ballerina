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
      mapRenderer: MapRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.DispatchAs(
        mapRenderer.keyRenderer,
        dispatcherContext,
        "key",
      )
        .Then((keyTemplate) =>
          dispatcherContext
            .defaultState(type.args[0], mapRenderer.keyRenderer.renderer)
            .Then((defaultKeyState) =>
              dispatcherContext
                .defaultValue(type.args[0], mapRenderer.keyRenderer.renderer)
                .Then((defaultKeyValue) =>
                  NestedDispatcher.Operations.DispatchAs(
                    mapRenderer.valueRenderer,
                    dispatcherContext,
                    "value",
                  ).Then((valueTemplate) =>
                    dispatcherContext
                      .defaultState(
                        type.args[1],
                        mapRenderer.valueRenderer.renderer,
                      )
                      .Then((defaultValueState) =>
                        dispatcherContext
                          .defaultValue(
                            type.args[1],
                            mapRenderer.valueRenderer.renderer,
                          )
                          .Then((defaultValueValue) =>
                            mapRenderer.renderer.kind != "lookupRenderer"
                              ? ValueOrErrors.Default.throwOne<
                                  Template<any, any, any, any>,
                                  string
                                >(
                                  `received non lookup renderer kind "${mapRenderer.renderer.kind}" when resolving defaultState for map`,
                                )
                              : dispatcherContext
                                  .getConcreteRenderer(
                                    "map",
                                    mapRenderer.renderer.name,
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
