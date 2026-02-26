import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  isString,
  Renderer,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { ReferenceType } from "../../../../../types/state";
import { NestedRenderer } from "../nestedRenderer/state";

export type SerializedReferenceRenderer = {
  renderer: string;
  detailsRenderer: unknown;
  previewRenderer?: unknown;
  api: Array<string>;
};

export type ReferenceRenderer<T> = {
  kind: "referenceRenderer";
  api: Array<string>;
  type: ReferenceType<T>;
  concreteRenderer: string;
  detailsRenderer: NestedRenderer<T>;
  previewRenderer?: NestedRenderer<T>;
};

export const ReferenceRenderer = {
  Default: <T>(
    type: ReferenceType<T>,
    api: Array<string>,
    concreteRenderer: string,
    detailsRenderer: NestedRenderer<T>,
    previewRenderer?: NestedRenderer<T>,
  ): ReferenceRenderer<T> => ({
    kind: "referenceRenderer",
    type,
    concreteRenderer,
    api,
    detailsRenderer,
    previewRenderer,
  }),
  Operations: {
    tryAsValidReferenceRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedReferenceRenderer, string> =>
      !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(
            `serialized reference renderer is not an object`,
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
      CustomPresentationContext,
      ExtraContext,
    >(
      type: ReferenceType<T>,
      serialized: SerializedReferenceRenderer,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
      forms: object,
      alreadyParsedForms: Map<string, Renderer<T>>,
    ): ValueOrErrors<
      [NestedRenderer<T> | undefined, Map<string, Renderer<T>>],
      string
    > =>
      serialized.previewRenderer == undefined
        ? ValueOrErrors.Default.return<
            [NestedRenderer<T> | undefined, Map<string, Renderer<T>>],
            string
          >([undefined, alreadyParsedForms])
        : NestedRenderer.Operations.DeserializeAs(
            type.arg,
            serialized.previewRenderer,
            concreteRenderers,
            "preview renderer",
            types,
            forms,
            alreadyParsedForms,
          ),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      type: ReferenceType<T>,
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
    ): ValueOrErrors<[ReferenceRenderer<T>, Map<string, Renderer<T>>], string> =>
      ReferenceRenderer.Operations.tryAsValidReferenceRenderer(serialized).Then(
        (validatedSerialized) =>
          NestedRenderer.Operations.DeserializeAs(
            type.arg,
            validatedSerialized.detailsRenderer,
            concreteRenderers,
            "details renderer",
            types,
            forms,
            alreadyParsedForms,
          ).Then(([detailsRenderer, detailsAlreadyParsedForms]) =>
            ReferenceRenderer.Operations.DeserializePreviewRenderer(
              type,
              validatedSerialized,
              concreteRenderers,
              types,
              forms,
              detailsAlreadyParsedForms,
            ).Then(([previewRenderer, previewAlreadyParsedForms]) =>
              ValueOrErrors.Default.return<
                [ReferenceRenderer<T>, Map<string, Renderer<T>>],
                string
              >([
                ReferenceRenderer.Default(
                  type,
                  validatedSerialized.api,
                  validatedSerialized.renderer,
                  detailsRenderer,
                  previewRenderer,
                ),
                previewAlreadyParsedForms,
              ]),
            ),
          ),
      ),
  },
};
