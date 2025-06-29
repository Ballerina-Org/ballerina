import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  Guid,
  isObject,
  isString,
  Unit,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import {
  MultiSelectionType,
  SingleSelectionType,
} from "../../../../../types/state";
import { Renderer, SerializedRenderer } from "../../state";

export type SerializedEnumRenderer = {
  renderer: string;
  options: string;
};

export type EnumRenderer<T> = {
  kind: "enumRenderer";
  concreteRenderer: string;
  options: Guid;
  type: SingleSelectionType<T> | MultiSelectionType<T>;
};

export const EnumRenderer = {
  Default: <T extends DispatchInjectablesTypes<T>>(
    type: SingleSelectionType<T> | MultiSelectionType<T>,
    options: string,
    concreteRenderer: string,
  ): EnumRenderer<T> => ({
    kind: "enumRenderer",
    type,
    options,
    concreteRenderer,
  }),
  Operations: {
    hasRenderer: (
      serialized: unknown,
    ): serialized is object & {
      renderer: SerializedRenderer;
    } => isObject(serialized) && "renderer" in serialized,
    hasOptions: (
      serialized: unknown,
    ): serialized is object & {
      options: string;
    } => isObject(serialized) && "options" in serialized,
    tryAsValidEnumRenderer: <T extends DispatchInjectablesTypes<T>>(
      serialized: unknown,
    ): ValueOrErrors<SerializedEnumRenderer, string> =>
      !EnumRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(`renderer is required`)
        : !isString(serialized.renderer)
          ? ValueOrErrors.Default.throwOne(`renderer must be a string`)
          : !EnumRenderer.Operations.hasOptions(serialized)
            ? ValueOrErrors.Default.throwOne(`options are required`)
            : !isString(serialized.options)
              ? ValueOrErrors.Default.throwOne(`options must be a string`)
              : ValueOrErrors.Default.return({
                  renderer: serialized.renderer,
                  options: serialized.options,
                }),
    Deserialize: <T extends DispatchInjectablesTypes<T>>(
      type: SingleSelectionType<T> | MultiSelectionType<T>,
      serialized: unknown,
    ): ValueOrErrors<EnumRenderer<T>, string> =>
      EnumRenderer.Operations.tryAsValidEnumRenderer(serialized)
        .Then((serialized) =>
          ValueOrErrors.Default.return(
            EnumRenderer.Default(type, serialized.options, serialized.renderer),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as EnumRenderer`),
        ),
  },
};
