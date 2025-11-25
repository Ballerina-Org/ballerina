import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  ListType,
  Renderer,
  SpecVersion,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { NestedRenderer } from "../nestedRenderer/state";

export type SerializedListRenderer = {
  renderer: string;
  elementRenderer: unknown;
  actions: unknown;
};

export const ListMethod = {
  add: "add",
  remove: "remove",
  removeAll: "removeAll",
  move: "move",
  duplicate: "duplicate",
} as const;
export type ListMethod = (typeof ListMethod)[keyof typeof ListMethod];

export type ListMethods = Array<ListMethod>;
export const ListMethods = {
  Operations: {
    fromRawValue: (value: unknown): ValueOrErrors<ListMethods, string> =>
      typeof value === "undefined"
        ? ValueOrErrors.Default.return([])
        : typeof value !== "object" || value == null
          ? ValueOrErrors.Default.throwOne(
              `expected an object for list methods, got ${typeof value}`,
            )
          : Object.keys(value).length == 0
            ? ValueOrErrors.Default.return([])
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
  concreteRenderer: string;
  elementRenderer: NestedRenderer<T>;
  type: ListType<T>;
  methods: Array<ListMethod>;
};

export const ListRenderer = {
  Default: <T>(
    type: ListType<T>,
    concreteRenderer: string,
    elementRenderer: NestedRenderer<T>,
    methods?: ListRenderer<T>["methods"],
  ): ListRenderer<T> => ({
    kind: "listRenderer",
    type,
    concreteRenderer,
    elementRenderer,
    methods: methods ?? [],
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
              actions: "actions" in serialized ? serialized.actions : [],
              renderer: serialized.renderer,
              elementRenderer: serialized.elementRenderer,
            }),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      type: ListType<T>,
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
    ): ValueOrErrors<[ListRenderer<T>, Map<string, Renderer<T>>], string> =>
      ListRenderer.Operations.tryAsValidBaseListRenderer(serialized, type)
        .Then((serializedRenderer) =>
          NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            serializedRenderer.elementRenderer,
            concreteRenderers,
            "list element",
            types,
            forms,
            alreadyParsedForms,
            specVersionContext,
          ).Then(([elementRenderer, newAlreadyParsedForms]) =>
            ListMethods.Operations.fromRawValue(
              serializedRenderer.actions,
            ).Then((methods) =>
              ValueOrErrors.Default.return<
                [ListRenderer<T>, Map<string, Renderer<T>>],
                string
              >([
                ListRenderer.Default(
                  type,
                  serializedRenderer.renderer,
                  elementRenderer,
                  methods,
                ),
                newAlreadyParsedForms,
              ]),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as List`),
        ),
  },
};
