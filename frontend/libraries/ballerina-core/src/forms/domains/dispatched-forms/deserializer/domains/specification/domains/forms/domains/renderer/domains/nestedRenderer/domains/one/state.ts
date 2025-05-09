import { Map } from "immutable";

import {
  DispatchParsedType,
  Expr,
  isString,
  OneType,
  SerializedTableFormRenderer,
  TableFormRenderer,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import {
  RecordFormRenderer,
  SerializedRecordFormRenderer,
} from "../../../recordFormRenderer/state";
import {
  SerializedNestedPrimitiveRenderer,
  ParentContext,
  NestedRenderer,
  BaseBaseRenderer,
  SerializedBaseRenderer,
} from "../../state";

export type SerializedBaseOneRenderer = {
  api?: unknown;
  detailsRenderer?: unknown;
  previewRenderer?: unknown;
} & SerializedNestedPrimitiveRenderer;

export type BaseOneRenderer<T> = BaseBaseRenderer & {
  kind: "baseOneRenderer";
  api: string | Array<string>;
  type: OneType<T>;
  concreteRendererName: string;
  detailsRenderer: {
    renderer: BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>;
  };
  previewRenderer?: {
    renderer: BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>;
  };
};

export const BaseOneRenderer = {
  Default: <T>(
    type: OneType<T>,
    concreteRendererName: string,
    api: string | Array<string>,
    detailsRenderer: {
      renderer: BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>;
    },
    previewRenderer?: {
      renderer: BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>;
    },
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
        detailsRenderer: {
          renderer:
            | SerializedBaseRenderer
            | SerializedTableFormRenderer
            | SerializedRecordFormRenderer;
        };
        previewRenderer?: {
          renderer:
            | SerializedBaseRenderer
            | SerializedTableFormRenderer
            | SerializedRecordFormRenderer;
        };
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
        ? ValueOrErrors.Default.throwOne(`api array elements must be strings`)
        : serialized.renderer == undefined
        ? ValueOrErrors.Default.throwOne(`renderer is missing`)
        : typeof serialized.renderer != "string"
        ? ValueOrErrors.Default.throwOne(`renderer must be a string`)
        : serialized.detailsRenderer == undefined
        ? ValueOrErrors.Default.throwOne(`detailsRenderer is missing`)
        : ValueOrErrors.Default.return({
            ...serialized,
            detailsRenderer: {
              renderer: serialized.detailsRenderer,
            },
            previewRenderer: serialized.previewRenderer
              ? {
                  renderer: serialized.previewRenderer,
                }
              : undefined,
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
        : serialized.previewRenderer.renderer == undefined
        ? ValueOrErrors.Default.throwOne(`previewRenderer.renderer is missing`)
        : NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            serialized.previewRenderer.renderer,
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
          NestedRenderer.Operations.ComputeVisibility(
            renderer.visible,
            renderingContext,
          ).Then((visibilityExpr) =>
            NestedRenderer.Operations.ComputeDisabled(
              renderer.disabled,
              renderingContext,
            ).Then((disabledExpr) =>
              NestedRenderer.Operations.DeserializeAs(
                type.args[0],
                renderer.detailsRenderer.renderer,
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
                      {
                        renderer: detailsRenderer,
                      },
                      previewRenderer
                        ? {
                            renderer: previewRenderer,
                          }
                        : undefined,
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
