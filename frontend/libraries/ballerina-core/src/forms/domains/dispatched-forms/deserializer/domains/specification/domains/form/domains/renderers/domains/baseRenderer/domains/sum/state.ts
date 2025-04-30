import { Map } from "immutable";
import {
  SerializedBaseRenderer,
  BaseSerializedBaseRenderer,
  BaseRenderer,
  BaseBaseRenderer as BaseBaseRenderer,
  ParentContext,
} from "../../state";
import { DispatchParsedType, SumType } from "../../../../../../../types/state";
import {
  Expr,
  TableFormRenderer,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import { RecordFormRenderer } from "../../../recordFormRenderer/state";

export type SerializedSumBaseRenderer = {
  leftRenderer?: unknown;
  rightRenderer?: unknown;
} & BaseSerializedBaseRenderer;

export type BaseSumRenderer<T> = BaseBaseRenderer & {
  kind: "baseSumRenderer";
  leftRenderer: BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>;
  rightRenderer: BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>;
  type: SumType<T>;
  concreteRendererName: string;
};

export const BaseSumRenderer = {
  Default: <T>(
    type: SumType<T>,
    concreteRendererName: string,
    leftRenderer:
      | BaseRenderer<T>
      | TableFormRenderer<T>
      | RecordFormRenderer<T>,
    rightRenderer:
      | BaseRenderer<T>
      | TableFormRenderer<T>
      | RecordFormRenderer<T>,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseSumRenderer<T> => ({
    kind: "baseSumRenderer",
    type,
    concreteRendererName,
    leftRenderer,
    rightRenderer,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderers: (
      serialized: SerializedSumBaseRenderer,
    ): serialized is SerializedSumBaseRenderer & {
      renderer: string;
      leftRenderer: SerializedBaseRenderer;
      rightRenderer: SerializedBaseRenderer;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string" &&
      serialized.leftRenderer != undefined &&
      serialized.rightRenderer != undefined,
    tryAsValidSumBaseRenderer: (
      serialized: SerializedSumBaseRenderer,
    ): ValueOrErrors<
      Omit<
        SerializedSumBaseRenderer,
        "renderer" | "leftRenderer" | "rightRenderer"
      > & {
        renderer: string;
        leftRenderer: SerializedBaseRenderer;
        rightRenderer: SerializedBaseRenderer;
      },
      string
    > =>
      !BaseSumRenderer.Operations.hasRenderers(serialized)
        ? ValueOrErrors.Default.throwOne(
            `renderer, leftRenderer and rightRenderer are required`,
          )
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SumType<T>,
      serialized: SerializedSumBaseRenderer,
      renderingContext: ParentContext,
      fieldViews: any,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<BaseSumRenderer<T>, string> =>
      BaseSumRenderer.Operations.tryAsValidSumBaseRenderer(serialized)
        .Then((renderer) =>
          BaseRenderer.Operations.ComputeVisibility(
            renderer.visible,
            renderingContext,
          ).Then((visibleExpr) =>
            BaseRenderer.Operations.ComputeDisabled(
              renderer.disabled,
              renderingContext,
            ).Then((disabledExpr) =>
              BaseRenderer.Operations.DeserializeAs(
                type.args[0],
                renderer.leftRenderer,
                fieldViews,
                "nested",
                "Left",
                types,
              ).Then((deserializedLeftRenderer) =>
                BaseRenderer.Operations.DeserializeAs(
                  type.args[1],
                  renderer.rightRenderer,
                  fieldViews,
                  "nested",
                  "Right",
                  types,
                ).Then((deserializedRightRenderer) =>
                  ValueOrErrors.Default.return(
                    BaseSumRenderer.Default(
                      type,
                      renderer.renderer,
                      deserializedLeftRenderer,
                      deserializedRightRenderer,
                      visibleExpr,
                      disabledExpr,
                      renderer.label,
                      renderer.tooltip,
                      renderer.details,
                    ),
                  ),
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Sum renderer`),
        ),
  },
};
