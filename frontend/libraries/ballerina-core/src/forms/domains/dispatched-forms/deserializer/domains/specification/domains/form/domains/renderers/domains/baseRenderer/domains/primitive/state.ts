import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import { DispatchPrimitiveType } from "../../../../../../../types/state";
import {
  BaseBaseRenderer,
  BaseSerializedBaseRenderer,
  BaseRenderer,
  ParentContext,
} from "../../state";

export type SerializedPrimitiveBaseRenderer = BaseSerializedBaseRenderer;

export type BasePrimitiveRenderer<T> = BaseBaseRenderer & {
  kind: "basePrimitiveRenderer";
  type: DispatchPrimitiveType<T>;
  concreteRendererName: string;
};

export const BasePrimitiveRenderer = {
  Default: <T>(
    type: DispatchPrimitiveType<T>,
    concreteRendererName: string,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BasePrimitiveRenderer<T> => ({
    kind: "basePrimitiveRenderer",
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
      serialized: SerializedPrimitiveBaseRenderer,
    ): serialized is SerializedPrimitiveBaseRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",
    tryAsValidPrimitiveBaseRenderer: (
      serialized: SerializedPrimitiveBaseRenderer,
    ): ValueOrErrors<
      Omit<SerializedPrimitiveBaseRenderer, "renderer"> & {
        renderer: string;
      },
      string
    > =>
      !BasePrimitiveRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(`renderer is required`)
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: DispatchPrimitiveType<T>,
      serialized: SerializedPrimitiveBaseRenderer,
      renderingContext: ParentContext,
    ): ValueOrErrors<BasePrimitiveRenderer<T>, string> =>
      BasePrimitiveRenderer.Operations.tryAsValidPrimitiveBaseRenderer(
        serialized,
      )
        .Then((primitiveRecordFieldRenderer) =>
          BaseRenderer.Operations.ComputeVisibility(
            primitiveRecordFieldRenderer.visible,
            renderingContext,
          ).Then((visibilityExpr) =>
            BaseRenderer.Operations.ComputeDisabled(
              primitiveRecordFieldRenderer.disabled,
              renderingContext,
            ).Then((disabledExpr) =>
              ValueOrErrors.Default.return(
                BasePrimitiveRenderer.Default(
                  type,
                  primitiveRecordFieldRenderer.renderer,
                  visibilityExpr,
                  disabledExpr,
                  primitiveRecordFieldRenderer.label,
                  primitiveRecordFieldRenderer.tooltip,
                  primitiveRecordFieldRenderer.details,
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Primitive`),
        ),
  },
};
