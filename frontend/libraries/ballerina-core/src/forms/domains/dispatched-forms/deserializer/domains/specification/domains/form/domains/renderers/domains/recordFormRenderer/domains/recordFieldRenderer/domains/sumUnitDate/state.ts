import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../../../main";
import { SumType } from "../../../../../../../../../types/state";
import {
  BaseSerializedRecordFieldRenderer,
  RecordFieldBaseRenderer,
} from "../../state";

export type SerializedSumUnitDateFieldRenderer =
  BaseSerializedRecordFieldRenderer;

export type RecordFieldSumUnitDateRenderer<T> = RecordFieldBaseRenderer<T> & {
  kind: "recordFieldSumUnitDateRenderer";
  type: SumType<T>;
  concreteRendererName: string;
};

export const RecordFieldSumUnitDateRenderer = {
  Default: <T>(
    type: SumType<T>,
    fieldName: string,
    concreteRendererName: string,
    visible: Expr,
    disabled: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): RecordFieldSumUnitDateRenderer<T> => ({
    kind: "recordFieldSumUnitDateRenderer",
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
      serialized: SerializedSumUnitDateFieldRenderer,
    ): serialized is SerializedSumUnitDateFieldRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",
    tryAsValidSumUnitDateFieldRenderer: (
      fieldName: string,
      serialized: SerializedSumUnitDateFieldRenderer,
    ): ValueOrErrors<
      Omit<SerializedSumUnitDateFieldRenderer, "renderer"> & {
        renderer: string;
      },
      string
    > =>
      !RecordFieldSumUnitDateRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(
            `renderer is required for sum unit date field renderer ${fieldName}`,
          )
        : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SumType<T>,
      fieldName: string,
      serialized: SerializedSumUnitDateFieldRenderer,
    ): ValueOrErrors<RecordFieldSumUnitDateRenderer<T>, string> =>
      RecordFieldSumUnitDateRenderer.Operations.tryAsValidSumUnitDateFieldRenderer(
        fieldName,
        serialized,
      )
        .Then((sumUnitDateRenderer) =>
          Expr.Operations.parseAsVisibilityExpression(
            sumUnitDateRenderer.visible,
          ).Then((visible) =>
            Expr.Operations.parseAsDisabledExpression(
              sumUnitDateRenderer.disabled,
            ).Then((disabled) =>
              ValueOrErrors.Default.return(
                RecordFieldSumUnitDateRenderer.Default(
                  type,
                  fieldName,
                  sumUnitDateRenderer.renderer,
                  visible,
                  disabled,
                  sumUnitDateRenderer.label,
                  sumUnitDateRenderer.tooltip,
                  sumUnitDateRenderer.details,
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When parsing as SumUnitDate renderer`,
          ),
        ),
  },
};
