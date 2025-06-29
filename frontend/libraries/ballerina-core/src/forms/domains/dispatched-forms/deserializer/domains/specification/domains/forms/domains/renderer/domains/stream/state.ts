import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  isString,
  Unit,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import {
  MultiSelectionType,
  SingleSelectionType,
} from "../../../../../types/state";
import { Renderer } from "../../state";

export type SerializedStreamRenderer = {
  renderer: string;
  stream: string;
};

export type StreamRenderer<T> = {
  kind: "streamRenderer";
  concreteRenderer: string;
  stream: string;
  type: SingleSelectionType<T> | MultiSelectionType<T>;
};

export const StreamRenderer = {
  Default: <T>(
    type: SingleSelectionType<T> | MultiSelectionType<T>,
    stream: string,
    concreteRenderer: string,
  ): StreamRenderer<T> => ({
    kind: "streamRenderer",
    type,
    stream,
    concreteRenderer,
  }),
  Operations: {
    tryAsValidStreamBaseRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedStreamRenderer, string> =>
      !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(`stream renderer is not an object`)
        : !("renderer" in serialized)
          ? ValueOrErrors.Default.throwOne(`renderer is required`)
          : !isString(serialized.renderer)
            ? ValueOrErrors.Default.throwOne(`renderer must be a string`)
            : !("stream" in serialized)
              ? ValueOrErrors.Default.throwOne(`stream is required`)
              : !isString(serialized.stream)
                ? ValueOrErrors.Default.throwOne(`stream must be a string`)
                : ValueOrErrors.Default.return({
                    renderer: serialized.renderer,
                    stream: serialized.stream,
                  }),
    Deserialize: <T extends DispatchInjectablesTypes<T>>(
      type: SingleSelectionType<T> | MultiSelectionType<T>,
      serialized: unknown,
    ): ValueOrErrors<StreamRenderer<T>, string> =>
      StreamRenderer.Operations.tryAsValidStreamBaseRenderer(serialized)
        .Then((validatedSerialized) =>
          ValueOrErrors.Default.return(
            StreamRenderer.Default(
              type,
              validatedSerialized.stream,
              validatedSerialized.renderer,
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Stream renderer`),
        ),
  },
};
