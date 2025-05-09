import { Map } from "immutable";
import {
  DispatchParsedType,
  isObject,
  isString,
} from "../../../../../../../../../../../main";
import { ValueOrErrors } from "../../../../../../../../../../collections/domains/valueOrErrors/state";
import { Renderer } from "../renderer/state";

export type SerializedNestedRenderer = {
  renderer: unknown;
  label?: unknown;
  tooltip?: unknown;
  details?: unknown;
};

export type NestedRenderer<T> = {
  renderer: Renderer<T>;
  label?: string;
  tooltip?: string;
  details?: string;
};

export const NestedRenderer = {
  Operations: {
    tryAsValidSerializedNestedRenderer: <T>(
      serialized: unknown,
    ): ValueOrErrors<
      Omit<
        SerializedNestedRenderer,
        "renderer" | "label" | "tooltip" | "details"
      > & {
        renderer: unknown;
        label?: string;
        tooltip?: string;
        details?: string;
      },
      string
    > =>
      !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(
            `serialized nested renderer is not an object`,
          )
        : "label" in serialized && !isString(serialized.label)
        ? ValueOrErrors.Default.throwOne(`label is not a string`)
        : "tooltip" in serialized && !isString(serialized.tooltip)
        ? ValueOrErrors.Default.throwOne(`tooltip is not a string`)
        : "details" in serialized && !isString(serialized.details)
        ? ValueOrErrors.Default.throwOne(`details is not a string`)
        : !("renderer" in serialized)
        ? ValueOrErrors.Default.throwOne(`renderer is missing`)
        : ValueOrErrors.Default.return(serialized),
    DeserializeAs: <T>(
      type: DispatchParsedType<T>,
      serialized: unknown,
      fieldViews: any,
      as: string,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<NestedRenderer<T>, string> =>
      Renderer.Operations.DeserializeAs(
        type,
        serialized,
        fieldViews,
        as,
        types,
      ).Then((renderer) =>
        ValueOrErrors.Default.return({
          renderer,
        }),
      ),
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      serialized: unknown,
      fieldViews: any,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<NestedRenderer<T>, string> =>
      NestedRenderer.Operations.tryAsValidSerializedNestedRenderer(serialized)
        .Then((validatedSerialized) =>
          Renderer.Operations.Deserialize(
            type,
            validatedSerialized.renderer,
            fieldViews,
            types,
          ).Then((renderer) =>
            ValueOrErrors.Default.return({
              renderer,
              label: validatedSerialized.label,
              tooltip: validatedSerialized.tooltip,
              details: validatedSerialized.details,
            }),
          ),
        ),
  },
};
