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

export type SerializedTableCellRenderer = {
  disabled?: unknown;
} & SerializedNestedRenderer;

export type TableCellRenderer<T> = {
  disabled?: Expr;
} & NestedRenderer<T>;

export const TableCellRenderer = {
  Operations: {
    ComputeDisabled: (
      disabled: unknown,
    ): ValueOrErrors<Expr | undefined, string> =>
      disabled == undefined
        ? ValueOrErrors.Default.return(undefined)
        : Expr.Operations.parseAsDisabledExpression(disabled),
    tryAsValidTableCellRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedTableCellRenderer, string> =>
      NestedRenderer.Operations.tryAsValidSerializedNestedRenderer(
        serialized,
      ).Then((deserializedRenderer) =>
        ValueOrErrors.Default.return<SerializedTableCellRenderer, string>({
          ...deserializedRenderer,
          disabled:
            isObject(serialized) && "disabled" in serialized
              ? serialized.disabled
              : undefined,
        }),
      ),
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
      columnName: string,
    ): ValueOrErrors<TableCellRenderer<T>, string> =>
      TableCellRenderer.Operations.tryAsValidTableCellRenderer(serialized).Then(
        (validatedSerialized) =>
          NestedRenderer.Operations.DeserializeAs(
            type,
            validatedSerialized,
            concreteRenderers,
            `Table cell renderer for column ${columnName}`,
            types,
          ).Then((deserializedNestedRenderer) =>
            TableCellRenderer.Operations.ComputeDisabled(
              validatedSerialized.disabled,
            ).Then((disabled) =>
              ValueOrErrors.Default.return({
                ...deserializedNestedRenderer,
                disabled,
              }),
            ),
          ),
      ),
  },
};
