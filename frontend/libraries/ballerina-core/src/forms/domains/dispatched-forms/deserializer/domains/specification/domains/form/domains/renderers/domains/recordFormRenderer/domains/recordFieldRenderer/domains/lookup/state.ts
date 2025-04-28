import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../../../main";
import { LookupType } from "../../../../../../../../../types/state";
import {
  RecordFieldBaseRenderer,
  BaseSerializedRecordFieldRenderer,
} from "../../state";

export type SerializedLookupRecordFieldRenderer =
  BaseSerializedRecordFieldRenderer;

export type RecordFieldLookupRenderer<T> = RecordFieldBaseRenderer<T> & {
  kind: "recordFieldLookupRenderer";
  type: LookupType;
  lookupRendererName: string;
};

export const RecordFieldLookupRenderer = {
  Default: <T>(
    type: LookupType,
    fieldName: string,
    lookupRendererName: string,
    visible: Expr,
    disabled: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): RecordFieldLookupRenderer<T> => ({
    kind: "recordFieldLookupRenderer",
    type,
    fieldName,
    lookupRendererName,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderer: (
      serialized: SerializedLookupRecordFieldRenderer,
    ): serialized is SerializedLookupRecordFieldRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",
    tryAsValidLookupRecordField: (
      fieldName: string,
      serialized: SerializedLookupRecordFieldRenderer,
    ): ValueOrErrors<
      Omit<SerializedLookupRecordFieldRenderer, "renderer"> & {
        renderer: string;
      },
      string
    > =>
      !RecordFieldLookupRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(
            `renderer is required for field ${fieldName}`,
          )
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: LookupType,
      fieldName: string,
      serialized: SerializedLookupRecordFieldRenderer,
    ): ValueOrErrors<RecordFieldLookupRenderer<T>, string> =>
      RecordFieldLookupRenderer.Operations.tryAsValidLookupRecordField(
        fieldName,
        serialized,
      )
        .Then((lookupRecordFieldRenderer) =>
          Expr.Operations.parseAsVisibilityExpression(
            lookupRecordFieldRenderer.visible ?? true,
          ).Then((visibilityExpr) =>
            Expr.Operations.parseAsDisabledExpression(
              lookupRecordFieldRenderer.disabled ?? false,
            ).Then((disabledExpr) =>
              ValueOrErrors.Default.return(
                RecordFieldLookupRenderer.Default(
                  type,
                  fieldName,
                  lookupRecordFieldRenderer.renderer,
                  visibilityExpr,
                  disabledExpr,
                  lookupRecordFieldRenderer.label,
                  lookupRecordFieldRenderer.tooltip,
                  lookupRecordFieldRenderer.details,
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Lookup renderer`),
        ),
  },
};
