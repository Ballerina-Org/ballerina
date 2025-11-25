import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  Renderer,
  SpecVersion,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";

import {
  NestedRenderer,
  SerializedNestedRenderer,
} from "../../../nestedRenderer/state";

export type SerializedTableCellRenderer = SerializedNestedRenderer;

export type TableCellRenderer<T> = NestedRenderer<T>;

export const TableCellRenderer = {
  Operations: {
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
      columnName: string,
      forms: object,
      alreadyParsedForms: Map<string, Renderer<T>>,
      specVersionContext: SpecVersion,
    ): ValueOrErrors<
      [TableCellRenderer<T>, Map<string, Renderer<T>>],
      string
    > =>
      NestedRenderer.Operations.tryAsValidSerializedNestedRenderer(
        serialized,
      ).Then((validatedSerialized) =>
        NestedRenderer.Operations.DeserializeAs(
          type,
          validatedSerialized,
          concreteRenderers,
          `Table cell renderer for column ${columnName}`,
          types,
          forms,
          alreadyParsedForms,
          specVersionContext,
        ).Then(([deserializedNestedRenderer, newAlreadyParsedForms]) =>
          ValueOrErrors.Default.return<
            [TableCellRenderer<T>, Map<string, Renderer<T>>],
            string
          >([deserializedNestedRenderer, newAlreadyParsedForms]),
        ),
      ),
  },
};
