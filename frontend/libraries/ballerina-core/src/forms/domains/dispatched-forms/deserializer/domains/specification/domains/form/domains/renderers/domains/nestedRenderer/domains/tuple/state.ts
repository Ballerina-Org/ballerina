import { ValueOrErrors } from "../../../../../../../../../../../../../../../main";
import { TupleType } from "../../../../../../../types/state";
import {
  BaseSerializedNestedRenderer,
  NestedRenderer,
  SerializedNestedRenderer,
  BaseNestedRenderer,
} from "../../state";
import { List } from "immutable";

export type SerializedNestedTupleRenderer = {
  itemRenderers?: unknown;
} & BaseSerializedNestedRenderer;

export type NestedTupleRenderer<T> = BaseNestedRenderer & {
  kind: "nestedTupleRenderer";
  itemRenderers: Array<NestedRenderer<T>>;
  type: TupleType<T>;
  concreteRendererName: string;
};

export const NestedTupleRenderer = {
  Default: <T>(
    type: TupleType<T>,
    concreteRendererName: string,
    itemRenderers: Array<NestedRenderer<T>>,
    label?: string,
    tooltip?: string,
    details?: string,
  ): NestedTupleRenderer<T> => ({
    kind: "nestedTupleRenderer",
    type,
    concreteRendererName,
    itemRenderers,
    label,
    tooltip,
    details,
  }),
  Operations: {
    tryAsValidNestedTupleRenderer: <T>(
      serialized: SerializedNestedTupleRenderer,
      type: TupleType<T>,
    ): ValueOrErrors<
      Omit<SerializedNestedTupleRenderer, "renderer" | "itemRenderers"> & {
        renderer: string;
        itemRenderers: Array<SerializedNestedRenderer>;
      },
      string
    > => {
      const renderer = serialized.renderer;
      const itemRenderers = serialized.itemRenderers;
      if (renderer == undefined)
        return ValueOrErrors.Default.throwOne(`renderer is missing`);
      if (typeof renderer != "string")
        return ValueOrErrors.Default.throwOne(`renderer must be a string`);
      if (itemRenderers == undefined)
        return ValueOrErrors.Default.throwOne(`itemRenderers are missing`);
      if (!Array.isArray(itemRenderers))
        return ValueOrErrors.Default.throwOne(`itemRenderers must be an array`);
      if (itemRenderers.length != type.args.length)
        return ValueOrErrors.Default.throwOne(
          `${itemRenderers.length} itemRenderers found, but ${type.args.length} expected`,
        );
      if (itemRenderers.some((itemRenderer) => typeof itemRenderer != "object"))
        return ValueOrErrors.Default.throwOne(`itemRenderers must be objects`);
      return ValueOrErrors.Default.return({
        ...serialized,
        renderer,
        itemRenderers,
      });
    },
    Deserialize: <T>(
      type: TupleType<T>,
      serialized: SerializedNestedTupleRenderer,
      fieldViews: any,
    ): ValueOrErrors<NestedTupleRenderer<T>, string> =>
      NestedTupleRenderer.Operations.tryAsValidNestedTupleRenderer(
        serialized,
        type,
      )
        .Then((serializedNestedTupleRenderer) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<NestedRenderer<T>, string>>(
              serializedNestedTupleRenderer.itemRenderers.map(
                (itemRenderer, index) =>
                  NestedRenderer.Operations.DeserializeAs(
                    type.args[index],
                    itemRenderer,
                    fieldViews,
                    `Item ${index + 1} renderer`,
                  ).Then((deserializedItemRenderer) =>
                    ValueOrErrors.Default.return(deserializedItemRenderer),
                  ),
              ),
            ),
          ).Then((deserializedItemRenderers) =>
            ValueOrErrors.Default.return(
              NestedTupleRenderer.Default(
                type,
                serializedNestedTupleRenderer.renderer,
                deserializedItemRenderers.toArray(),
                serializedNestedTupleRenderer.label,
                serializedNestedTupleRenderer.tooltip,
                serializedNestedTupleRenderer.details,
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Tuple renderer`),
        ),
  },
};
