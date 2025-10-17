import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isString,
} from "../../../../../../../../../../../../../main";
import {
  isObject,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { SumType } from "../../../../../types/state";
import { Renderer } from "../../state";

export type SerializedSumUnitDateBaseRenderer = {
  renderer: string;
};

export type SumUnitDateRenderer<T> = {
  kind: "sumUnitDateRenderer";
  type: SumType<T>;
  concreteRenderer: string;
};

export type BaseSumUnitDateRenderer<T> = {
  kind: "sumUnitDateRenderer";
  type: SumType<T>;
  concreteRenderer: string;
};

export const BaseSumUnitDateRenderer = {
  Default: <T>(
    type: SumType<T>,
    concreteRenderer: string,
  ): BaseSumUnitDateRenderer<T> => ({
    kind: "sumUnitDateRenderer",
    type,
    concreteRenderer,
  }),
  Operations: {
    tryAsValidSumUnitDateBaseRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedSumUnitDateBaseRenderer, string> =>
      !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(
            `sumUnitDate renderer is not an object`,
          )
        : !("renderer" in serialized)
          ? ValueOrErrors.Default.throwOne(`renderer is required`)
          : !isString(serialized.renderer)
            ? ValueOrErrors.Default.throwOne(`renderer must be a string`)
            : ValueOrErrors.Default.return({
                renderer: serialized.renderer,
              }),
    Deserialize: <T extends DispatchInjectablesTypes<T>>(
      type: SumType<T>,
      serialized: unknown,
    ): ValueOrErrors<BaseSumUnitDateRenderer<T>, string> =>
      BaseSumUnitDateRenderer.Operations.tryAsValidSumUnitDateBaseRenderer(
        serialized,
      )
        .Then((validatedSerialized) =>
          ValueOrErrors.Default.return(
            BaseSumUnitDateRenderer.Default(type, validatedSerialized.renderer),
          ),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When parsing as SumUnitDate renderer`,
          ),
        ),
  },
};
