import { ValueOrErrors } from "../../../../../../../../../../../../../../../main";
import {
  SingleSelectionType,
  MultiSelectionType,
} from "../../../../../../../types/state";
import { BaseSerializedNestedRenderer, BaseNestedRenderer } from "../../state";
import { List } from "immutable";

export type SerializedNestedStreamRenderer = {
  stream?: string;
} & BaseSerializedNestedRenderer;

export type NestedStreamRenderer<T> = BaseNestedRenderer & {
  kind: "nestedStreamRenderer";
  stream: string;
  type: SingleSelectionType<T> | MultiSelectionType<T>;
  concreteRendererName: string;
};

export const NestedStreamRenderer = {
  Default: <T>(
    concreteRendererName: string,
    type: SingleSelectionType<T> | MultiSelectionType<T>,
    stream: string,
    label?: string,
    tooltip?: string,
    details?: string,
  ): NestedStreamRenderer<T> => ({
    kind: "nestedStreamRenderer",
    concreteRendererName,
    type,
    stream,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderer: (
      serialized: SerializedNestedStreamRenderer,
    ): serialized is SerializedNestedStreamRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",
    hasStream: (
      serialized: SerializedNestedStreamRenderer,
    ): serialized is SerializedNestedStreamRenderer & {
      stream: string;
    } => serialized.stream != undefined && typeof serialized.stream == "string",
    tryAsValidNestedStreamRenderer: (
      serialized: SerializedNestedStreamRenderer,
    ): ValueOrErrors<
      Omit<SerializedNestedStreamRenderer, "renderer" | "stream"> & {
        renderer: string;
        stream: string;
      },
      string
    > => {
      if (!NestedStreamRenderer.Operations.hasRenderer(serialized))
        return ValueOrErrors.Default.throwOne(`renderer is missing`);

      if (!NestedStreamRenderer.Operations.hasStream(serialized))
        return ValueOrErrors.Default.throwOne(`stream is missing`);

      return ValueOrErrors.Default.return(serialized);
    },
    Deserialize: <T>(
      type: SingleSelectionType<T> | MultiSelectionType<T>,
      serialized: SerializedNestedStreamRenderer,
    ): ValueOrErrors<NestedStreamRenderer<T>, string> =>
      NestedStreamRenderer.Operations.tryAsValidNestedStreamRenderer(serialized)
        .Then((streamRecordFieldRenderer) =>
          ValueOrErrors.Default.return(
            NestedStreamRenderer.Default(
              streamRecordFieldRenderer.renderer,
              type,
              streamRecordFieldRenderer.stream,
              streamRecordFieldRenderer.label,
              streamRecordFieldRenderer.tooltip,
              streamRecordFieldRenderer.details,
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Stream renderer`),
        ),
  },
};
