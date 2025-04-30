import {
  DispatcherContext,
  MapAbstractRenderer,
  NestedDispatcher,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { MapType } from "../../../../../deserializer/domains/specification/domains/types/state";
import { BaseMapRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/domains/map/state";

export const NestedMapDispatcher = {
  Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
    type: MapType<T>,
    mapRenderer: BaseMapRenderer<T>,
    dispatcherContext: DispatcherContext<T>,
  ): ValueOrErrors<Template<any, any, any, any>, string> =>
    NestedDispatcher.Operations.DispatchAs(
      type.args[0],
      mapRenderer.keyRenderer,
      dispatcherContext,
      "key",
    )
      .Then((keyTemplate) =>
        dispatcherContext
          .defaultState(type.args[0], mapRenderer.keyRenderer)
          .Then((defaultKeyState) =>
            dispatcherContext
              .defaultValue(type.args[0], mapRenderer.keyRenderer)
              .Then((defaultKeyValue) =>
                NestedDispatcher.Operations.DispatchAs(
                  type.args[1],
                  mapRenderer.valueRenderer,
                  dispatcherContext,
                  "value",
                ).Then((valueTemplate) =>
                  dispatcherContext
                    .defaultState(type.args[1], mapRenderer.valueRenderer)
                    .Then((defaultValueState) =>
                      dispatcherContext
                        .defaultValue(type.args[1], mapRenderer.valueRenderer)
                        .Then((defaultValueValue) =>
                          dispatcherContext
                            .getConcreteRenderer(
                              "map",
                              mapRenderer.concreteRendererName,
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
                                )
                                  .mapContext((_: any) => ({
                                    ..._,
                                    type: mapRenderer.type,
                                    label: mapRenderer.label,
                                    tooltip: mapRenderer.tooltip,
                                    details: mapRenderer.details,
                                  }))
                                  .withView(concreteRenderer),
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
};
