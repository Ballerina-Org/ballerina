import { ValueOrErrors } from "../../../../../../../../../../../../../../../main";
import { MapType } from "../../../../../../../types/state";
import {
  NestedRenderer,
  BaseSerializedNestedRenderer,
  SerializedNestedRenderer,
  BaseNestedRenderer,
} from "../../state";
import { List } from "immutable";

export type SerializedNestedMapRenderer = {
  keyRenderer?: unknown;
  valueRenderer?: unknown;
} & BaseSerializedNestedRenderer;

export type NestedMapRenderer<T> = BaseNestedRenderer & {
  kind: "nestedMapRenderer";
  keyRenderer: NestedRenderer<T>;
  valueRenderer: NestedRenderer<T>;
  type: MapType<T>;
  concreteRendererName: string;
};

export const NestedMapRenderer = {
  Default: <T>(
    type: MapType<T>,
    concreteRendererName: string,
    keyRenderer: NestedRenderer<T>,
    valueRenderer: NestedRenderer<T>,
    label?: string,
    tooltip?: string,
    details?: string,
  ): NestedMapRenderer<T> => ({
    kind: "nestedMapRenderer",
    type,
    concreteRendererName,
    keyRenderer,
    valueRenderer,
    label,
    tooltip,
    details,
  }),
  Operations: {
    tryAsValidNestedMapRenderer: (
      serialized: SerializedNestedMapRenderer,
    ): ValueOrErrors<
      Omit<
        SerializedNestedMapRenderer,
        "renderer" | "keyRenderer" | "valueRenderer"
      > & {
        renderer: string;
        keyRenderer: SerializedNestedRenderer;
        valueRenderer: SerializedNestedRenderer;
      },
      string
    > => {
      const renderer = serialized.renderer;
      const keyRenderer = serialized.keyRenderer;
      const valueRenderer = serialized.valueRenderer;
      if (renderer == undefined)
        return ValueOrErrors.Default.throwOne(`renderer is missing`);
      if (typeof renderer != "string")
        return ValueOrErrors.Default.throwOne(`renderer must be a string`);
      if (keyRenderer == undefined)
        return ValueOrErrors.Default.throwOne(`keyRenderer is missing`);
      if (valueRenderer == undefined)
        return ValueOrErrors.Default.throwOne(`valueRenderer is missing`);
      return ValueOrErrors.Default.return({
        ...serialized,
        renderer,
        keyRenderer,
        valueRenderer,
      });
    },
    Deserialize: <T>(
      type: MapType<T>,
      serialized: SerializedNestedMapRenderer,
      fieldViews: any,
    ): ValueOrErrors<NestedMapRenderer<T>, string> =>
      NestedMapRenderer.Operations.tryAsValidNestedMapRenderer(serialized)
        .Then((serializedNestedMapRenderer) =>
          NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            serializedNestedMapRenderer.keyRenderer,
            fieldViews,
            "Key",
          ).Then((deserializedKeyRenderer) =>
            NestedRenderer.Operations.DeserializeAs(
              type.args[1],
              serializedNestedMapRenderer.valueRenderer,
              fieldViews,
              "Value",
            ).Then((deserializedValueRenderer) =>
              ValueOrErrors.Default.return(
                NestedMapRenderer.Default(
                  type,
                  serializedNestedMapRenderer.renderer,
                  deserializedKeyRenderer,
                  deserializedValueRenderer,
                  serializedNestedMapRenderer.label,
                  serializedNestedMapRenderer.tooltip,
                  serializedNestedMapRenderer.details,
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Map renderer`),
        ),
  },
};
