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
  SpecVersion,
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
      CustomPresentationContext,
      ExtraContext,
    >(
      type: SumType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
      forms: object,
      alreadyParsedForms: Map<string, Renderer<T>>,
      specVersionContext: SpecVersion,
    ): ValueOrErrors<[SumRenderer<T>, Map<string, Renderer<T>>], string> =>
      SumRenderer.Operations.tryAsValidSumBaseRenderer(serialized)
        .Then((validatedSerialized) =>
          NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            validatedSerialized.leftRenderer,
            concreteRenderers,
            "Left renderer",
            types,
            forms,
            alreadyParsedForms,
            specVersionContext,
          ).Then(([deserializedLeftRenderer, leftAlreadyParsedForms]) =>
            NestedRenderer.Operations.DeserializeAs(
              type.args[1],
              validatedSerialized.rightRenderer,
              concreteRenderers,
              "Right renderer",
              types,
              forms,
              leftAlreadyParsedForms,
              specVersionContext,
            ).Then(([deserializedRightRenderer, rightAlreadyParsedForms]) =>
              ValueOrErrors.Default.return<
                [SumRenderer<T>, Map<string, Renderer<T>>],
                string
              >([
                SumRenderer.Default(
                  type,
                  validatedSerialized.renderer,
                  deserializedLeftRenderer,
                  deserializedRightRenderer,
                ),
                rightAlreadyParsedForms,
              ]),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Sum renderer`),
        ),
  },
};
