import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  ReadOnlyType,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { NestedRenderer } from "../nestedRenderer/state";

export type SerializedReadOnlyRenderer = {
  renderer: string;
  childRenderer: unknown;
};

export type ReadOnlyRenderer<T> = {
  kind: "readOnlyRenderer";
  concreteRenderer: string;
  childRenderer: NestedRenderer<T>;
  type: ReadOnlyType<T>;
};

export const ReadOnlyRenderer = {
  Default: <T>(
    type: ReadOnlyType<T>,
    concreteRenderer: string,
    childRenderer: NestedRenderer<T>,
  ): ReadOnlyRenderer<T> => ({
    kind: "readOnlyRenderer",
    type,
    concreteRenderer,
    childRenderer,
  }),
  Operations: {
    hasRenderers: (
      serialized: unknown,
    ): serialized is object & {
      renderer: string;
      childRenderer: unknown;
    } =>
      isObject(serialized) &&
      "renderer" in serialized &&
      "childRenderer" in serialized,
    tryAsValidBaseReadOnlyRenderer: <T>(
      serialized: unknown,
      type: DispatchParsedType<T>,
    ): ValueOrErrors<
      Omit<SerializedReadOnlyRenderer, "renderer" | "childRenderer"> & {
        renderer: string;
        childRenderer: unknown;
      },
      string
    > =>
      type.kind != "readOnly"
        ? ValueOrErrors.Default.throwOne(`type ${type.kind} is not a readOnly`)
        : !ReadOnlyRenderer.Operations.hasRenderers(serialized)
          ? ValueOrErrors.Default.throwOne(
              `renderer and childRenderer are required`,
            )
          : ValueOrErrors.Default.return({
              renderer: serialized.renderer,
              childRenderer: serialized.childRenderer,
            }),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: ReadOnlyType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<ReadOnlyRenderer<T>, string> =>
      ReadOnlyRenderer.Operations.tryAsValidBaseReadOnlyRenderer(
        serialized,
        type,
      )
        .Then((serializedRenderer) =>
          NestedRenderer.Operations.DeserializeAs(
            type.arg,
            serializedRenderer.childRenderer,
            concreteRenderers,
            "readOnly child",
            types,
          ).Then((childRenderer) =>
            ValueOrErrors.Default.return(
              ReadOnlyRenderer.Default(
                type,
                serializedRenderer.renderer,
                childRenderer,
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as ReadOnly`),
        ),
  },
};
