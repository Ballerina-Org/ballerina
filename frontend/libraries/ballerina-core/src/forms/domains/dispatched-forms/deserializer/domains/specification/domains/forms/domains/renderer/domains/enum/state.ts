import { Map } from "immutable";
import {
  ConcreteRendererKinds,
  DispatchParsedType,
  Guid,
  isObject,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import {
  MultiSelectionType,
  SingleSelectionType,
} from "../../../../../types/state";
import { Renderer, SerializedRenderer } from "../../state";

export type SerializedEnumRenderer = {
  renderer: unknown;
  options: string;
};

export type EnumRenderer<T> = {
  kind: "enumRenderer";
  renderer: Renderer<T>;
  options: Guid;
  type: SingleSelectionType<T> | MultiSelectionType<T>;
};

export const EnumRenderer = {
  Default: <T>(
    type: SingleSelectionType<T> | MultiSelectionType<T>,
    options: string,
    renderer: Renderer<T>,
  ): EnumRenderer<T> => ({
    kind: "enumRenderer",
    type,
    options,
    renderer,
  }),
  Operations: {
    hasRenderer: (
      serialized: unknown,
    ): serialized is SerializedEnumRenderer & {
      renderer: SerializedRenderer;
    } => isObject(serialized) && "renderer" in serialized,
    hasOptions: (
      serialized: unknown,
    ): serialized is SerializedEnumRenderer & {
      options: string;
    } => isObject(serialized) && "options" in serialized,
    tryAsValidEnumRenderer: <T>(
      serialized: unknown,
    ): ValueOrErrors<SerializedEnumRenderer, string> =>
      !EnumRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(`renderer is required`)
        : !EnumRenderer.Operations.hasOptions(serialized)
          ? ValueOrErrors.Default.throwOne(`options are required`)
          : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SingleSelectionType<T> | MultiSelectionType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<EnumRenderer<T>, string> =>
      EnumRenderer.Operations.tryAsValidEnumRenderer(serialized)
        .Then((serialized) =>
          Renderer.Operations.Deserialize(
            type,
            serialized.renderer,
            concreteRenderers,
            types,
          ).Then((renderer) =>
            ValueOrErrors.Default.return(
              EnumRenderer.Default(type, serialized.options, renderer),
            ),
          ),
        )

        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as EnumRenderer`),
        ),
  },
};
