import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  isObject,
  Renderer,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";

import {
  NestedRenderer,
  SerializedNestedRenderer,
} from "../../../nestedRenderer/state";

export type SerializedRecordFieldRenderer = {
  api?: unknown;
} & SerializedNestedRenderer;

export type RecordFieldRenderer<T> = {
  api?: string | Array<string>;
} & NestedRenderer<T>;

export const RecordFieldRenderer = {
  tryAsValidRecordFieldRenderer: (
    serialized: unknown,
  ): ValueOrErrors<SerializedRecordFieldRenderer, string> =>
    NestedRenderer.Operations.tryAsValidSerializedNestedRenderer(
      serialized,
    ).Then((deserializedRenderer) =>
      ValueOrErrors.Default.return<SerializedRecordFieldRenderer, string>({
        ...deserializedRenderer,
        api:
          isObject(serialized) && "api" in serialized
            ? serialized.api
            : undefined,
      }),
    ),
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
    fieldName: string,
    forms: object,
    alreadyParsedForms: Map<string, Renderer<T>>,
  ): ValueOrErrors<
    [RecordFieldRenderer<T>, Map<string, Renderer<T>>],
    string
  > =>
    RecordFieldRenderer.tryAsValidRecordFieldRenderer(serialized).Then(
      (validatedSerialized) =>
        NestedRenderer.Operations.DeserializeAs(
          type,
          validatedSerialized,
          concreteRenderers,
          `Record field renderer for field ${fieldName}`,
          types,
          forms,
          alreadyParsedForms,
        ).Then(([deserializedNestedRenderer, newAlreadyParsedForms]) =>
          ValueOrErrors.Default.return([
            deserializedNestedRenderer,
            newAlreadyParsedForms,
          ]),
        ),
    ),
};
