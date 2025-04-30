import { Map } from "immutable";
import {
  Expr,
  TableFormRenderer,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import { DispatchParsedType, MapType } from "../../../../../../../types/state";
import {
  BaseBaseRenderer,
  BaseSerializedBaseRenderer,
  SerializedBaseRenderer,
  ParentContext,
  BaseRenderer,
} from "../../state";
import { RecordFormRenderer } from "../../../recordFormRenderer/state";

export type SerializedBaseMapRenderer = {
  keyRenderer?: unknown;
  valueRenderer?: unknown;
} & BaseSerializedBaseRenderer;

export type BaseMapRenderer<T> = BaseBaseRenderer & {
  kind: "baseMapRenderer";
  keyRenderer: BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>;
  valueRenderer: BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>;
  type: MapType<T>;
  concreteRendererName: string;
};

export const BaseMapRenderer = {
  Default: <T>(
    type: MapType<T>,
    concreteRendererName: string,
    keyRenderer: BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>,
    valueRenderer:
      | BaseRenderer<T>
      | TableFormRenderer<T>
      | RecordFormRenderer<T>,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseMapRenderer<T> => ({
    kind: "baseMapRenderer",
    type,
    concreteRendererName,
    keyRenderer,
    valueRenderer,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderers: (
      serialized: SerializedBaseMapRenderer,
    ): serialized is SerializedBaseMapRenderer & {
      renderer: string;
      keyRenderer: SerializedBaseRenderer;
      valueRenderer: SerializedBaseRenderer;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string" &&
      serialized.keyRenderer != undefined &&
      serialized.valueRenderer != undefined,
    tryAsValidMapBaseRenderer: (
      serialized: SerializedBaseMapRenderer,
    ): ValueOrErrors<
      Omit<
        SerializedBaseMapRenderer,
        "renderer" | "keyRenderer" | "valueRenderer"
      > & {
        renderer: string;
        keyRenderer: SerializedBaseRenderer;
        valueRenderer: SerializedBaseRenderer;
      },
      string
    > =>
      !BaseMapRenderer.Operations.hasRenderers(serialized)
        ? ValueOrErrors.Default.throwOne(
            `renderer, keyRenderer and valueRenderer are required`,
          )
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: MapType<T>,
      serialized: SerializedBaseMapRenderer,
      fieldViews: any,
      renderingContext: ParentContext,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<BaseMapRenderer<T>, string> =>
      BaseMapRenderer.Operations.tryAsValidMapBaseRenderer(serialized)
        .Then((renderer) =>
          BaseRenderer.Operations.ComputeVisibility(
            renderer.visible,
            renderingContext,
          ).Then((visibilityExpr) =>
            BaseRenderer.Operations.ComputeDisabled(
              renderer.disabled,
              renderingContext,
            ).Then((disabledExpr) =>
              BaseRenderer.Operations.DeserializeAs(
                type.args[0],
                renderer.keyRenderer,
                fieldViews,
                "nested",
                "Key",
                types,
              ).Then((deserializedKeyRenderer) =>
                BaseRenderer.Operations.DeserializeAs(
                  type.args[1],
                  renderer.valueRenderer,
                  fieldViews,
                  "nested",
                  "Value",
                  types,
                ).Then((deserializedValueRenderer) =>
                  ValueOrErrors.Default.return(
                    BaseMapRenderer.Default(
                      type,
                      renderer.renderer,
                      deserializedKeyRenderer,
                      deserializedValueRenderer,
                      visibilityExpr,
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
          errors.map((error) => `${error}\n...When parsing as Map`),
        ),
  },
};
