import { List } from "immutable";
import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import {
  BaseBaseRenderer,
  BaseSerializedBaseRenderer,
  ParentContext,
  BaseRenderer,
} from "../../state";
import {
  MultiSelectionType,
  SingleSelectionType,
} from "../../../../../../../types/state";
export type SerializedStreamBaseRenderer = {
  stream?: string;
} & BaseSerializedBaseRenderer;

export type BaseStreamRenderer<T> = BaseBaseRenderer & {
  kind: "baseStreamRenderer";
  stream: string;
  type: SingleSelectionType<T> | MultiSelectionType<T>;
  concreteRendererName: string;
};

export const BaseStreamRenderer = {
  Default: <T>(
    type: SingleSelectionType<T> | MultiSelectionType<T>,
    stream: string,
    concreteRendererName: string,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseStreamRenderer<T> => ({
    kind: "baseStreamRenderer",
    type,
    stream,
    concreteRendererName,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderer: (
      serialized: SerializedStreamBaseRenderer,
    ): serialized is SerializedStreamBaseRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",
    hasStream: (
      serialized: SerializedStreamBaseRenderer,
    ): serialized is SerializedStreamBaseRenderer & {
      stream: string;
    } => serialized.stream != undefined && typeof serialized.stream == "string",
    tryAsValidStreamBaseRenderer: (
      serialized: SerializedStreamBaseRenderer,
    ): ValueOrErrors<
      Omit<SerializedStreamBaseRenderer, "renderer" | "stream"> & {
        renderer: string;
        stream: string;
      },
      string
    > =>
      !BaseStreamRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(`renderer is required`)
        : !BaseStreamRenderer.Operations.hasStream(serialized)
          ? ValueOrErrors.Default.throwOne(`stream is required`)
          : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SingleSelectionType<T> | MultiSelectionType<T>,
      serialized: SerializedStreamBaseRenderer,
      renderingContext: ParentContext,
    ): ValueOrErrors<BaseStreamRenderer<T>, string> =>
      BaseStreamRenderer.Operations.tryAsValidStreamBaseRenderer(serialized)
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
                BaseStreamRenderer.Default(
                  type,
                  renderer.stream,
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
          errors.map((error) => `${error}\n...When parsing as Stream renderer`),
        ),
  },
};
