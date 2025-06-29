import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  isString,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { OneType } from "../../../../../types/state";
import { NestedRenderer } from "../nestedRenderer/state";
import { Renderer } from "../../state";

export type SerializedOneRenderer = {
  renderer: string;
  detailsRenderer: unknown;
  previewRenderer?: unknown;
  api: Array<string>;
};

export type OneRenderer<T> = {
  kind: "oneRenderer";
  api: Array<string>;
  type: OneType<T>;
  concreteRenderer: string;
  detailsRenderer: NestedRenderer<T>;
  previewRenderer?: NestedRenderer<T>;
};

export const OneRenderer = {
  Default: <T>(
    type: OneType<T>,
    api: Array<string>,
    concreteRenderer: string,
    detailsRenderer: NestedRenderer<T>,
    previewRenderer?: NestedRenderer<T>,
  ): OneRenderer<T> => ({
    kind: "oneRenderer",
    type,
    concreteRenderer,
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
          : !Array.isArray(serialized.api)
            ? ValueOrErrors.Default.throwOne(`api must be an array`)
            : serialized.api.length != 2
              ? ValueOrErrors.Default.throwOne(
                  `api must be an array of length 2`,
                )
              : !serialized.api.every(isString)
                ? ValueOrErrors.Default.throwOne(
                    `api array elements must be strings`,
                  )
                : !("renderer" in serialized)
                  ? ValueOrErrors.Default.throwOne(`renderer is missing`)
                  : !isString(serialized.renderer)
                    ? ValueOrErrors.Default.throwOne(
                        `renderer must be a string`,
                      )
                    : !("detailsRenderer" in serialized)
                      ? ValueOrErrors.Default.throwOne(
                          `detailsRenderer is missing`,
                        )
                      : ValueOrErrors.Default.return({
                          ...serialized,
                          renderer: serialized.renderer,
                          detailsRenderer: serialized.detailsRenderer,
                          api: serialized.api,
                        }),
    DeserializePreviewRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: OneType<T>,
      serialized: SerializedOneRenderer,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<NestedRenderer<T> | undefined, string> =>
      serialized.previewRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedRenderer.Operations.DeserializeAs(
            type.arg,
            serialized.previewRenderer,
            concreteRenderers,
            "preview renderer",
            types,
          ),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: OneType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<OneRenderer<T>, string> =>
      OneRenderer.Operations.tryAsValidOneRenderer(serialized).Then(
        (validatedSerialized) =>
          NestedRenderer.Operations.DeserializeAs(
            type.arg,
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
              ValueOrErrors.Default.return(
                OneRenderer.Default(
                  type,
                  validatedSerialized.api,
                  validatedSerialized.renderer,
                  detailsRenderer,
                  previewRenderer,
                ),
              ),
            ),
          ),
      ),
  },
};
