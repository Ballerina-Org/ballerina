import { List, Map } from "immutable";
import {
  DispatchFieldName,
  DispatchIsObject,
  RecordType,
  DispatchParsedType,
  UnionType,
  TableType,
} from "../types/state";
import {
  RecordFormRenderer,
  SerializedRecordFormRenderer,
} from "./domains/renderers/domains/recordFormRenderer/state";
import {
  MapRepo,
  SerializedTableFormRenderer,
  TableFormRenderer,
  ValueOrErrors,
} from "../../../../../../../../../main";

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
    IsSerializedTableFormRenderer: (
      _: unknown,
    ): _ is SerializedTableFormRenderer =>
      DispatchIsObject(_) && "columns" in _,
    IsSerializedRecordFormRenderer: (
      _: unknown,
    ): _ is SerializedRecordFormRenderer =>
      DispatchIsObject(_) && "fields" in _,
    hasRenderer: (_: unknown): _ is { renderer: unknown } =>
      DispatchIsObject(_) && "renderer" in _,
    hasCases: (_: unknown): _ is { cases: unknown } =>
      DispatchIsObject(_) && "cases" in _,
    hasFields: (_: unknown): _ is { fields: unknown } =>
      DispatchIsObject(_) && "fields" in _,
    hasTabs: (_: unknown): _ is { tabs: unknown } =>
      DispatchIsObject(_) && "tabs" in _,
    hasHeader: (_: unknown): _ is { header: unknown } =>
      DispatchIsObject(_) && "header" in _,
    hasExtends: (_: unknown): _ is { extends: unknown } =>
      DispatchIsObject(_) && "extends" in _ && Array.isArray(_.extends),
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
            _["type"],
            types,
            () => `form type ${_["type"]} is not supported`,
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
                  : ValueOrErrors.Default.throwOne(
                      "form kind is not supported",
                    ),
          )
        : ValueOrErrors.Default.throwOne(
            "form is missing the required type attribute",
          ),
  },
};

export type Form<T> = RecordFormRenderer<T> | TableFormRenderer<T>;

export const Form = <T>() => ({
  Operations: {
    Deserialize: (
      types: Map<string, DispatchParsedType<T>>,
      formName: string,
      serialized: SerializedForm,
      fieldViews?: any,
    ): ValueOrErrors<Form<T>, string> =>
      SerializedForm.Operations.withType(serialized, types)
        .Then((serializedWithType) =>
          serializedWithType.kind == "recordForm"
            ? (RecordFormRenderer.Operations.Deserialize(
                serializedWithType.type,
                serializedWithType.form,
                fieldViews,
              ) as ValueOrErrors<Form<T>, string>) /// TODO why??
            : (TableFormRenderer.Operations.Deserialize(
                serializedWithType.type,
                serializedWithType.form,
                types,
                fieldViews,
              ) as ValueOrErrors<Form<T>, string>),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing Form ${formName}`),
        ),
  },
});
