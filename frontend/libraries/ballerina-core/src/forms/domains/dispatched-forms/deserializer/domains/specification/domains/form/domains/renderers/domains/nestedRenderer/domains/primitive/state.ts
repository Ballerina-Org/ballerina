import { ValueOrErrors } from "../../../../../../../../../../../../../../../main";
import { DispatchPrimitiveType } from "../../../../../../../types/state";
import {
  NestedRenderer,
  BaseSerializedNestedRenderer,
  BaseNestedRenderer,
} from "../../state";
import { List } from "immutable";

export type SerializedNestedPrimitiveRenderer = BaseSerializedNestedRenderer;

export type NestedPrimitiveRenderer<T> = BaseNestedRenderer & {
  kind: "nestedPrimitiveRenderer";
  type: DispatchPrimitiveType<T>;
  concreteRendererName: string;
};

export const NestedPrimitiveRenderer = {
  Default: <T>(
    type: DispatchPrimitiveType<T>,
    concreteRendererName: string,
    label?: string,
    tooltip?: string,
    details?: string,
  ): NestedPrimitiveRenderer<T> => ({
    kind: "nestedPrimitiveRenderer",
    type,
    concreteRendererName,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderer: (
      serialized: SerializedNestedPrimitiveRenderer,
    ): serialized is SerializedNestedPrimitiveRenderer & {
      renderer: string;
    } => {
      return (
        serialized.renderer != undefined &&
        typeof serialized.renderer == "string"
      );
    },
    tryAsValidNestedPrimitiveRenderer: (
      serialized: SerializedNestedPrimitiveRenderer,
    ): ValueOrErrors<
      Omit<SerializedNestedPrimitiveRenderer, "renderer"> & {
        renderer: string;
      },
      string
    > => {
      if (!NestedPrimitiveRenderer.Operations.hasRenderer(serialized))
        return ValueOrErrors.Default.throwOne(`renderer is missing`);

      if (typeof serialized.renderer != "string")
        return ValueOrErrors.Default.throwOne(`renderer must be a string`);

      return ValueOrErrors.Default.return(serialized);
    },
    Deserialize: <T>(
      type: DispatchPrimitiveType<T>,
      serialized: SerializedNestedPrimitiveRenderer,
    ): ValueOrErrors<NestedPrimitiveRenderer<T>, string> =>
      NestedPrimitiveRenderer.Operations.tryAsValidNestedPrimitiveRenderer(
        serialized,
      )
        .Then((primitiveRenderer) =>
          ValueOrErrors.Default.return(
            NestedPrimitiveRenderer.Default(
              type,
              primitiveRenderer.renderer,
              primitiveRenderer.label,
              primitiveRenderer.tooltip,
              primitiveRenderer.details,
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When parsing as Primitive renderer`,
          ),
        ),
  },
};
