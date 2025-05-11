import { type } from "node:os";
import {
  DispatchParsedType,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { LookupType } from "../../../../../../../../../../../../../main";

export type SerializedLookupRenderer = string;

export type LookupRenderer<T> = {
  kind: "lookupRenderer";
  renderer: string;
  type: DispatchParsedType<T>;
  api?: string | string[];
};

export const LookupRenderer = {
  Default: <T>(
    renderer: string,
    type: DispatchParsedType<T>,
    api?: string | string[],
  ): LookupRenderer<T> => ({
    kind: "lookupRenderer",
    renderer,
    type,
    api,
  }),
  Operations: {
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      serialized: SerializedLookupRenderer,
      api?: string | string[],
    ): ValueOrErrors<LookupRenderer<T>, string> =>
      ValueOrErrors.Default.return(
        LookupRenderer.Default(serialized, type, api),
      ),
  },
};
