import { Map } from "immutable";
import { Renderer } from "../../state";
import { NestedRenderer } from "../nestedRenderer/state";
import {
  ConcreteRendererKinds,
  DispatchParsedType,
  isObject,
  SumType,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";

export type SerializedSumRenderer = {
  renderer: unknown;
  leftRenderer: unknown;
  rightRenderer: unknown;
};

export type SumRenderer<T> = {
  kind: "sumRenderer";
  renderer: Renderer<T>;
  leftRenderer: NestedRenderer<T>;
  rightRenderer: NestedRenderer<T>;
  type: SumType<T>;
};

export const SumRenderer = {
  Default: <T>(
    type: SumType<T>,
    renderer: Renderer<T>,
    leftRenderer: NestedRenderer<T>,
    rightRenderer: NestedRenderer<T>,
  ): SumRenderer<T> => ({
    kind: "sumRenderer",
    type,
    renderer,
    leftRenderer,
    rightRenderer,
  }),
  Operations: {
    hasRenderers: (
      serialized: unknown,
    ): serialized is SerializedSumRenderer & {
      renderer: unknown;
      leftRenderer: unknown;
      rightRenderer: unknown;
    } =>
      isObject(serialized) &&
      "renderer" in serialized &&
      "leftRenderer" in serialized &&
      "rightRenderer" in serialized,
    tryAsValidSumBaseRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedSumRenderer, string> =>
      !SumRenderer.Operations.hasRenderers(serialized)
        ? ValueOrErrors.Default.throwOne(
            `renderer, leftRenderer and rightRenderer are required`,
          )
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SumType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<SumRenderer<T>, string> =>
      SumRenderer.Operations.tryAsValidSumBaseRenderer(serialized)
        .Then((validatedSerialized) =>
          NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            validatedSerialized.leftRenderer,
            concreteRenderers,
            "Left renderer",
            types,
          ).Then((deserializedLeftRenderer) =>
            NestedRenderer.Operations.DeserializeAs(
              type.args[1],
              validatedSerialized.rightRenderer,
              concreteRenderers,
              "Right renderer",
              types,
            ).Then((deserializedRightRenderer) =>
              Renderer.Operations.Deserialize(
                type,
                validatedSerialized.renderer,
                concreteRenderers,
                types,
              ).Then((renderer) =>
                ValueOrErrors.Default.return(
                  SumRenderer.Default(
                    type,
                    renderer,
                    deserializedLeftRenderer,
                    deserializedRightRenderer,
                  ),
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Sum renderer`),
        ),
  },
};
