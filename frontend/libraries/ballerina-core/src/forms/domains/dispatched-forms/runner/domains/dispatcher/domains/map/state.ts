import { ValueOrErrors } from "../../../../../../../../collections/domains/valueOrErrors/state";
import {
  DispatchInjectablesTypes,
  MapAbstractRenderer,
  Template,
} from "../../../../../../../../../main";
import { MapRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/map/state";
import {
  MapType,
  StringSerializedType,
} from "../../../../../deserializer/domains/specification/domains/types/state";
import { DispatcherContext } from "../../../../../deserializer/state";
import { NestedDispatcher } from "../nestedDispatcher/state";

export const MapDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: MapRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts
      >,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      NestedDispatcher.Operations.DispatchAs(
        renderer.keyRenderer,
        dispatcherContext,
        "key",
        "key",
      )
        .Then((keyTemplate) =>
          dispatcherContext
            .defaultState(renderer.type.args[0], renderer.keyRenderer.renderer)
            .Then((defaultKeyState) =>
              dispatcherContext
                .defaultValue(
                  renderer.type.args[0],
                  renderer.keyRenderer.renderer,
                )
                .Then((defaultKeyValue) =>
                  NestedDispatcher.Operations.DispatchAs(
                    renderer.valueRenderer,
                    dispatcherContext,
                    "value",
                    "value",
                  ).Then((valueTemplate) =>
                    dispatcherContext
                      .defaultState(
                        renderer.type.args[1],
                        renderer.valueRenderer.renderer,
                      )
                      .Then((defaultValueState) =>
                        dispatcherContext
                          .defaultValue(
                            renderer.type.args[1],
                            renderer.valueRenderer.renderer,
                          )
                          .Then((defaultValueValue) =>
                            renderer.renderer.kind != "lookupRenderer"
                              ? ValueOrErrors.Default.throwOne<
                                  [
                                    Template<any, any, any, any>,
                                    StringSerializedType,
                                  ],
                                  string
                                >(
                                  `received non lookup renderer kind "${renderer.renderer.kind}" when resolving defaultState for map`,
                                )
                              : renderer.renderer.renderer.kind !=
                                  "concreteLookup"
                                ? ValueOrErrors.Default.throwOne<
                                    [
                                      Template<any, any, any, any>,
                                      StringSerializedType,
                                    ],
                                    string
                                  >(
                                    `received non concrete lookup renderer kind "${renderer.renderer.renderer.kind}" when resolving defaultState for list`,
                                  )
                                : dispatcherContext
                                    .getConcreteRenderer(
                                      "map",
                                      renderer.renderer.renderer.renderer,
                                    )
                                    .Then((concreteRenderer) =>
                                      ValueOrErrors.Default.return<
                                        [
                                          Template<any, any, any, any>,
                                          StringSerializedType,
                                        ],
                                        string
                                      >([
                                        MapAbstractRenderer(
                                          () => defaultKeyState,
                                          () => defaultKeyValue,
                                          () => defaultValueState,
                                          () => defaultValueValue,
                                          keyTemplate[0],
                                          valueTemplate[0],
                                          dispatcherContext.IdProvider,
                                          dispatcherContext.ErrorRenderer,
                                        ).withView(concreteRenderer),
                                        MapType.SerializeToString([
                                          keyTemplate[1],
                                          valueTemplate[1],
                                        ]),
                                      ]),
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
