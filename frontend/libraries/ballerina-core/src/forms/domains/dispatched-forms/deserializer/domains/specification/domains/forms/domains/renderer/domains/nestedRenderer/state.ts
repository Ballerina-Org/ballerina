import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  isString,
} from "../../../../../../../../../../../../../main";
import { ValueOrErrors } from "../../../../../../../../../../../../collections/domains/valueOrErrors/state";
import { Renderer } from "../../state";

export type SerializedNestedRenderer = {
  renderer: unknown;
  options?: unknown;
  stream?: unknown;
  leftRenderer?: unknown;
  rightRenderer?: unknown;
  elementRenderer?: unknown;
  itemRenderers?: unknown;
  keyRenderer?: unknown;
  valueRenderer?: unknown;
  label?: unknown;
  tooltip?: unknown;
  details?: unknown;
  api?: unknown;
};

export type NestedRenderer<T> = {
  renderer: Renderer<T>;
  label?: string;
  tooltip?: string;
  details?: string;
};

export const NestedRenderer = {
  Operations: {
    tryAsValidSerializedNestedRenderer: (
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
    DeserializeAs: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      as: string,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<NestedRenderer<T>, string> =>
      NestedRenderer.Operations.Deserialize(
        type,
        serialized,
        concreteRenderers,
        types,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing as ${as}`),
      ),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<NestedRenderer<T>, string> =>
      NestedRenderer.Operations.tryAsValidSerializedNestedRenderer(
        serialized,
      ).Then((validatedSerialized) =>
        Renderer.Operations.Deserialize(
          type,
          type.kind == "primitive" ||
            type.kind == "lookup" ||
            type.kind == "record" ||
            type.kind == "union" ||
            type.kind == "table"
            ? validatedSerialized.renderer
            : validatedSerialized,
          concreteRenderers,
          types,
          "api" in validatedSerialized && isString(validatedSerialized.api)
            ? validatedSerialized.api
            : undefined,
        ).Then((renderer) =>
          ValueOrErrors.Default.return<NestedRenderer<T>, string>({
            renderer,
            label: validatedSerialized.label,
            tooltip: validatedSerialized.tooltip,
            details: validatedSerialized.details,
          }).MapErrors<NestedRenderer<T>, string, string>((errors) =>
            errors.map(
              (error) =>
                `${error}\n...When parsing as ${renderer.kind} nested renderer`,
            ),
          ),
        ),
      ),
  },
};
