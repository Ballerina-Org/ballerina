import { type } from "node:os";
import { DispatchParsedType, ValueOrErrors } from "../../../../../../../../../../../../../main";
import { LookupType } from "../../../../../../../../../../../../../main";

export type SerializedLookupRenderer = string;

export type LookupRenderer<T> = {
  kind: "lookupRenderer";
  renderer: string;
  type: DispatchParsedType<T>;
};

export const LookupRenderer = {
  Default: <T>(
    renderer: string,
    type: DispatchParsedType<T>,
  ): LookupRenderer<T> => ({
    kind: "lookupRenderer",
    renderer,
    type,
  }),
  Operations: {
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      serialized: SerializedLookupRenderer,
    ): ValueOrErrors<LookupRenderer<T>, string> =>
      ValueOrErrors.Default.return(LookupRenderer.Default(serialized, type)),
  },
};
