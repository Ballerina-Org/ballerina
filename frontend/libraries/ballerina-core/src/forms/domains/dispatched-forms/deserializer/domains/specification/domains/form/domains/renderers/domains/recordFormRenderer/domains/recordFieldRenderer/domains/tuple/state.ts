import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../../../main";
import { TupleType } from "../../../../../../../../../types/state";
import { NestedRenderer } from "../../../../../nestedRenderer/state";

import {
  RecordFieldBaseRenderer,
  BaseSerializedRecordFieldRenderer,
  RecordFieldRenderer,
  SerializedRecordFieldRenderer,
} from "../../state";
import { List } from "immutable";

export type SerializedTupleRecordFieldRenderer = {
  itemRenderers?: unknown;
} & BaseSerializedRecordFieldRenderer;

export type RecordFieldTupleRenderer<T> = RecordFieldBaseRenderer<T> & {
  kind: "recordFieldTupleRenderer";
  itemRenderers: Array<NestedRenderer<T>>;
  type: TupleType<T>;
  concreteRendererName: string;
};

export const RecordFieldTupleRenderer = {
  Default: <T>(
    type: TupleType<T>,
    fieldName: string,
    concreteRendererName: string,
    itemRenderers: Array<NestedRenderer<T>>,
    visible: Expr,
    disabled: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): RecordFieldTupleRenderer<T> => ({
    kind: "recordFieldTupleRenderer",
    type,
    fieldName,
    concreteRendererName,
    itemRenderers,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderers: (
      serialized: SerializedTupleRecordFieldRenderer,
    ): serialized is SerializedTupleRecordFieldRenderer & {
      renderer: string;
      itemRenderers: Array<SerializedRecordFieldRenderer>;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string" &&
      serialized.itemRenderers != undefined,
    tryAsValidTupleRecordFieldRenderer: (
      fieldName: string,
      serialized: SerializedTupleRecordFieldRenderer,
    ): ValueOrErrors<
      Omit<SerializedTupleRecordFieldRenderer, "renderer" | "itemRenderers"> & {
        renderer: string;
        itemRenderers: Array<SerializedRecordFieldRenderer>;
      },
      string
    > => {
      if (!RecordFieldTupleRenderer.Operations.hasRenderers(serialized))
        return ValueOrErrors.Default.throwOne(
          `renderer and itemRenderers are required for renderer ${fieldName}`,
        );
      if (!Array.isArray(serialized.itemRenderers)) {
        return ValueOrErrors.Default.throwOne(
          `itemRenderers must be an array for renderer ${fieldName}`,
        );
      }
      if (serialized.itemRenderers.length == 0) {
        return ValueOrErrors.Default.throwOne(
          `itemRenderers must have at least one item for renderer ${fieldName}`,
        );
      }
      if (
        serialized.itemRenderers.some(
          (itemRenderer) => typeof itemRenderer != "object",
        )
      ) {
        return ValueOrErrors.Default.throwOne(
          `itemRenderers must be objects for renderer ${fieldName}`,
        );
      }
      const itemRenderers =
        serialized.itemRenderers as Array<SerializedRecordFieldRenderer>;

      return ValueOrErrors.Default.return({
        ...serialized,
        itemRenderers: itemRenderers,
      });
    },
    Deserialize: <T>(
      type: TupleType<T>,
      fieldName: string,
      serialized: SerializedTupleRecordFieldRenderer,
      fieldViews: any,
    ): ValueOrErrors<RecordFieldTupleRenderer<T>, string> =>
      RecordFieldTupleRenderer.Operations.tryAsValidTupleRecordFieldRenderer(
        fieldName,
        serialized,
      )
        .Then((serializedTupleRecordFieldRenderer) =>
          Expr.Operations.parseAsVisibilityExpression(
            serializedTupleRecordFieldRenderer.visible ?? true,
          ).Then((visibleExpr) =>
            Expr.Operations.parseAsDisabledExpression(
              serializedTupleRecordFieldRenderer.disabled ?? false,
            ).Then((disabledExpr) =>
              ValueOrErrors.Operations.All(
                List<ValueOrErrors<NestedRenderer<T>, string>>(
                  serializedTupleRecordFieldRenderer.itemRenderers.map(
                    (itemRenderer, index) =>
                      NestedRenderer.Operations.DeserializeAs(
                        type.args[index],
                        itemRenderer,
                        fieldViews,
                        `Item ${index + 1}`,
                      ).Then((deserializedItemRenderer) => {
                        return ValueOrErrors.Default.return(
                          deserializedItemRenderer,
                        );
                      }),
                  ),
                ),
              ).Then((deserializedItemRenderers) =>
                ValueOrErrors.Default.return(
                  RecordFieldTupleRenderer.Default(
                    type,
                    fieldName,
                    serializedTupleRecordFieldRenderer.renderer,
                    deserializedItemRenderers.toArray(),
                    visibleExpr,
                    disabledExpr,
                    serializedTupleRecordFieldRenderer.label,
                    serializedTupleRecordFieldRenderer.tooltip,
                    serializedTupleRecordFieldRenderer.details,
                  ),
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Tuple renderer`),
        ),
  },
};
