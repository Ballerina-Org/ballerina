import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  isString,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { MapType } from "../../../../../types/state";
import { NestedRenderer } from "../nestedRenderer/state";
import { Renderer } from "../../state";

export type SerializedMapRenderer = {
  renderer: string;
  keyRenderer: unknown;
  valueRenderer: unknown;
};

export type MapRenderer<T> = {
  kind: "mapRenderer";
  concreteRenderer: string;
  keyRenderer: NestedRenderer<T>;
  valueRenderer: NestedRenderer<T>;
  type: MapType<T>;
};

export const MapRenderer = {
  Default: <T>(
    type: MapType<T>,
    concreteRenderer: string,
    keyRenderer: NestedRenderer<T>,
    valueRenderer: NestedRenderer<T>,
  ): MapRenderer<T> => ({
    kind: "mapRenderer",
    type,
    concreteRenderer,
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
          : !isString(serialized.renderer)
            ? ValueOrErrors.Default.throwOne(`renderer must be a string`)
            : !("keyRenderer" in serialized)
              ? ValueOrErrors.Default.throwOne(`keyRenderer is missing`)
              : !("valueRenderer" in serialized)
                ? ValueOrErrors.Default.throwOne(`valueRenderer is missing`)
                : ValueOrErrors.Default.return({
                    renderer: serialized.renderer,
                    keyRenderer: serialized.keyRenderer,
                    valueRenderer: serialized.valueRenderer,
                  }),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: MapType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
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
              ValueOrErrors.Default.return(
                MapRenderer.Default(
                  type,
                  renderer.renderer,
                  deserializedKeyRenderer,
                  deserializedValueRenderer,
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
