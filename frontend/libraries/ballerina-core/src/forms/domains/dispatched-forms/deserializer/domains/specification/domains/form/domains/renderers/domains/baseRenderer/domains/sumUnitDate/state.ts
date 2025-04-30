import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import { SumType } from "../../../../../../../types/state";
import {
  BaseSerializedBaseRenderer,
  BaseBaseRenderer,
  BaseRenderer,
  ParentContext,
} from "../../state";

export type SerializedSumUnitDateBaseRenderer = BaseSerializedBaseRenderer;

export type BaseSumUnitDateRenderer<T> = BaseBaseRenderer & {
  kind: "baseSumUnitDateRenderer";
  type: SumType<T>;
  concreteRendererName: string;
};

export const BaseSumUnitDateRenderer = {
  Default: <T>(
    type: SumType<T>,
    concreteRendererName: string,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseSumUnitDateRenderer<T> => ({
    kind: "baseSumUnitDateRenderer",
    type,
    concreteRendererName,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderer: (
      serialized: SerializedSumUnitDateBaseRenderer,
    ): serialized is SerializedSumUnitDateBaseRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",
    tryAsValidSumUnitDateBaseRenderer: (
      serialized: SerializedSumUnitDateBaseRenderer,
    ): ValueOrErrors<
      Omit<SerializedSumUnitDateBaseRenderer, "renderer"> & {
        renderer: string;
      },
      string
    > =>
      !BaseSumUnitDateRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(`renderer is required`)
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SumType<T>,
      serialized: SerializedSumUnitDateBaseRenderer,
      renderingContext: ParentContext,
    ): ValueOrErrors<BaseSumUnitDateRenderer<T>, string> =>
      BaseSumUnitDateRenderer.Operations.tryAsValidSumUnitDateBaseRenderer(
        serialized,
      )
        .Then((sumUnitDateRenderer) =>
          BaseRenderer.Operations.ComputeVisibility(
            sumUnitDateRenderer.visible,
            renderingContext,
          ).Then((visible) =>
            BaseRenderer.Operations.ComputeDisabled(
              sumUnitDateRenderer.disabled,
              renderingContext,
            ).Then((disabled) =>
              ValueOrErrors.Default.return(
                BaseSumUnitDateRenderer.Default(
                  type,
                  sumUnitDateRenderer.renderer,
                  visible,
                  disabled,
                  sumUnitDateRenderer.label,
                  sumUnitDateRenderer.tooltip,
                  sumUnitDateRenderer.details,
                ),
              ),
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
