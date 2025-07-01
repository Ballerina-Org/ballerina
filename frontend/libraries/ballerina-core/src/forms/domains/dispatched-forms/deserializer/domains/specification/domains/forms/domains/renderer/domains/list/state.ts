import { List, Map } from "immutable";
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
  actions: unknown;
};

export const ListMethod = {
  add: "add",
  remove: "remove",
  move: "move",
  duplicate: "duplicate",
} as const;
export type ListMethod = (typeof ListMethod)[keyof typeof ListMethod];

export type ListMethods = Array<ListMethod>;
export const ListMethods = {
  Operations: {
    fromRawValue: (value: unknown): ValueOrErrors<ListMethods, string> =>
      typeof value === "undefined"
        ? ValueOrErrors.Default.return([] as ListMethods)
        : typeof value !== "object" || value == null
          ? ValueOrErrors.Default.throwOne(
              `expected an object for list methods, got ${typeof value}`,
            )
          : Object.keys(value).length == 0
            ? ValueOrErrors.Default.return([] as ListMethods)
            : Object.keys(value).find(
                  (_) => !ListMethod[_ as keyof typeof ListMethod],
                )
              ? ValueOrErrors.Default.throwOne(
                  `expected an object with keys ${Object.keys(ListMethod).join(
                    ", ",
                  )}, got ${Object.keys(value).join(", ")}`,
                )
              : ValueOrErrors.Default.return(Object.keys(value) as ListMethods),
  },
};

export type ListRenderer<T> = {
  kind: "listRenderer";
  renderer: Renderer<T>;
  elementRenderer: NestedRenderer<T>;
  type: ListType<T>;
  methods: Array<ListMethod>;
};

export const ListRenderer = {
  Default: <T>(
    type: ListType<T>,
    renderer: Renderer<T>,
    elementRenderer: NestedRenderer<T>,
    methods?: ListRenderer<T>["methods"],
  ): ListRenderer<T> => ({
    kind: "listRenderer",
    type,
    renderer,
    elementRenderer,
    methods: methods ?? [],
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
              ListMethods.Operations.fromRawValue(
                serializedRenderer.actions,
              ).Then((methods) =>
                ValueOrErrors.Default.return(
                  ListRenderer.Default(
                    type,
                    renderer,
                    elementRenderer,
                    methods,
                  ),
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as List`),
        ),
  },
};
