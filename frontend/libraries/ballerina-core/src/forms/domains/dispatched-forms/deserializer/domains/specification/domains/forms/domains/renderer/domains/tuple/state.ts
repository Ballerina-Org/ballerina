import { List, Map } from "immutable";
import { NestedRenderer } from "../nestedRenderer/state";
import { DispatchParsedType, TupleType } from "../../../../../types/state";
import { Renderer } from "../../state";
import { ValueOrErrors } from "../../../../../../../../../../../../collections/domains/valueOrErrors/state";
import {
  ConcreteRendererKinds,
  isObject,
} from "../../../../../../../../../../../../../main";

export type SerializedTupleRenderer = {
  renderer: unknown;
  itemRenderers: Array<unknown>;
};

export type TupleRenderer<T> = {
  kind: "tupleRenderer";
  renderer: Renderer<T>;
  itemRenderers: Array<NestedRenderer<T>>;
  type: TupleType<T>;
};

export const TupleRenderer = {
  Default: <T>(
    type: TupleType<T>,
    renderer: Renderer<T>,
    itemRenderers: Array<NestedRenderer<T>>,
  ): TupleRenderer<T> => ({
    kind: "tupleRenderer",
    type,
    renderer,
    itemRenderers,
  }),
  Operations: {
    tryAsValidTupleRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedTupleRenderer, string> =>
      !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(`serialized must be an object`)
        : !("renderer" in serialized)
          ? ValueOrErrors.Default.throwOne(`renderer is required`)
          : !("itemRenderers" in serialized)
            ? ValueOrErrors.Default.throwOne(`itemRenderers is required`)
            : !Array.isArray(serialized.itemRenderers)
              ? ValueOrErrors.Default.throwOne(`itemRenderers must be an array`)
              : serialized.itemRenderers.length == 0
                ? ValueOrErrors.Default.throwOne(
                    `itemRenderers must have at least one item`,
                  )
                : ValueOrErrors.Default.return({
                    ...serialized,
                    itemRenderers: serialized.itemRenderers,
                  }),
    Deserialize: <T>(
      type: TupleType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds<T>, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<TupleRenderer<T>, string> =>
      TupleRenderer.Operations.tryAsValidTupleRenderer(serialized).Then(
        (validatedSerialized) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<NestedRenderer<T>, string>>(
              validatedSerialized.itemRenderers.map((itemRenderer, index) =>
                NestedRenderer.Operations.DeserializeAs(
                  type.args[index],
                  itemRenderer,
                  concreteRenderers,
                  `Item ${index + 1}`,
                  types,
                ).Then((deserializedItemRenderer) =>
                  ValueOrErrors.Default.return(deserializedItemRenderer),
                ),
              ),
            ),
          )
            .Then((deserializedItemRenderers) =>
              Renderer.Operations.Deserialize(
                type,
                validatedSerialized.renderer,
                concreteRenderers,
                types,
              ).Then((deserializedRenderer) =>
                ValueOrErrors.Default.return(
                  TupleRenderer.Default(
                    type,
                    deserializedRenderer,
                    deserializedItemRenderers.toArray(),
                  ),
                ),
              ),
            )
            .MapErrors((errors) =>
              errors.map(
                (error) => `${error}\n...When parsing as Tuple renderer`,
              ),
            ),
      ),
  },
};
