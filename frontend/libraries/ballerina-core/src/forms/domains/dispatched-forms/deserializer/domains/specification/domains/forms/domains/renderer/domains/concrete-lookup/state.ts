import {
  DispatchInjectablesTypes,
  DispatchParsedType,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";

export type SerializedLookupRenderer = string;

export type ConcreteLookupRenderer<T> = {
  kind: "concreteLookupRenderer";
  renderer: string;
  type: DispatchParsedType<T>;
};

export const ConcreteLookupRenderer = {
  Default: <T extends DispatchInjectablesTypes<T>>(
    renderer: string,
    type: DispatchParsedType<T>,
    api?: string | string[],
  ): ConcreteLookupRenderer<T> => ({
    kind: "concreteLookupRenderer",
    renderer,
    type,
  }),
  Operations: {
    Deserialize: <T extends DispatchInjectablesTypes<T>>(
      type: DispatchParsedType<T>,
      serialized: SerializedLookupRenderer,
    ): ValueOrErrors<ConcreteLookupRenderer<T>, string> =>
      ValueOrErrors.Default.return(
        ConcreteLookupRenderer.Default(serialized, type),
      ),
  },
};
