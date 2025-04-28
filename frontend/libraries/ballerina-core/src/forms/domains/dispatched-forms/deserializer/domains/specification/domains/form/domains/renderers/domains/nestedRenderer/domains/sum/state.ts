import { ValueOrErrors } from "../../../../../../../../../../../../../../../main";
import { SumType } from "../../../../../../../types/state";
import {
  NestedRenderer,
  BaseSerializedNestedRenderer,
  SerializedNestedRenderer,
  BaseNestedRenderer,
} from "../../state";
import { List } from "immutable";

export type SerializedNestedSumRenderer = {
  leftRenderer?: unknown;
  rightRenderer?: unknown;
} & BaseSerializedNestedRenderer;

export type NestedSumRenderer<T> = BaseNestedRenderer & {
  kind: "nestedSumRenderer";
  leftRenderer: NestedRenderer<T>;
  rightRenderer: NestedRenderer<T>;
  type: SumType<T>;
  concreteRendererName: string;
};

export const NestedSumRenderer = {
  Default: <T>(
    type: SumType<T>,
    concreteRendererName: string,
    leftRenderer: NestedRenderer<T>,
    rightRenderer: NestedRenderer<T>,
    label?: string,
    tooltip?: string,
    details?: string,
  ): NestedSumRenderer<T> => ({
    kind: "nestedSumRenderer",
    type,
    concreteRendererName,
    leftRenderer,
    rightRenderer,
    label,
    tooltip,
    details,
  }),
  Operations: {
    tryAsValidNestedSumRenderer: (
      serialized: SerializedNestedSumRenderer,
    ): ValueOrErrors<
      Omit<
        SerializedNestedSumRenderer,
        "renderer" | "leftRenderer" | "rightRenderer"
      > & {
        renderer: string;
        leftRenderer: SerializedNestedRenderer;
        rightRenderer: SerializedNestedRenderer;
      },
      string
    > => {
      const renderer = serialized.renderer;
      const leftRenderer = serialized.leftRenderer;
      const rightRenderer = serialized.rightRenderer;
      if (renderer == undefined)
        return ValueOrErrors.Default.throwOne(`renderer is missing`);
      if (typeof renderer != "string")
        return ValueOrErrors.Default.throwOne(`renderer must be a string`);
      if (leftRenderer == undefined)
        return ValueOrErrors.Default.throwOne(`leftRenderer is missing`);
      if (rightRenderer == undefined)
        return ValueOrErrors.Default.throwOne(`rightRenderer is missing`);
      return ValueOrErrors.Default.return({
        ...serialized,
        renderer,
        leftRenderer,
        rightRenderer,
      });
    },
    Deserialize: <T>(
      type: SumType<T>,
      serialized: SerializedNestedSumRenderer,
      fieldViews: any,
    ): ValueOrErrors<NestedSumRenderer<T>, string> =>
      NestedSumRenderer.Operations.tryAsValidNestedSumRenderer(serialized)
        .Then((serializedNestedSumRenderer) =>
          NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            serializedNestedSumRenderer.leftRenderer,
            fieldViews,
            "Left",
          ).Then((deserializedLeftRenderer) =>
            NestedRenderer.Operations.DeserializeAs(
              type.args[1],
              serializedNestedSumRenderer.rightRenderer,
              fieldViews,
              "Right",
            ).Then((deserializedRightRenderer) =>
              ValueOrErrors.Default.return(
                NestedSumRenderer.Default(
                  type,
                  serializedNestedSumRenderer.renderer,
                  deserializedLeftRenderer,
                  deserializedRightRenderer,
                  serializedNestedSumRenderer.label,
                  serializedNestedSumRenderer.tooltip,
                  serializedNestedSumRenderer.details,
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
