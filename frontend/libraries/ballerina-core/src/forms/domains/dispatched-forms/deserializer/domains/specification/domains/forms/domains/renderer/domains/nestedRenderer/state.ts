import { Map } from "immutable";
import {
  ConcreteRendererKinds,
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
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      as: string,
      types: Map<string, DispatchParsedType<T>>,
      canOmitType?: boolean,
    ): ValueOrErrors<NestedRenderer<T>, string> =>
      NestedRenderer.Operations.Deserialize(
        type,
        serialized,
        concreteRenderers,
        types,
        canOmitType,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing as ${as}`),
      ),
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
      canOmitType?: boolean,
    ): ValueOrErrors<NestedRenderer<T>, string> =>
      NestedRenderer.Operations.tryAsValidSerializedNestedRenderer(
        serialized,
      ).Then((validatedSerialized) =>
        type.kind == "primitive" ||
        type.kind == "singleSelection" ||
        type.kind == "multiSelection" ||
        type.kind == "list" ||
        type.kind == "sum" ||
        type.kind == "tuple" ||
        type.kind == "one" ||
        type.kind == "map"
          ? Renderer.Operations.Deserialize(
              type,
              type.kind == "primitive"
                ? validatedSerialized.renderer
                : validatedSerialized,
              concreteRenderers,
              types,
            ).Then((renderer) =>
              renderer.kind == "tableRenderer" ||
              renderer.kind == "recordRenderer" ||
              renderer.kind == "unionRenderer"
                ? ValueOrErrors.Default.throwOne<NestedRenderer<T>, string>(
                    `renderer ${renderer.kind} does not match type ${type.kind}`,
                  )
                : ValueOrErrors.Default.return<NestedRenderer<T>, string>({
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
            )
          : Renderer.Operations.Deserialize(
              type,
              validatedSerialized.renderer,
              concreteRenderers,
              types,
              "api" in validatedSerialized && isString(validatedSerialized.api)
                ? validatedSerialized.api
                : undefined,
              canOmitType,
            ).Then((renderer) =>
              type.kind == "table" && renderer.kind == "tableRenderer"
                ? ValueOrErrors.Default.return({
                    renderer,
                    label: validatedSerialized.label,
                    tooltip: validatedSerialized.tooltip,
                    details: validatedSerialized.details,
                  }).MapErrors((errors) =>
                    errors.map(
                      (error) =>
                        `${error}\n...When parsing as table nested renderer`,
                    ),
                  )
                : renderer.kind == "recordRenderer" ||
                    renderer.kind == "unionRenderer" ||
                    renderer.kind == "lookupRenderer"
                  ? ValueOrErrors.Default.return({
                      renderer,
                      label: validatedSerialized.label,
                      tooltip: validatedSerialized.tooltip,
                      details: validatedSerialized.details,
                    }).MapErrors((errors) =>
                      errors.map(
                        (error) =>
                          `${error}\n...When parsing as ${renderer.kind} nested renderer`,
                      ),
                    )
                  : ValueOrErrors.Default.throwOne(
                      `renderer ${renderer.kind} does not match type ${type.kind}`,
                    ),
            ),
      ),
  },
};
