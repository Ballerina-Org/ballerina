import {
  Expr,
  TableType,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import {
  BaseSerializedBaseRenderer,
  ParentContext,
  BaseRenderer,
  BaseBaseRenderer,
} from "../../state";

export type SerializedBaseTableRenderer = {
  api?: unknown;
} & BaseSerializedBaseRenderer;

export type BaseTableRenderer<T> = BaseBaseRenderer & {
  kind: "baseTableRenderer";
  api: string;
  type: TableType<T>;
  lookupRendererName: string;
};

export const BaseTableRenderer = {
  Default: <T>(
    type: TableType<T>,
    lookupRendererName: string,
    api: string,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseTableRenderer<T> => ({
    kind: "baseTableRenderer",
    type,
    lookupRendererName,
    api,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    tryAsValidBaseTableRenderer: <T>(
      serialized: SerializedBaseTableRenderer,
    ): ValueOrErrors<
      Omit<SerializedBaseTableRenderer, "renderer" | "api"> & {
        renderer: string;
        api: string;
      },
      string
    > => {
      const api = serialized.api;
      const renderer = serialized.renderer;
      return api == undefined
        ? ValueOrErrors.Default.throwOne(`api is missing`)
        : typeof api != "string"
          ? ValueOrErrors.Default.throwOne(`api must be a string`)
          : renderer == undefined
            ? ValueOrErrors.Default.throwOne(`renderer is missing`)
            : typeof renderer != "string"
              ? ValueOrErrors.Default.throwOne(`renderer must be a string`)
              : ValueOrErrors.Default.return({
                  ...serialized,
                  renderer,
                  api,
                });
    },
    Deserialize: <T>(
      type: TableType<T>,
      serialized: SerializedBaseTableRenderer,
      renderingContext: ParentContext,
    ): ValueOrErrors<BaseTableRenderer<T>, string> =>
      BaseTableRenderer.Operations.tryAsValidBaseTableRenderer(serialized).Then(
        (renderer) =>
          BaseRenderer.Operations.ComputeVisibility(
            renderer.visible,
            renderingContext,
          ).Then((visibilityExpr) =>
            BaseRenderer.Operations.ComputeDisabled(
              renderer.disabled,
              renderingContext,
            ).Then((disabledExpr) =>
              ValueOrErrors.Default.return(
                BaseTableRenderer.Default(
                  type,
                  renderer.renderer,
                  renderer.api,
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
  },
};
