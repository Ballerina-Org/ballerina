import { Map } from "immutable";
import {
  DispatchIsObject,
  RecordType,
  DispatchParsedType,
  TableType,
} from "../types/state";

import {
  MapRepo,
  SerializedTableFormRenderer,
  TableFormRenderer,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { SerializedUnionRenderer } from "./domains/renderer/domains/union/state";
import { Renderer } from "./domains/renderer/state";

export type SerializedForm = {
  type?: unknown;
  renderer?: unknown;
  columns?: unknown;
  detailsRenderer?: unknown;
  visibleColumns?: unknown;
  fields?: unknown;
  cases?: unknown;
  tabs?: unknown;
  header?: unknown;
  extends?: unknown;
};
export const SerializedForm = {
  Operations: {
    hasValidType: (_: unknown): _ is { type: string } =>
      DispatchIsObject(_) && "type" in _ && typeof _["type"] == "string",
    withType: <T>(
      _: unknown,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<
      | {
          kind: "recordForm";
          form: SerializedRecordFormRenderer;
          type: RecordType<T>;
        }
      | {
          kind: "tableForm";
          form: SerializedTableFormRenderer;
          type: TableType<T>;
        },
      string
    > =>
      SerializedForm.Operations.hasValidType(_)
        ? MapRepo.Operations.tryFindWithError(
            _.type,
            types,
            () => `form type ${_.type} is not supported`,
          ).Then((formType) =>
            formType.kind != "record"
              ? ValueOrErrors.Default.throwOne(
                  "form is missing the required type attribute",
                )
              : SerializedForm.Operations.IsSerializedRecordFormRenderer(_)
              ? ValueOrErrors.Default.return({
                  kind: "recordForm",
                  form: _,
                  type: formType,
                })
              : SerializedForm.Operations.IsSerializedTableFormRenderer(_)
              ? ValueOrErrors.Default.return({
                  kind: "tableForm",
                  form: _,
                  type: DispatchParsedType.Default.table(
                    formType.name,
                    [formType],
                    formType.typeName,
                  ),
                })
              : ValueOrErrors.Default.throwOne("form kind is not supported"),
          )
        : ValueOrErrors.Default.throwOne(
            "form is missing the required type attribute",
          ),
  },
};

export const Form = <T>() => ({
  Operations: {
    Deserialize: (
      types: Map<string, DispatchParsedType<T>>,
      formName: string,
      serialized: unknown,
      fieldViews?: any,
    ): ValueOrErrors<Renderer<T>, string> =>
      SerializedForm.Operations.withType(serialized, types)
        .Then((serializedWithType) =>
          serializedWithType.kind == "recordForm"
            ? RecordFormRenderer.Operations.Deserialize(
                serializedWithType.type,
                serializedWithType.form,
                types,
                fieldViews,
              )
            : (TableFormRenderer.Operations.Deserialize(
                serializedWithType.type,
                serializedWithType.form,
                types,
                fieldViews,
              ) as ValueOrErrors<FormRenderer<T>, string>),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing Form ${formName}`),
        ),
  },
});
