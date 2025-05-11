import { Map } from "immutable";
import {
  ConcreteRendererKinds,
  DispatchParsedType,
  isObject,
  isString,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { OneType } from "../../../../../types/state";
import { NestedRenderer } from "../nestedRenderer/state";
import { Renderer } from "../../state";

export type SerializedOneRenderer = {
  renderer: unknown;
  detailsRenderer: unknown;
  previewRenderer?: unknown;
  api: string | Array<string>;
};

export type OneRenderer<T> = {
  kind: "oneRenderer";
  api: string | Array<string>;
  type: OneType<T>;
  renderer: Renderer<T>;
  detailsRenderer: NestedRenderer<T>;
  previewRenderer?: NestedRenderer<T>;
};

export const OneRenderer = {
  Default: <T>(
    type: OneType<T>,
    api: string | Array<string>,
    renderer: Renderer<T>,
    detailsRenderer: NestedRenderer<T>,
    previewRenderer?: NestedRenderer<T>,
  ): OneRenderer<T> => ({
    kind: "oneRenderer",
    type,
    renderer,
    api,
    detailsRenderer,
    previewRenderer,
  }),
  Operations: {
    tryAsValidOneRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedOneRenderer, string> =>
      !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(
            `serialized one renderer is not an object`,
          )
        : !("api" in serialized)
          ? ValueOrErrors.Default.throwOne(`api is missing`)
          : !isString(serialized.api) && !Array.isArray(serialized.api)
            ? ValueOrErrors.Default.throwOne(`api must be a string or an array`)
            : Array.isArray(serialized.api) && serialized.api.length != 2
              ? ValueOrErrors.Default.throwOne(
                  `api must be an array of length 2`,
                )
              : Array.isArray(serialized.api) && !serialized.api.every(isString)
                ? ValueOrErrors.Default.throwOne(
                    `api array elements must be strings`,
                  )
                : !("renderer" in serialized)
                  ? ValueOrErrors.Default.throwOne(`renderer is missing`)
                  : !("detailsRenderer" in serialized)
                    ? ValueOrErrors.Default.throwOne(
                        `detailsRenderer is missing`,
                      )
                    : ValueOrErrors.Default.return({
                        ...serialized,
                        detailsRenderer: serialized.detailsRenderer,
                        api: serialized.api,
                      }),
    DeserializePreviewRenderer: <T>(
      type: OneType<T>,
      serialized: SerializedOneRenderer,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<NestedRenderer<T> | undefined, string> =>
      serialized.previewRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            serialized.previewRenderer,
            concreteRenderers,
            "preview renderer",
            types,
          ),
    Deserialize: <T>(
      type: OneType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<OneRenderer<T>, string> =>
      OneRenderer.Operations.tryAsValidOneRenderer(serialized).Then(
        (validatedSerialized) =>
          NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            validatedSerialized.detailsRenderer,
            concreteRenderers,
            "details renderer",
            types,
          ).Then((detailsRenderer) =>
            OneRenderer.Operations.DeserializePreviewRenderer(
              type,
              validatedSerialized,
              concreteRenderers,
              types,
            ).Then((previewRenderer) =>
              Renderer.Operations.Deserialize(
                type,
                validatedSerialized.renderer,
                concreteRenderers,
                types,
              ).Then((renderer) =>
                ValueOrErrors.Default.return(
                  OneRenderer.Default(
                    type,
                    validatedSerialized.api,
                    renderer,
                    detailsRenderer,
                    previewRenderer,
                  ),
                ),
              ),
            ),
          ),
      ),
  },
};
