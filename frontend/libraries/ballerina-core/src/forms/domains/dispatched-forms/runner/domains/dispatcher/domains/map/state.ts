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
      ExtraContext,
    >(
      renderer: MapRenderer<T>,
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
      NestedDispatcher.Operations.DispatchAs(
        renderer.keyRenderer,
        dispatcherContext,
        "key",
        isInlined,
        tableApi,
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
                    isInlined,
                    tableApi,
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
                            dispatcherContext
                              .getConcreteRenderer(
                                "map",
                                renderer.concreteRenderer,
                              )
                              .Then((concreteRenderer) => {
                                const serializedType =
                                  MapType.SerializeToString([
                                    keyTemplate[1],
                                    valueTemplate[1],
                                  ]);
                                return ValueOrErrors.Default.return<
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
                                    serializedType,
                                  ).withView(concreteRenderer),
                                  serializedType,
                                ]);
                              }),
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
