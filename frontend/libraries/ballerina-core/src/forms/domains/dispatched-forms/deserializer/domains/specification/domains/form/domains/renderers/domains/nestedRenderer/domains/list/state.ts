import { ValueOrErrors } from "../../../../../../../../../../../../../../../main";
import { ListType } from "../../../../../../../types/state";
import {
  NestedRenderer,
  BaseSerializedNestedRenderer,
  BaseNestedRenderer,
  SerializedNestedRenderer,
} from "../../state";
import { List } from "immutable";

export type SerializedNestedListRenderer = {
  elementRenderer?: unknown;
  elementLabel?: string;
  elementTooltip?: string;
  elementDetails?: string;
} & BaseSerializedNestedRenderer;

export type NestedListRenderer<T> = BaseNestedRenderer & {
  kind: "nestedListRenderer";
  elementRenderer: NestedRenderer<T>;
  type: ListType<T>;
  concreteRendererName: string;
};

export const NestedListRenderer = {
  Default: <T>(
    type: ListType<T>,
    concreteRendererName: string,
    elementRenderer: NestedRenderer<T>,
    label?: string,
    tooltip?: string,
    details?: string,
  ): NestedListRenderer<T> => ({
    kind: "nestedListRenderer",
    type,
    concreteRendererName,
    elementRenderer,
    label,
    tooltip,
    details,
  }),
  Operations: {
    tryAsValidNestedListRenderer: (
      serialized: SerializedNestedListRenderer,
    ): ValueOrErrors<
      Omit<SerializedNestedListRenderer, "renderer" | "elementRenderer"> & {
        renderer: string;
        elementRenderer: SerializedNestedRenderer;
      },
      string
    > => {
      const renderer = serialized.renderer;
      const elementRenderer = serialized.elementRenderer;
      if (renderer == undefined)
        return ValueOrErrors.Default.throwOne(`renderer is missing`);
      if (typeof renderer != "string")
        return ValueOrErrors.Default.throwOne(`renderer must be a string`);
      if (elementRenderer == undefined)
        return ValueOrErrors.Default.throwOne(`elementRenderer is missing`);
      // Backwards compatability
      if (typeof elementRenderer == "string") {
        return ValueOrErrors.Default.return({
          renderer,
          label: serialized?.label,
          details: serialized?.details,
          elementRenderer: {
            renderer: elementRenderer,
            label: serialized?.elementLabel,
            tooltip: serialized?.elementTooltip,
            details: serialized?.elementDetails,
          },
        });
      }
      return ValueOrErrors.Default.return({
        ...serialized,
        renderer,
        elementRenderer,
      });
    },
    Deserialize: <T>(
      type: ListType<T>,
      serialized: SerializedNestedListRenderer,
      fieldViews: any,
    ): ValueOrErrors<NestedListRenderer<T>, string> =>
      NestedListRenderer.Operations.tryAsValidNestedListRenderer(serialized)
        .Then((serializedNestedListRenderer) =>
          NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            serializedNestedListRenderer.elementRenderer,
            fieldViews,
            "Element",
          ).Then((deserializedElementRenderer) =>
            ValueOrErrors.Default.return(
              NestedListRenderer.Default(
                type,
                serializedNestedListRenderer.renderer,
                deserializedElementRenderer,
                serializedNestedListRenderer.label,
                serializedNestedListRenderer.tooltip,
                serializedNestedListRenderer.details,
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as List renderer`),
        ),
  },
};
