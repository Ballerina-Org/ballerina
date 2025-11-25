import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  isString,
  SpecVersion,
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

const isLookupRenderer = (renderer: unknown): boolean => {
  return (
    typeof renderer === "object" &&
    renderer !== null &&
    !("options" in renderer) &&
    !("stream" in renderer) &&
    !("leftRenderer" in renderer) &&
    !("rightRenderer" in renderer) &&
    !("elementRenderer" in renderer) &&
    !("itemRenderers" in renderer) &&
    !("keyRenderer" in renderer) &&
    !("valueRenderer" in renderer)
  );
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
      CustomPresentationContext,
      ExtraContext,
    >(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      as: string,
      types: Map<string, DispatchParsedType<T>>,
      forms: object,
      alreadyParsedForms: Map<string, Renderer<T>>,
      specVersionContext: SpecVersion,
    ): ValueOrErrors<[NestedRenderer<T>, Map<string, Renderer<T>>], string> =>
      NestedRenderer.Operations.Deserialize(
        type,
        serialized,
        concreteRenderers,
        types,
        forms,
        alreadyParsedForms,
        specVersionContext,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing as ${as}`),
      ),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
      forms: object,
      alreadyParsedForms: Map<string, Renderer<T>>,
      specVersionContext: SpecVersion,
    ): ValueOrErrors<[NestedRenderer<T>, Map<string, Renderer<T>>], string> =>
      NestedRenderer.Operations.tryAsValidSerializedNestedRenderer(
        serialized,
      ).Then((validatedSerialized) =>
        Renderer.Operations.Deserialize(
          type,
          type.kind == "primitive" ||
            (type.kind == "lookup" && isLookupRenderer(validatedSerialized)) ||
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
          forms,
          alreadyParsedForms,
          specVersionContext,
        ).Then(([renderer, newAlreadyParsedForms]) =>
          ValueOrErrors.Default.return<
            [NestedRenderer<T>, Map<string, Renderer<T>>],
            string
          >([
            {
              renderer,
              label: validatedSerialized.label,
              tooltip: validatedSerialized.tooltip,
              details: validatedSerialized.details,
            },
            newAlreadyParsedForms,
          ]).MapErrors<
            [NestedRenderer<T>, Map<string, Renderer<T>>],
            string,
            string
          >((errors) =>
            errors.map(
              (error) =>
                `${error}\n...When parsing as ${renderer.kind} nested renderer`,
            ),
          ),
        ),
      ),
  },
};
