import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import { LookupType } from "../../../../../../../types/state";
import {
  BaseBaseRenderer,
  BaseSerializedBaseRenderer,
  ParentContext,
  BaseRenderer,
} from "../../state";

export type SerializedBaseLookupRenderer = BaseSerializedBaseRenderer;

export type BaseLookupRenderer<T> = BaseBaseRenderer & {
  kind: "baseLookupRenderer";
  type: LookupType;
  lookupRendererName: string;
};

export const BaseLookupRenderer = {
  Default: <T>(
    type: LookupType,
    lookupRendererName: string,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseLookupRenderer<T> => ({
    kind: "baseLookupRenderer",
    type,
    lookupRendererName,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderer: (
      serialized: SerializedBaseLookupRenderer,
    ): serialized is SerializedBaseLookupRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",
    tryAsValidLookupBaseRenderer: (
      serialized: SerializedBaseLookupRenderer,
    ): ValueOrErrors<
      Omit<SerializedBaseLookupRenderer, "renderer"> & {
        renderer: string;
      },
      string
    > =>
      !BaseLookupRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(`renderer is required`)
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: LookupType,
      serialized: SerializedBaseLookupRenderer,
      renderingContext: ParentContext,
    ): ValueOrErrors<BaseLookupRenderer<T>, string> =>
      BaseLookupRenderer.Operations.tryAsValidLookupBaseRenderer(serialized)
        .Then((renderer) =>
          BaseRenderer.Operations.ComputeVisibility(
            renderer.visible,
            renderingContext,
          ).Then((visibilityExpr) =>
            BaseRenderer.Operations.ComputeDisabled(
              renderer.disabled,
              renderingContext,
            ).Then((disabledExpr) =>
              ValueOrErrors.Default.return(
                BaseLookupRenderer.Default(
                  type,
                  renderer.renderer,
                  visibilityExpr,
                  disabledExpr,
                  renderer.label,
                  renderer.tooltip,
                  renderer.details,
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Lookup`),
        ),
  },
};
