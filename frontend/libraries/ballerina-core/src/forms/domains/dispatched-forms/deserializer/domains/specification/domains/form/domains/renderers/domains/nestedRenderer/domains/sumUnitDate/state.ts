import { ValueOrErrors } from "../../../../../../../../../../../../../../../main";
import { SumType } from "../../../../../../../types/state";
import { BaseSerializedNestedRenderer, BaseNestedRenderer } from "../../state";
import { List } from "immutable";

export type SerializedNestedSumUnitDateRenderer = BaseSerializedNestedRenderer;

export type NestedSumUnitDateRenderer<T> = BaseNestedRenderer & {
  kind: "nestedSumUnitDateRenderer";
  type: SumType<T>;
  concreteRendererName: string;
};

export const NestedSumUnitDateRenderer = {
  Default: <T>(
    type: SumType<T>,
    concreteRendererName: string,
    label?: string,
    tooltip?: string,
    details?: string,
  ): NestedSumUnitDateRenderer<T> => ({
    kind: "nestedSumUnitDateRenderer",
    type,
    concreteRendererName,
    label,
    tooltip,
    details,
  }),
  Operations: {
    tryAsValidNestedSumUnitDateRenderer: (
      serialized: SerializedNestedSumUnitDateRenderer,
    ): ValueOrErrors<
      Omit<SerializedNestedSumUnitDateRenderer, "renderer"> & {
        renderer: string;
      },
      string
    > => {
      const renderer = serialized.renderer;
      if (renderer == undefined)
        return ValueOrErrors.Default.throwOne(`renderer is missing`);
      if (typeof renderer != "string")
        return ValueOrErrors.Default.throwOne(`renderer must be a string`);
      return ValueOrErrors.Default.return({
        ...serialized,
        renderer,
      });
    },
    Deserialize: <T>(
      type: SumType<T>,
      serialized: SerializedNestedSumUnitDateRenderer,
    ): ValueOrErrors<NestedSumUnitDateRenderer<T>, string> =>
      NestedSumUnitDateRenderer.Operations.tryAsValidNestedSumUnitDateRenderer(
        serialized,
      )
        .Then((sumUnitDateRenderer) =>
          ValueOrErrors.Default.return(
            NestedSumUnitDateRenderer.Default(
              type,
              sumUnitDateRenderer.renderer,
              sumUnitDateRenderer.label,
              sumUnitDateRenderer.tooltip,
              sumUnitDateRenderer.details,
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When parsing as SumUnitDate renderer`,
          ),
        ),
  },
};
