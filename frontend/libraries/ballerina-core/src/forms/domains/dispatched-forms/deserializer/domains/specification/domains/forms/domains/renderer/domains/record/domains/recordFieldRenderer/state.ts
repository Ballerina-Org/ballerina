import { Map } from "immutable";
import {
  ConcreteRendererKinds,
  DispatchParsedType,
  Expr,
  isObject,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";

import {
  NestedRenderer,
  SerializedNestedRenderer,
} from "../../../nestedRenderer/state";

export type SerializedRecordFieldRenderer = {
  visible?: unknown;
  disabled?: unknown;
} & SerializedNestedRenderer;

export type RecordFieldRenderer<T> = {
  visible?: Expr;
  disabled?: Expr;
} & NestedRenderer<T>;

export const RecordFieldRenderer = {
  ComputeVisibility: (
    visible: unknown,
  ): ValueOrErrors<Expr | undefined, string> =>
    visible == undefined
      ? ValueOrErrors.Default.return(undefined)
      : Expr.Operations.parseAsVisibilityExpression(visible),
  ComputeDisabled: (
    disabled: unknown,
  ): ValueOrErrors<Expr | undefined, string> =>
    disabled == undefined
      ? ValueOrErrors.Default.return(undefined)
      : Expr.Operations.parseAsDisabledExpression(disabled),
  tryAsValidRecordFieldRenderer: (
    serialized: unknown,
  ): ValueOrErrors<SerializedRecordFieldRenderer, string> =>
    NestedRenderer.Operations.tryAsValidSerializedNestedRenderer(
      serialized,
    ).Then((deserializedRenderer) =>
      ValueOrErrors.Default.return<SerializedRecordFieldRenderer, string>({
        ...deserializedRenderer,
        visible:
          isObject(serialized) && "visible" in serialized
            ? serialized.visible
            : undefined,
        disabled:
          isObject(serialized) && "disabled" in serialized
            ? serialized.disabled
            : undefined,
      }),
    ),
  Deserialize: <T>(
    type: DispatchParsedType<T>,
    serialized: unknown,
    concreteRenderers: Record<keyof ConcreteRendererKinds<T>, any>,
    types: Map<string, DispatchParsedType<T>>,
    fieldName: string,
  ): ValueOrErrors<RecordFieldRenderer<T>, string> =>
    RecordFieldRenderer.tryAsValidRecordFieldRenderer(serialized).Then(
      (validatedSerialized) =>
        NestedRenderer.Operations.DeserializeAs(
          type,
          validatedSerialized,
          concreteRenderers,
          `Record field renderer for field ${fieldName}`,
          types,
        ).Then((deserializedNestedRenderer) =>
          RecordFieldRenderer.ComputeVisibility(
            validatedSerialized.visible,
          ).Then((visible) =>
            RecordFieldRenderer.ComputeDisabled(
              validatedSerialized.disabled,
            ).Then((disabled) =>
              ValueOrErrors.Default.return({
                ...deserializedNestedRenderer,
                visible,
                disabled,
              }),
            ),
          ),
        ),
    ),
};
