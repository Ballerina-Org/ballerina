import { Map } from "immutable";
import {
  ConcreteRendererKinds,
  DispatchParsedType,
  isObject,
  ListType,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import {
  NestedRenderer,
  SerializedNestedRenderer,
} from "../nestedRenderer/state";
import { Renderer, SerializedRenderer } from "../../state";

export type SerializedListRenderer = {
  renderer: unknown;
  elementRenderer: unknown;
};

export type ListRenderer<T> = {
  kind: "listRenderer";
  renderer: Renderer<T>;
  elementRenderer: NestedRenderer<T>;
  type: ListType<T>;
};

export const ListRenderer = {
  Default: <T>(
    type: ListType<T>,
    renderer: Renderer<T>,
    elementRenderer: NestedRenderer<T>,
  ): ListRenderer<T> => ({
    kind: "listRenderer",
    type,
    renderer,
    elementRenderer,
  }),
  Operations: {
    hasRenderers: (
      serialized: unknown,
    ): serialized is SerializedListRenderer & {
      renderer: SerializedRenderer;
      elementRenderer: SerializedNestedRenderer;
    } =>
      isObject(serialized) &&
      "renderer" in serialized &&
      "elementRenderer" in serialized,
    tryAsValidBaseListRenderer: <T>(
      serialized: unknown,
      type: DispatchParsedType<T>,
    ): ValueOrErrors<
      Omit<SerializedListRenderer, "renderer" | "elementRenderer"> & {
        renderer: SerializedRenderer;
        elementRenderer: SerializedNestedRenderer;
      },
      string
    > =>
      type.kind != "list"
        ? ValueOrErrors.Default.throwOne(`type ${type.kind} is not a list`)
        : !ListRenderer.Operations.hasRenderers(serialized)
          ? ValueOrErrors.Default.throwOne(
              `renderer and elementRenderer are required`,
            )
          : ValueOrErrors.Default.return(serialized),

    Deserialize: <T>(
      type: ListType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds<T>, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<ListRenderer<T>, string> =>
      ListRenderer.Operations.tryAsValidBaseListRenderer(serialized, type)
        .Then((serializedRenderer) =>
          NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            serializedRenderer.elementRenderer,
            concreteRenderers,
            "list element",
            types,
          ).Then((elementRenderer) =>
            Renderer.Operations.Deserialize(
              type,
              serializedRenderer.renderer,
              concreteRenderers,
              types,
            ).Then((renderer) =>
              ValueOrErrors.Default.return(
                ListRenderer.Default(type, renderer, elementRenderer),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as List`),
        ),
  },
};
