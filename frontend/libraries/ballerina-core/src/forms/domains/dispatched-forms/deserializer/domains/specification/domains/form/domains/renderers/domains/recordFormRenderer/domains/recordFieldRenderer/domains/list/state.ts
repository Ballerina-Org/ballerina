import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../../../main";
import { ListType } from "../../../../../../../../../types/state";
import {
  RecordFieldBaseRenderer,
  BaseSerializedRecordFieldRenderer,
} from "../../state";
import { NestedRenderer } from "../../../../../nestedRenderer/state";

export type SerializedListRecordFieldRenderer = {
  elementRenderer?: unknown;
  elementLabel?: string;
  elementTooltip?: string;
} & BaseSerializedRecordFieldRenderer;

export type RecordFieldListRenderer<T> = RecordFieldBaseRenderer<T> & {
  kind: "recordFieldListRenderer";
  elementRenderer: NestedRenderer<T>;
  type: ListType<T>;
  concreteRendererName: string;
};

export const RecordFieldListRenderer = {
  Default: <T>(
    type: ListType<T>,
    fieldName: string,
    concreteRendererName: string,
    elementRenderer: NestedRenderer<T>,
    visible: Expr,
    disabled: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): RecordFieldListRenderer<T> => ({
    kind: "recordFieldListRenderer",
    type,
    fieldName,
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
      serialized: SerializedListRecordFieldRenderer,
    ): serialized is SerializedListRecordFieldRenderer & {
      renderer: string;
      elementRenderer: string | object;
    } => {
      return (
        serialized.renderer != undefined &&
        typeof serialized.renderer == "string" &&
        serialized.elementRenderer != undefined
      );
    },
    tryAsValidListRecordField: (
      fieldName: string,
      serialized: SerializedListRecordFieldRenderer,
    ): ValueOrErrors<
      Omit<
        SerializedListRecordFieldRenderer,
        "renderer" | "elementRenderer"
      > & {
        renderer: string;
        elementRenderer: object;
      },
      string
    > => {
      if (!RecordFieldListRenderer.Operations.hasRenderers(serialized))
        return ValueOrErrors.Default.throwOne(
          `renderer and elementRenderer are required for field ${fieldName}`,
        );
      const elementRenderer = serialized.elementRenderer;
      // Backwards compatability
      if (typeof elementRenderer == "string") {
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
      }
      return ValueOrErrors.Default.return({
        ...serialized,
        elementRenderer: elementRenderer,
      });
    },
    Deserialize: <T>(
      type: ListType<T>,
      fieldName: string,
      serialized: SerializedListRecordFieldRenderer,
      fieldViews: any,
    ): ValueOrErrors<RecordFieldListRenderer<T>, string> =>
      RecordFieldListRenderer.Operations.tryAsValidListRecordField(
        fieldName,
        serialized,
      )
        .Then((serializedListRecordFieldRenderer) =>
          Expr.Operations.parseAsVisibilityExpression(
            serializedListRecordFieldRenderer.visible ?? true,
          ).Then((visibilityExpr) =>
            Expr.Operations.parseAsDisabledExpression(
              serializedListRecordFieldRenderer.disabled ?? false,
            ).Then((disabledExpr) =>
              NestedRenderer.Operations.DeserializeAs(
                type.args[0],
                serializedListRecordFieldRenderer.elementRenderer,
                fieldViews,
                "Element",
              ).Then((elementRenderer) =>
                ValueOrErrors.Default.return(
                  RecordFieldListRenderer.Default(
                    type,
                    fieldName,
                    serializedListRecordFieldRenderer.renderer,
                    elementRenderer,
                    visibilityExpr,
                    disabledExpr,
                    serializedListRecordFieldRenderer.label,
                    serializedListRecordFieldRenderer.tooltip,
                    serializedListRecordFieldRenderer.details,
                  ),
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as List renderer`),
        ),
  },
};
