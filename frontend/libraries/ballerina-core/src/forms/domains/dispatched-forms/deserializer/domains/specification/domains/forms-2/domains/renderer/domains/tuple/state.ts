import { List, Map } from "immutable";
import { NestedRenderer } from "../../../nestedRenderer/state";
import { DispatchParsedType, TupleType } from "../../../../../types/state";
import { Renderer } from "../../state";
import { ValueOrErrors } from "../../../../../../../../../../../../collections/domains/valueOrErrors/state";
import { isObject } from "../../../../../../../../../../../../../main";

export type SerializedBaseTupleRenderer = {
  renderer: unknown;
  itemRenderers: Array<unknown>;
};

export type BaseTupleRenderer<T> = {
  kind: "tupleRenderer";
  renderer: Renderer<T>;
  itemRenderers: Array<NestedRenderer<T>>;
  type: TupleType<T>;
};

export const BaseTupleRenderer = {
  Default: <T>(
    type: TupleType<T>,
    renderer: Renderer<T>,
    itemRenderers: Array<NestedRenderer<T>>,
  ): BaseTupleRenderer<T> => ({
    kind: "tupleRenderer",
    type,
    renderer,
    itemRenderers,
  }),
  Operations: {
    tryAsValidBaseTupleRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedBaseTupleRenderer, string> =>
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
      serialized: SerializedBaseTupleRenderer,
      fieldViews: any,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<BaseTupleRenderer<T>, string> =>
      BaseTupleRenderer.Operations.tryAsValidBaseTupleRenderer(serialized).Then(
        (validatedSerialized) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<NestedRenderer<T>, string>>(
              validatedSerialized.itemRenderers.map((itemRenderer, index) =>
                NestedRenderer.Operations.DeserializeAs(
                  type.args[index],
                  itemRenderer,
                  fieldViews,
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
                fieldViews,
                types,
              ).Then((deserializedRenderer) =>
                ValueOrErrors.Default.return(
                  BaseTupleRenderer.Default(
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
