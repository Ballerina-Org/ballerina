import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  ListType,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { NestedRenderer } from "../nestedRenderer/state";

export type SerializedListRenderer = {
  renderer: string;
  elementRenderer: unknown;
};

export type ListRenderer<T> = {
  kind: "listRenderer";
  concreteRenderer: string;
  elementRenderer: NestedRenderer<T>;
  type: ListType<T>;
};

export const ListRenderer = {
  Default: <T>(
    type: ListType<T>,
    concreteRenderer: string,
    elementRenderer: NestedRenderer<T>,
  ): ListRenderer<T> => ({
    kind: "listRenderer",
    type,
    concreteRenderer,
    elementRenderer,
  }),
  Operations: {
    hasRenderers: (
      serialized: unknown,
    ): serialized is object & {
      renderer: string;
      elementRenderer: unknown;
    } =>
      isObject(serialized) &&
      "renderer" in serialized &&
      "elementRenderer" in serialized,
    tryAsValidBaseListRenderer: <T>(
      serialized: unknown,
      type: DispatchParsedType<T>,
    ): ValueOrErrors<
      Omit<SerializedListRenderer, "renderer" | "elementRenderer"> & {
        renderer: string;
        elementRenderer: unknown;
      },
      string
    > =>
      type.kind != "list"
        ? ValueOrErrors.Default.throwOne(`type ${type.kind} is not a list`)
        : !ListRenderer.Operations.hasRenderers(serialized)
          ? ValueOrErrors.Default.throwOne(
              `renderer and elementRenderer are required`,
            )
          : ValueOrErrors.Default.return({
              renderer: serialized.renderer,
              elementRenderer: serialized.elementRenderer,
            }),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      type: ListType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts
      >,
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
            ValueOrErrors.Default.return(
              ListRenderer.Default(
                type,
                serializedRenderer.renderer,
                elementRenderer,
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as List`),
        ),
  },
};
