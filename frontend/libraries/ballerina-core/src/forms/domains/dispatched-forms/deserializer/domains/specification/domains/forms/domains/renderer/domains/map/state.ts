import { Map } from "immutable";
import {
  ConcreteRendererKinds,
  DispatchParsedType,
  isObject,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { MapType } from "../../../../../types/state";
import { NestedRenderer } from "../nestedRenderer/state";
import { Renderer } from "../../state";

export type SerializedMapRenderer = {
  renderer: unknown;
  keyRenderer: unknown;
  valueRenderer: unknown;
};

export type MapRenderer<T> = {
  kind: "mapRenderer";
  renderer: Renderer<T>;
  keyRenderer: NestedRenderer<T>;
  valueRenderer: NestedRenderer<T>;
  type: MapType<T>;
};

export const MapRenderer = {
  Default: <T>(
    type: MapType<T>,
    renderer: Renderer<T>,
    keyRenderer: NestedRenderer<T>,
    valueRenderer: NestedRenderer<T>,
  ): MapRenderer<T> => ({
    kind: "mapRenderer",
    type,
    renderer,
    keyRenderer,
    valueRenderer,
  }),
  Operations: {
    tryAsValidMapBaseRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedMapRenderer, string> =>
      !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(
            `serialized map renderer is not an object`,
          )
        : !("renderer" in serialized)
          ? ValueOrErrors.Default.throwOne(`renderer is missing`)
          : !("keyRenderer" in serialized)
            ? ValueOrErrors.Default.throwOne(`keyRenderer is missing`)
            : !("valueRenderer" in serialized)
              ? ValueOrErrors.Default.throwOne(`valueRenderer is missing`)
              : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: MapType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<MapRenderer<T>, string> =>
      MapRenderer.Operations.tryAsValidMapBaseRenderer(serialized)
        .Then((renderer) =>
          NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            renderer.keyRenderer,
            concreteRenderers,
            "Map key",
            types,
          ).Then((deserializedKeyRenderer) =>
            NestedRenderer.Operations.DeserializeAs(
              type.args[1],
              renderer.valueRenderer,
              concreteRenderers,
              "Map value",
              types,
            ).Then((deserializedValueRenderer) =>
              Renderer.Operations.Deserialize(
                type,
                renderer.renderer,
                concreteRenderers,
                types,
              ).Then((deserializedRenderer) =>
                ValueOrErrors.Default.return(
                  MapRenderer.Default(
                    type,
                    deserializedRenderer,
                    deserializedKeyRenderer,
                    deserializedValueRenderer,
                  ),
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Map`),
        ),
  },
};
