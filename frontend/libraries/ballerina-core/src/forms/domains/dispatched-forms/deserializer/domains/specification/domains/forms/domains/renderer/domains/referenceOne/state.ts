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
import { ReferenceOneType } from "../../../../../types/state";
import { NestedRenderer } from "../nestedRenderer/state";

export type SerializedReferenceOneRenderer = {
  renderer: string;
  entityName: string
  detailsRenderer?: unknown;
  previewRenderer?: unknown;
};

export type ReferenceOneRenderer<T> = {
  kind: "referenceOneRenderer";
  type: ReferenceOneType<T>;
  entityName: string
  concreteRenderer: string; 
  detailsRenderer?: NestedRenderer<T>;
  previewRenderer?: NestedRenderer<T>;
};

export const ReferenceOneRenderer = {
  Default: <T>(
    type: ReferenceOneType<T>,
    concreteRenderer: string, 
    entityName: string, 
    detailsRenderer?: NestedRenderer<T>,
    previewRenderer?: NestedRenderer<T>,
  ): ReferenceOneRenderer<T> => ({
    kind: "referenceOneRenderer",
    type,
    concreteRenderer,
    entityName,
    detailsRenderer,
    previewRenderer,
  }),
  Operations: {
    tryAsValidReferenceOneRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedReferenceOneRenderer, string> => {
      return !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(
            `serialized referenceOne renderer is not an object`,
          )
        : !("renderer" in serialized)
          ? ValueOrErrors.Default.throwOne(`renderer is missing`)
          : !isString(serialized.renderer)
            ? ValueOrErrors.Default.throwOne(`renderer must be a string`)
            : !("entityName" in serialized)
              ? ValueOrErrors.Default.throwOne(`entityName is missing`)
              : !isString(serialized.entityName)
                ? ValueOrErrors.Default.throwOne(`entityName must be a string`)
                : !("vectorName" in serialized)
                  ? ValueOrErrors.Default.throwOne(`vectorName is missing`)
                  : !isString(serialized.vectorName)
                    ? ValueOrErrors.Default.throwOne(`vectorName must be a string`)
                      : !("previewRenderer" in serialized || "detailsRenderer" in serialized)
                      ? ValueOrErrors.Default.throwOne(
                          `previewRenderer or detailsRenderer or both should be present`,
                        )
                      : ValueOrErrors.Default.return({
                        ...serialized,
                        renderer: serialized.renderer,
                        entityName: serialized.entityName,
                        detailsRenderer: "detailsRenderer" in serialized ? serialized.detailsRenderer : undefined,
                        previewRenderer: "previewRenderer" in serialized ? serialized.previewRenderer : undefined,
                      })
      },
    DeserializePreviewRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      type: ReferenceOneType<T>,
      serialized: SerializedReferenceOneRenderer,
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
            type.previewType,
            serialized.previewRenderer,
            concreteRenderers,
            "ReferenceOne preview renderer",
            types,
            forms,
            alreadyParsedForms,
          ),
    DeserializeDetailsRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      type: ReferenceOneType<T>,
      serialized: SerializedReferenceOneRenderer,
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
      serialized.detailsRenderer == undefined
        ? ValueOrErrors.Default.return<
            [NestedRenderer<T> | undefined, Map<string, Renderer<T>>],
            string
          >([undefined, alreadyParsedForms])
        : NestedRenderer.Operations.DeserializeAs(
            type.detailsType,
            serialized.detailsRenderer,
            concreteRenderers,
            "ReferenceOne details renderer",
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
      type: ReferenceOneType<T>,
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
    ): ValueOrErrors<[ReferenceOneRenderer<T>, Map<string, Renderer<T>>], string> =>
      ReferenceOneRenderer.Operations.tryAsValidReferenceOneRenderer(serialized).Then(
        (validatedSerialized) => {
          return ReferenceOneRenderer.Operations.DeserializeDetailsRenderer(
            type,
            validatedSerialized,
            concreteRenderers,
            types,
            forms,
            alreadyParsedForms,
          ).Then(([detailsRenderer, detailsAlreadyParsedForms]) => {
            return ReferenceOneRenderer.Operations.DeserializePreviewRenderer(
              type,
              validatedSerialized,
              concreteRenderers,
              types,
              forms,
              detailsAlreadyParsedForms,
            ).Then(([previewRenderer, previewAlreadyParsedForms]) =>
              ValueOrErrors.Default.return<
                [ReferenceOneRenderer<T>, Map<string, Renderer<T>>],
                string
              >([
                ReferenceOneRenderer.Default(
                  type,
                  validatedSerialized.renderer,
                  validatedSerialized.entityName,
                  detailsRenderer, 
                  previewRenderer,
                ),
                previewAlreadyParsedForms,
              ]),
            )
          })
      }),
  },
};
