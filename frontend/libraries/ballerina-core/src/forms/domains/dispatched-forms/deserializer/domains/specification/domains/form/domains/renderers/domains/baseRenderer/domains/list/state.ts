import { Map } from "immutable";
import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import { DispatchParsedType, ListType } from "../../../../../../../types/state";
import {
  BaseBaseRenderer,
  BaseSerializedBaseRenderer,
  ParentContext,
  BaseRenderer,
} from "../../state";
import { TableFormRenderer } from "../../../tableFormRenderer/state";
import { RecordFormRenderer } from "../../../recordFormRenderer/state";

export type SerializedBaseListRenderer = {
  elementRenderer?: unknown;
  elementLabel?: string;
  elementTooltip?: string;
} & BaseSerializedBaseRenderer;

export type BaseListRenderer<T> = BaseBaseRenderer & {
  kind: "baseListRenderer";
  elementRenderer:
    | BaseRenderer<T>
    | TableFormRenderer<T>
    | RecordFormRenderer<T>;
  type: ListType<T>;
  concreteRendererName: string;
};

export const BaseListRenderer = {
  Default: <T>(
    type: ListType<T>,
    concreteRendererName: string,
    elementRenderer:
      | BaseRenderer<T>
      | TableFormRenderer<T>
      | RecordFormRenderer<T>,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseListRenderer<T> => ({
    kind: "baseListRenderer",
    type,
    concreteRendererName,
    elementRenderer,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderers: (
      serialized: SerializedBaseListRenderer,
    ): serialized is SerializedBaseListRenderer & {
      renderer: string;
      elementRenderer: string | object;
    } => {
      return (
        serialized.renderer != undefined &&
        typeof serialized.renderer == "string" &&
        serialized.elementRenderer != undefined
      );
    },
    tryAsValidBaseListRenderer: (
      serialized: SerializedBaseListRenderer,
    ): ValueOrErrors<
      Omit<SerializedBaseListRenderer, "renderer" | "elementRenderer"> & {
        renderer: string;
        elementRenderer: object;
      },
      string
    > => {
      if (!BaseListRenderer.Operations.hasRenderers(serialized))
        return ValueOrErrors.Default.throwOne(
          `renderer and elementRenderer are required`,
        );
      const elementRenderer = serialized.elementRenderer;
      // Backwards compatability
      if (typeof elementRenderer == "string")
        return ValueOrErrors.Default.return({
          renderer: serialized.renderer,
          label: serialized?.label,
          visible: serialized.visible,
          disabled: serialized?.disabled,
          details: serialized?.details,
          elementRenderer: {
            renderer: serialized.elementRenderer,
            label: serialized?.elementLabel,
            tooltip: serialized?.elementTooltip,
          },
        });

      return ValueOrErrors.Default.return({
        ...serialized,
        elementRenderer: elementRenderer,
      });
    },
    Deserialize: <T>(
      type: ListType<T>,
      serialized: SerializedBaseListRenderer,
      fieldViews: any,
      renderingContext: ParentContext,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<BaseListRenderer<T>, string> =>
      BaseListRenderer.Operations.tryAsValidBaseListRenderer(serialized)
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
                renderer.elementRenderer,
                fieldViews,
                "nested",
                "Element",
                types,
              ).Then((elementRenderer) =>
                ValueOrErrors.Default.return(
                  BaseListRenderer.Default(
                    type,
                    renderer.renderer,
                    elementRenderer,
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
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as List`),
        ),
  },
};
