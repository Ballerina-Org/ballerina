import { ValueOrErrors } from "../../../../../../../../../../../../../../../main";
import { BaseSerializedNestedRenderer, BaseNestedRenderer } from "../../state";
import { List } from "immutable";
import { LookupType } from "../../../../../../../types/state";
export type SerializedNestedLookupRenderer = BaseSerializedNestedRenderer;

export type NestedLookupRenderer<T> = BaseNestedRenderer & {
  kind: "nestedLookupRenderer";
  type: LookupType;
  lookupRendererName: string;
};

export const NestedLookupRenderer = {
  Default: <T>(
    type: LookupType,
    lookupRendererName: string,
  ): NestedLookupRenderer<T> => ({
    kind: "nestedLookupRenderer",
    lookupRendererName,
    type,
  }),
  Operations: {
    tryAsValidNestedLookupRenderer: (
      serialized: SerializedNestedLookupRenderer,
    ): ValueOrErrors<
      Omit<SerializedNestedLookupRenderer, "renderer"> & {
        renderer: string;
      },
      string
    > => {
      const renderer = serialized.renderer;
      if (renderer == undefined) {
        return ValueOrErrors.Default.throwOne(`renderer is missing`);
      }
      if (typeof renderer != "string") {
        return ValueOrErrors.Default.throwOne(`renderer must be a string`);
      }
      return ValueOrErrors.Default.return({
        ...serialized,
        renderer,
      });
    },
    Deserialize: <T>(
      type: LookupType,
      serialized: SerializedNestedLookupRenderer,
    ): ValueOrErrors<NestedLookupRenderer<T>, string> =>
      NestedLookupRenderer.Operations.tryAsValidNestedLookupRenderer(serialized)
        .Then((nestedLookupRenderer) =>
          ValueOrErrors.Default.return(
            NestedLookupRenderer.Default(type, nestedLookupRenderer.renderer),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Lookup renderer`),
        ),
  },
};
