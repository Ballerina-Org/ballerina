import { Map } from "immutable";
import { Renderer } from "../../state";
import { NestedRenderer } from "../nestedRenderer/state";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  isString,
  SumType,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";

export type SerializedSumRenderer = {
  renderer: string;
  leftRenderer: unknown;
  rightRenderer: unknown;
};

export type SumRenderer<T> = {
  kind: "sumRenderer";
  concreteRenderer: string;
  leftRenderer: NestedRenderer<T>;
  rightRenderer: NestedRenderer<T>;
  type: SumType<T>;
};

export const SumRenderer = {
  Default: <T>(
    type: SumType<T>,
    concreteRenderer: string,
    leftRenderer: NestedRenderer<T>,
    rightRenderer: NestedRenderer<T>,
  ): SumRenderer<T> => ({
    kind: "sumRenderer",
    type,
    concreteRenderer,
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
        : !isString(serialized.renderer)
          ? ValueOrErrors.Default.throwOne(`renderer must be a string`)
          : ValueOrErrors.Default.return({
              ...serialized,
              renderer: serialized.renderer,
            }),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: SumType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
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
              ValueOrErrors.Default.return(
                SumRenderer.Default(
                  type,
                  validatedSerialized.renderer,
                  deserializedLeftRenderer,
                  deserializedRightRenderer,
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
