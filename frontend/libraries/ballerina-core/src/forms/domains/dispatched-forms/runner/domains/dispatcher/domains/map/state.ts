import { ValueOrErrors } from "../../../../../../../../collections/domains/valueOrErrors/state";
import {
  DispatchInjectablesTypes,
  MapAbstractRenderer,
  Template,
} from "../../../../../../../../../main";
import { MapRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/map/state";
import { DispatcherContextWithApiSources } from "../../../../coroutines/runner";
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
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
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
