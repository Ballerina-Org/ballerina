import { Map } from "immutable";
import {
  ConcreteRendererKinds,
  DispatchParsedType,
} from "../../../../../../../../../../../../../main";
import {
  isObject,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { SumType } from "../../../../../types/state";
import { Renderer } from "../../state";

export type SerializedSumUnitDateBaseRenderer = {
  renderer: unknown;
};

export type SumUnitDateRenderer<T> = {
  kind: "sumUnitDateRenderer";
  type: SumType<T>;
  renderer: Renderer<T>;
};

export type BaseSumUnitDateRenderer<T> = {
  kind: "sumUnitDateRenderer";
  type: SumType<T>;
  renderer: Renderer<T>;
};

export const BaseSumUnitDateRenderer = {
  Default: <T>(
    type: SumType<T>,
    renderer: Renderer<T>,
  ): BaseSumUnitDateRenderer<T> => ({
    kind: "sumUnitDateRenderer",
    type,
    renderer,
  }),
  Operations: {
    tryAsValidSumUnitDateBaseRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedSumUnitDateBaseRenderer, string> =>
      !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(`renderer is required`)
        : !("renderer" in serialized)
          ? ValueOrErrors.Default.throwOne(`renderer is required`)
          : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SumType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<BaseSumUnitDateRenderer<T>, string> =>
      BaseSumUnitDateRenderer.Operations.tryAsValidSumUnitDateBaseRenderer(
        serialized,
      )
        .Then((validatedSerialized) =>
          Renderer.Operations.Deserialize(
            type,
            validatedSerialized.renderer,
            concreteRenderers,
            types,
          ).Then((renderer) =>
            ValueOrErrors.Default.return(
              BaseSumUnitDateRenderer.Default(type, renderer),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When parsing as SumUnitDate renderer`,
          ),
        ),
  },
};
