import { Map } from "immutable";
import {
  ConcreteRendererKinds,
  DispatchParsedType,
  isObject,
  isString,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import {
  MultiSelectionType,
  SingleSelectionType,
} from "../../../../../types/state";
import { Renderer } from "../../state";

export type SerializedStreamRenderer = {
  renderer: unknown;
  stream: string;
};

export type StreamRenderer<T> = {
  kind: "streamRenderer";
  renderer: Renderer<T>;
  stream: string;
  type: SingleSelectionType<T> | MultiSelectionType<T>;
};

export const StreamRenderer = {
  Default: <T>(
    type: SingleSelectionType<T> | MultiSelectionType<T>,
    stream: string,
    renderer: Renderer<T>,
  ): StreamRenderer<T> => ({
    kind: "streamRenderer",
    type,
    stream,
    renderer,
  }),
  Operations: {
    tryAsValidStreamBaseRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedStreamRenderer, string> =>
      !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(`stream renderer is not an object`)
        : !("renderer" in serialized)
          ? ValueOrErrors.Default.throwOne(`renderer is required`)
          : !("stream" in serialized)
            ? ValueOrErrors.Default.throwOne(`stream is required`)
            : !isString(serialized.stream)
              ? ValueOrErrors.Default.throwOne(`stream must be a string`)
              : ValueOrErrors.Default.return({
                  ...serialized,
                  stream: serialized.stream,
                }),
    Deserialize: <T>(
      type: SingleSelectionType<T> | MultiSelectionType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<StreamRenderer<T>, string> =>
      StreamRenderer.Operations.tryAsValidStreamBaseRenderer(serialized)
        .Then((validatedSerialized) =>
          Renderer.Operations.Deserialize(
            type,
            validatedSerialized.renderer,
            concreteRenderers,
            types,
          ).Then((renderer) =>
            ValueOrErrors.Default.return(
              StreamRenderer.Default(
                type,
                validatedSerialized.stream,
                renderer,
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Stream renderer`),
        ),
  },
};
