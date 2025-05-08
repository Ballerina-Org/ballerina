import { Map } from "immutable";

import {
  DispatchParsedType,
  Expr,
  isString,
  OneType,
  TableFormRenderer,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import { RecordFormRenderer } from "../../../recordFormRenderer/state";
import {
  BaseSerializedBaseRenderer,
  ParentContext,
  BaseRenderer,
  BaseBaseRenderer,
  SerializedBaseRenderer,
} from "../../state";

export type SerializedBaseOneRenderer = {
  api?: unknown;
  detailsRenderer?: unknown;
  previewRenderer?: unknown;
} & BaseSerializedBaseRenderer;

export type BaseOneRenderer<T> = BaseBaseRenderer & {
  kind: "baseOneRenderer";
  api: string | Array<string>;
  type: OneType<T>;
  concreteRendererName: string;
  detailsRenderer:
    | BaseRenderer<T>
    | TableFormRenderer<T>
    | RecordFormRenderer<T>;
  previewRenderer?:
    | BaseRenderer<T>
    | TableFormRenderer<T>
    | RecordFormRenderer<T>;
};

export const BaseOneRenderer = {
  Default: <T>(
    type: OneType<T>,
    concreteRendererName: string,
    api: string | Array<string>,
    detailsRenderer:
      | BaseRenderer<T>
      | TableFormRenderer<T>
      | RecordFormRenderer<T>,
    previewRenderer?:
      | BaseRenderer<T>
      | TableFormRenderer<T>
      | RecordFormRenderer<T>,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseOneRenderer<T> => ({
    kind: "baseOneRenderer",
    type,
    concreteRendererName,
    api,
    detailsRenderer,
    previewRenderer,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    tryAsValidBaseOneRenderer: <T>(
      serialized: SerializedBaseOneRenderer,
    ): ValueOrErrors<
      Omit<SerializedBaseOneRenderer, "renderer" | "api"> & {
        renderer: string;
        detailsRenderer: SerializedBaseRenderer;
        api: string | Array<string>;
      },
      string
    > =>
      serialized.api == undefined
        ? ValueOrErrors.Default.throwOne(`api is missing`)
        : !isString(serialized.api) && !Array.isArray(serialized.api)
          ? ValueOrErrors.Default.throwOne(`api must be a string or an array`)
          : Array.isArray(serialized.api) && serialized.api.length != 2
            ? ValueOrErrors.Default.throwOne(`api must be an array of length 2`)
            : Array.isArray(serialized.api) &&
                (typeof serialized.api[0] != "string" ||
                  typeof serialized.api[1] != "string")
              ? ValueOrErrors.Default.throwOne(
                  `api array elements must be strings`,
                )
              : serialized.renderer == undefined
                ? ValueOrErrors.Default.throwOne(`renderer is missing`)
                : typeof serialized.renderer != "string"
                  ? ValueOrErrors.Default.throwOne(`renderer must be a string`)
                  : serialized.detailsRenderer == undefined
                    ? ValueOrErrors.Default.throwOne(
                        `detailsRenderer is missing`,
                      )
                    : ValueOrErrors.Default.return({
                        ...serialized,
                        detailsRenderer: serialized.detailsRenderer,
                        renderer: serialized.renderer,
                        api: serialized.api,
                      }),
    DeserializePreviewRenderer: <T>(
      type: OneType<T>,
      serialized: SerializedBaseOneRenderer,
      fieldViews: any,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<
      | BaseRenderer<T>
      | TableFormRenderer<T>
      | RecordFormRenderer<T>
      | undefined,
      string
    > =>
      serialized.previewRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : BaseRenderer.Operations.DeserializeAs(
            type.args[0],
            serialized.previewRenderer,
            fieldViews,
            "nested",
            "preview renderer",
            types,
          ),
    Deserialize: <T>(
      type: OneType<T>,
      serialized: SerializedBaseOneRenderer,
      renderingContext: ParentContext,
      fieldViews: any,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<BaseOneRenderer<T>, string> =>
      BaseOneRenderer.Operations.tryAsValidBaseOneRenderer(serialized).Then(
        (renderer) =>
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
                renderer.detailsRenderer,
                fieldViews,
                "nested",
                "detail renderer",
                types,
              ).Then((detailsRenderer) =>
                BaseOneRenderer.Operations.DeserializePreviewRenderer(
                  type,
                  renderer,
                  fieldViews,
                  types,
                ).Then((previewRenderer) =>
                  ValueOrErrors.Default.return(
                    BaseOneRenderer.Default(
                      type,
                      renderer.renderer,
                      renderer.api,
                      detailsRenderer,
                      previewRenderer,
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
      ),
  },
};
