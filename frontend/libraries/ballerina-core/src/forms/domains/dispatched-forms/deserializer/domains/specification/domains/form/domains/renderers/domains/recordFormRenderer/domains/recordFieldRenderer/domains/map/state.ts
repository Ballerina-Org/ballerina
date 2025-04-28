import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../../../main";
import { MapType } from "../../../../../../../../../types/state";
import { NestedRenderer } from "../../../../../nestedRenderer/state";
import {
  RecordFieldBaseRenderer,
  BaseSerializedRecordFieldRenderer,
  SerializedRecordFieldRenderer,
} from "../../state";

export type SerializedMapRecordFieldRenderer = {
  keyRenderer?: unknown;
  valueRenderer?: unknown;
} & BaseSerializedRecordFieldRenderer;

export type RecordFieldMapRenderer<T> = RecordFieldBaseRenderer<T> & {
  kind: "recordFieldMapRenderer";
  keyRenderer: NestedRenderer<T>;
  valueRenderer: NestedRenderer<T>;
  type: MapType<T>;
  concreteRendererName: string;
};

export const RecordFieldMapRenderer = {
  Default: <T>(
    type: MapType<T>,
    fieldName: string,
    concreteRendererName: string,
    keyRenderer: NestedRenderer<T>,
    valueRenderer: NestedRenderer<T>,
    visible: Expr,
    disabled: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): RecordFieldMapRenderer<T> => ({
    kind: "recordFieldMapRenderer",
    fieldName,
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
      serialized: SerializedMapRecordFieldRenderer,
    ): serialized is SerializedMapRecordFieldRenderer & {
      renderer: string;
      keyRenderer: SerializedRecordFieldRenderer;
      valueRenderer: SerializedRecordFieldRenderer;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string" &&
      serialized.keyRenderer != undefined &&
      serialized.valueRenderer != undefined,
    tryAsValidMapRecordFieldRenderer: (
      fieldName: string,
      serialized: SerializedMapRecordFieldRenderer,
    ): ValueOrErrors<
      Omit<
        SerializedMapRecordFieldRenderer,
        "renderer" | "keyRenderer" | "valueRenderer"
      > & {
        renderer: string;
        keyRenderer: SerializedRecordFieldRenderer;
        valueRenderer: SerializedRecordFieldRenderer;
      },
      string
    > =>
      !RecordFieldMapRenderer.Operations.hasRenderers(serialized)
        ? ValueOrErrors.Default.throwOne(
            `renderer, keyRenderer and valueRenderer are required for field ${fieldName}`,
          )
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: MapType<T>,
      fieldName: string,
      serialized: SerializedMapRecordFieldRenderer,
      fieldViews: any,
    ): ValueOrErrors<RecordFieldMapRenderer<T>, string> =>
      RecordFieldMapRenderer.Operations.tryAsValidMapRecordFieldRenderer(
        fieldName,
        serialized,
      )
        .Then((serializedMapRecordFieldRenderer) =>
          Expr.Operations.parseAsVisibilityExpression(
            serializedMapRecordFieldRenderer.visible ?? true,
          ).Then((visibilityExpr) =>
            Expr.Operations.parseAsDisabledExpression(
              serializedMapRecordFieldRenderer.disabled ?? false,
            ).Then((disabledExpr) =>
              NestedRenderer.Operations.DeserializeAs(
                type.args[0],
                serializedMapRecordFieldRenderer.keyRenderer,
                fieldViews,
                "Key",
              ).Then((deserializedKeyRenderer) =>
                NestedRenderer.Operations.DeserializeAs(
                  type.args[1],
                  serializedMapRecordFieldRenderer.valueRenderer,
                  fieldViews,
                  "Value",
                ).Then((deserializedValueRenderer) =>
                  ValueOrErrors.Default.return(
                    RecordFieldMapRenderer.Default(
                      type,
                      fieldName,
                      serializedMapRecordFieldRenderer.renderer,
                      deserializedKeyRenderer,
                      deserializedValueRenderer,
                      visibilityExpr,
                      disabledExpr,
                      serializedMapRecordFieldRenderer.label,
                      serializedMapRecordFieldRenderer.tooltip,
                      serializedMapRecordFieldRenderer.details,
                    ),
                  ),
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Map renderer`),
        ),
  },
};
