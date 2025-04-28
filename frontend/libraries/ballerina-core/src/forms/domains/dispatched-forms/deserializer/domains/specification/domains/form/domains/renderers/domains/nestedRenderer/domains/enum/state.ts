import { ValueOrErrors } from "../../../../../../../../../../../../../../../main";
import {
  MultiSelectionType,
  SingleSelectionType,
} from "../../../../../../../types/state";
import { BaseNestedRenderer, BaseSerializedNestedRenderer } from "../../state";
import { List } from "immutable";

export type SerializedNestedEnumRenderer = {
  options?: string;
} & BaseSerializedNestedRenderer;

export type NestedEnumRenderer<T> = BaseNestedRenderer & {
  kind: "nestedEnumRenderer";
  options: string;
  type: SingleSelectionType<T> | MultiSelectionType<T>;
  concreteRendererName: string;
};

export const NestedEnumRenderer = {
  Default: <T>(
    type: SingleSelectionType<T> | MultiSelectionType<T>,
    concreteRendererName: string,
    options: string,
    label?: string,
    tooltip?: string,
    details?: string,
  ): NestedEnumRenderer<T> => ({
    kind: "nestedEnumRenderer",
    type,
    concreteRendererName,
    options,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderer: (
      serialized: SerializedNestedEnumRenderer,
    ): serialized is SerializedNestedEnumRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",
    hasOptions: (
      serialized: SerializedNestedEnumRenderer,
    ): serialized is SerializedNestedEnumRenderer & {
      options: string;
    } =>
      serialized.options != undefined && typeof serialized.options == "string",
    tryAsValidNestedEnumRenderer: (
      serialized: SerializedNestedEnumRenderer,
    ): ValueOrErrors<
      Omit<SerializedNestedEnumRenderer, "renderer" | "options"> & {
        renderer: string;
        options: string;
      },
      string
    > => {
      if (!NestedEnumRenderer.Operations.hasRenderer(serialized))
        return ValueOrErrors.Default.throwOne(`renderer is missing`);

      if (!NestedEnumRenderer.Operations.hasOptions(serialized))
        return ValueOrErrors.Default.throwOne(`options are missing`);

      if (typeof serialized.options != "string")
        return ValueOrErrors.Default.throwOne(`options must be a string`);

      if (serialized.label && typeof serialized.label != "string")
        return ValueOrErrors.Default.throwOne(`label must be a string`);

      if (serialized.tooltip && typeof serialized.tooltip != "string")
        return ValueOrErrors.Default.throwOne(`tooltip must be a string`);

      if (serialized.details && typeof serialized.details != "string")
        return ValueOrErrors.Default.throwOne(`details must be a string`);

      return ValueOrErrors.Default.return(serialized);
    },
    Deserialize: <T>(
      type: SingleSelectionType<T> | MultiSelectionType<T>,
      serialized: SerializedNestedEnumRenderer,
    ): ValueOrErrors<NestedEnumRenderer<T>, string> =>
      NestedEnumRenderer.Operations.tryAsValidNestedEnumRenderer(serialized)
        .Then((nestedEnumRenderer) =>
          ValueOrErrors.Default.return(
            NestedEnumRenderer.Default(
              type,
              nestedEnumRenderer.renderer,
              nestedEnumRenderer.options,
              nestedEnumRenderer.label,
              nestedEnumRenderer.tooltip,
              nestedEnumRenderer.details,
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Enum renderer`),
        ),
  },
};
