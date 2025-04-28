import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../../../main";
import { DispatchPrimitiveType } from "../../../../../../../../../types/state";
import {
  RecordFieldBaseRenderer,
  BaseSerializedRecordFieldRenderer,
} from "../../state";

export type SerializedPrimitiveRecordFieldRenderer =
  BaseSerializedRecordFieldRenderer;

export type RecordFieldPrimitiveRenderer<T> = RecordFieldBaseRenderer<T> & {
  kind: "recordFieldPrimitiveRenderer";
  type: DispatchPrimitiveType<T>;
  concreteRendererName: string;
};

export const RecordFieldPrimitiveRenderer = {
  Default: <T>(
    type: DispatchPrimitiveType<T>,
    fieldName: string,
    concreteRendererName: string,
    visible: Expr,
    disabled: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): RecordFieldPrimitiveRenderer<T> => ({
    kind: "recordFieldPrimitiveRenderer",
    type,
    fieldName,
    concreteRendererName,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderer: (
      serialized: SerializedPrimitiveRecordFieldRenderer,
    ): serialized is SerializedPrimitiveRecordFieldRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",
    tryAsValidPrimitiveRecordField: (
      fieldName: string,
      serialized: SerializedPrimitiveRecordFieldRenderer,
    ): ValueOrErrors<
      Omit<SerializedPrimitiveRecordFieldRenderer, "renderer"> & {
        renderer: string;
      },
      string
    > =>
      !RecordFieldPrimitiveRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(
            `renderer is required for field ${fieldName}`,
          )
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: DispatchPrimitiveType<T>,
      fieldName: string,
      serialized: SerializedPrimitiveRecordFieldRenderer,
    ): ValueOrErrors<RecordFieldPrimitiveRenderer<T>, string> =>
      RecordFieldPrimitiveRenderer.Operations.tryAsValidPrimitiveRecordField(
        fieldName,
        serialized,
      )
        .Then((primitiveRecordFieldRenderer) =>
          Expr.Operations.parseAsVisibilityExpression(
            primitiveRecordFieldRenderer.visible ?? true,
          ).Then((visibilityExpr) =>
            Expr.Operations.parseAsDisabledExpression(
              primitiveRecordFieldRenderer.disabled ?? false,
            ).Then((disabledExpr) =>
              ValueOrErrors.Default.return(
                RecordFieldPrimitiveRenderer.Default(
                  type,
                  fieldName,
                  primitiveRecordFieldRenderer.renderer,
                  visibilityExpr,
                  disabledExpr,
                  primitiveRecordFieldRenderer.label,
                  primitiveRecordFieldRenderer.tooltip,
                  primitiveRecordFieldRenderer.details,
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When parsing as Primitive renderer`,
          ),
        ),
  },
};
