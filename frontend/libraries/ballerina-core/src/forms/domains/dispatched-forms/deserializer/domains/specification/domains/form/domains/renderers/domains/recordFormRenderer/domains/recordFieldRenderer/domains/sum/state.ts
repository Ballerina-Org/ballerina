import { List } from "immutable";
import {
  SerializedRecordFieldRenderer,
  BaseSerializedRecordFieldRenderer,
  RecordFieldRenderer,
  RecordFieldBaseRenderer as RecordFieldBaseRenderer,
} from "../../state";
import { SumType } from "../../../../../../../../../types/state";
import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../../../main";
import { NestedRenderer } from "../../../../../nestedRenderer/state";

export type SerializedSumRecordFieldRenderer = {
  leftRenderer?: unknown;
  rightRenderer?: unknown;
} & BaseSerializedRecordFieldRenderer;

export type RecordFieldSumRenderer<T> = RecordFieldBaseRenderer<T> & {
  kind: "recordFieldSumRenderer";
  leftRenderer: NestedRenderer<T>;
  rightRenderer: NestedRenderer<T>;
  type: SumType<T>;
  concreteRendererName: string;
};

export const RecordFieldSumRenderer = {
  Default: <T>(
    type: SumType<T>,
    fieldName: string,
    concreteRendererName: string,
    leftRenderer: NestedRenderer<T>,
    rightRenderer: NestedRenderer<T>,
    visible: Expr,
    disabled: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): RecordFieldSumRenderer<T> => ({
    kind: "recordFieldSumRenderer",
    type,
    fieldName,
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
      serialized: SerializedSumRecordFieldRenderer,
    ): serialized is SerializedSumRecordFieldRenderer & {
      renderer: string;
      leftRenderer: SerializedRecordFieldRenderer;
      rightRenderer: SerializedRecordFieldRenderer;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string" &&
      serialized.leftRenderer != undefined &&
      serialized.rightRenderer != undefined,
    tryAsValidSumRecordFieldRenderer: (
      fieldName: string,
      serialized: SerializedSumRecordFieldRenderer,
    ): ValueOrErrors<
      Omit<
        SerializedSumRecordFieldRenderer,
        "renderer" | "leftRenderer" | "rightRenderer"
      > & {
        renderer: string;
        leftRenderer: SerializedRecordFieldRenderer;
        rightRenderer: SerializedRecordFieldRenderer;
      },
      string
    > =>
      !RecordFieldSumRenderer.Operations.hasRenderers(serialized)
        ? ValueOrErrors.Default.throwOne(
            `renderer, leftRenderer and rightRenderer are required for renderer ${fieldName}`,
          )
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SumType<T>,
      fieldName: string,
      serialized: SerializedSumRecordFieldRenderer,
      fieldViews: any,
    ): ValueOrErrors<RecordFieldSumRenderer<T>, string> =>
      RecordFieldSumRenderer.Operations.tryAsValidSumRecordFieldRenderer(
        fieldName,
        serialized,
      )
        .Then((serializedSumRecordFieldRenderer) =>
          Expr.Operations.parseAsVisibilityExpression(
            serializedSumRecordFieldRenderer.visible ?? true,
          ).Then((visibleExpr) =>
            Expr.Operations.parseAsDisabledExpression(
              serializedSumRecordFieldRenderer.disabled ?? false,
            ).Then((disabledExpr) =>
              NestedRenderer.Operations.DeserializeAs(
                type.args[0],
                serializedSumRecordFieldRenderer.leftRenderer,
                fieldViews,
                "Left",
              ).Then((deserializedLeftRenderer) =>
                NestedRenderer.Operations.DeserializeAs(
                  type.args[1],
                  serializedSumRecordFieldRenderer.rightRenderer,
                  fieldViews,
                  "Right",
                ).Then((deserializedRightRenderer) =>
                  ValueOrErrors.Default.return(
                    RecordFieldSumRenderer.Default(
                      type,
                      fieldName,
                      serializedSumRecordFieldRenderer.renderer,
                      deserializedLeftRenderer,
                      deserializedRightRenderer,
                      visibleExpr,
                      disabledExpr,
                      serializedSumRecordFieldRenderer.label,
                      serializedSumRecordFieldRenderer.tooltip,
                      serializedSumRecordFieldRenderer.details,
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
