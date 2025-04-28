import { List } from "immutable";
import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../../../main";
import {
  RecordFieldBaseRenderer,
  BaseSerializedRecordFieldRenderer,
} from "../../state";
import {
  MultiSelectionType,
  SingleSelectionType,
} from "../../../../../../../../../types/state";
export type SerializedStreamRecordFieldRenderer = {
  stream?: string;
} & BaseSerializedRecordFieldRenderer;

export type RecordFieldStreamRenderer<T> = RecordFieldBaseRenderer<T> & {
  kind: "recordFieldStreamRenderer";
  stream: string;
  type: SingleSelectionType<T> | MultiSelectionType<T>;
  concreteRendererName: string;
};

export const RecordFieldStreamRenderer = {
  Default: <T>(
    type: SingleSelectionType<T> | MultiSelectionType<T>,
    fieldName: string,
    stream: string,
    concreteRendererName: string,
    visible: Expr,
    disabled: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): RecordFieldStreamRenderer<T> => ({
    kind: "recordFieldStreamRenderer",
    fieldName,
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
      serialized: SerializedStreamRecordFieldRenderer,
    ): serialized is SerializedStreamRecordFieldRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",
    hasStream: (
      serialized: SerializedStreamRecordFieldRenderer,
    ): serialized is SerializedStreamRecordFieldRenderer & {
      stream: string;
    } => serialized.stream != undefined && typeof serialized.stream == "string",
    tryAsValidStreamRecordField: (
      fieldName: string,
      serialized: SerializedStreamRecordFieldRenderer,
    ): ValueOrErrors<
      Omit<SerializedStreamRecordFieldRenderer, "renderer" | "stream"> & {
        renderer: string;
        stream: string;
      },
      string
    > =>
      !RecordFieldStreamRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(
            `renderer is required for field ${fieldName}`,
          )
        : !RecordFieldStreamRenderer.Operations.hasStream(serialized)
          ? ValueOrErrors.Default.throwOne(
              `stream is required for field ${fieldName}`,
            )
          : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SingleSelectionType<T> | MultiSelectionType<T>,
      fieldName: string,

      serialized: SerializedStreamRecordFieldRenderer,
    ): ValueOrErrors<RecordFieldStreamRenderer<T>, string> =>
      RecordFieldStreamRenderer.Operations.tryAsValidStreamRecordField(
        fieldName,
        serialized,
      )
        .Then((streamRecordFieldRenderer) =>
          Expr.Operations.parseAsVisibilityExpression(
            streamRecordFieldRenderer.visible ?? true,
          ).Then((visibilityExpr) =>
            Expr.Operations.parseAsDisabledExpression(
              streamRecordFieldRenderer.disabled ?? false,
            ).Then((disabledExpr) =>
              ValueOrErrors.Default.return(
                RecordFieldStreamRenderer.Default(
                  type,
                  fieldName,
                  streamRecordFieldRenderer.stream,
                  streamRecordFieldRenderer.renderer,
                  visibilityExpr,
                  disabledExpr,
                  streamRecordFieldRenderer.label,
                  streamRecordFieldRenderer.tooltip,
                  streamRecordFieldRenderer.details,
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
