import { DispatchParsedType, ValueOrErrors } from "../../../../../../../../../../../../../main";
import { LookupType } from "../../../../../../../../../../../../../main";

export type SerializedLookupRenderer = string;

export type LookupRenderer<T> = {
  kind: "lookupRenderer";
  lookupRendererName: string;
  lookupRendererType: DispatchParsedType<T>;
};

export const LookupRenderer = {
  Default: <T>(
    lookupRendererName: string,
    lookupRendererType: DispatchParsedType<T>,
  ): LookupRenderer<T> => ({
    kind: "lookupRenderer",
    lookupRendererName,
    lookupRendererType,
  }),
  Operations: {
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      serialized: SerializedLookupRenderer,
    ): ValueOrErrors<LookupRenderer<T>, string> =>
      ValueOrErrors.Default.return(LookupRenderer.Default(serialized, type)),
  },
};
