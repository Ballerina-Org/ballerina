import { List, Map } from "immutable";
import {
  DispatchIsObject,
  RecordType,
  DispatchParsedType,
} from "../../../../../types/state";
import {
  FormLayout,
  PredicateFormLayout,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { BaseRenderer, SerializedBaseRenderer } from "../baseRenderer/state";

export type SerializedRecordFormRenderer = {
  type?: unknown;
  renderer?: unknown;
  fields?: unknown;
  tabs?: unknown;
  extends?: unknown;
};

export type RecordFormRenderer<T> = {
  kind: "recordForm";
  type: DispatchParsedType<T>;
  fields: Map<string, BaseRenderer<T>>;
  tabs: PredicateFormLayout;
  extendsForms: string[];
  concreteRendererName?: string;
};

export const RecordFormRenderer = {
  Default: <T>(
    type: DispatchParsedType<T>,
    fields: Map<string, BaseRenderer<T>>,
    tabs: PredicateFormLayout,
    extendsForms: string[],
    concreteRendererName?: string,
  ): RecordFormRenderer<T> => ({
    kind: "recordForm",
    type,
    fields,
    tabs,
    extendsForms,
    concreteRendererName,
  }),
  Operations: {
    hasFields: (_: unknown): _ is { fields: object } =>
      DispatchIsObject(_) && "fields" in _ && DispatchIsObject(_.fields),
    hasTabs: (_: unknown): _ is { tabs: object } =>
      DispatchIsObject(_) && "tabs" in _ && DispatchIsObject(_.tabs),
    hasExtends: (_: unknown): _ is { extends: unknown } =>
      DispatchIsObject(_) && "extends" in _ && DispatchIsObject(_.extends),
    hasValidRenderer: (_: unknown): _ is { renderer: string | undefined } =>
      DispatchIsObject(_) && "renderer" in _
        ? typeof _.renderer == "string"
        : true,
    hasValidExtends: (_: unknown): _ is string[] =>
      Array.isArray(_) &&
      (_.length == 0 || _.every((e) => typeof e == "string")),
    tryAsValidRecordForm: <T>(
      _: SerializedRecordFormRenderer,
    ): ValueOrErrors<
      Omit<SerializedRecordFormRenderer, "fields" | "tabs" | "extends"> & {
        fields: Map<string, SerializedBaseRenderer>;
        tabs: object;
        extends: string[];
        renderer?: string;
      },
      string
    > => {
      if (!DispatchIsObject(_)) {
        return ValueOrErrors.Default.throwOne("record form is not an object");
      }
      if (!RecordFormRenderer.Operations.hasFields(_)) {
        return ValueOrErrors.Default.throwOne(
          "record form is missing the required fields attribute",
        );
      }
      if (!RecordFormRenderer.Operations.hasTabs(_)) {
        return ValueOrErrors.Default.throwOne(
          "record form is missing the required tabs attribute",
        );
      }
      const extendedFields = RecordFormRenderer.Operations.hasExtends(_)
        ? _.extends
        : [];
      if (!RecordFormRenderer.Operations.hasValidExtends(extendedFields)) {
        return ValueOrErrors.Default.throwOne(
          "record form extends attribute is not an array of strings",
        );
      }
      if (!RecordFormRenderer.Operations.hasValidRenderer(_)) {
        return ValueOrErrors.Default.throwOne(
          "record form renderer attribute is not a string",
        );
      }
      const renderer = _?.renderer;

      return ValueOrErrors.Default.return({
        ..._,
        fields: Map<string, SerializedBaseRenderer>(_.fields),
        tabs: _.tabs,
        extends: extendedFields,
        renderer,
      });
    },
    Deserialize: <T>(
      type: RecordType<T>,
      serialized: SerializedRecordFormRenderer,
      fieldViews?: any,
    ): ValueOrErrors<RecordFormRenderer<T>, string> =>
      RecordFormRenderer.Operations.tryAsValidRecordForm(serialized)
        .Then((validRecordForm) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<[string, BaseRenderer<T>], string>>(
              validRecordForm.fields
                .toArray()
                .map(
                  ([fieldName, fieldRecordRenderer]: [
                    string,
                    SerializedBaseRenderer,
                  ]) => {
                    // TODO refactor
                    const fieldType = type.fields.get(fieldName);
                    if (!fieldType) {
                      return ValueOrErrors.Default.throwOne(
                        `Unknown field type ${fieldName}`,
                      );
                    }
                    return BaseRenderer.Operations.DeserializeAs(
                      fieldType,
                      fieldRecordRenderer,
                      fieldViews,
                      "recordField",
                      `field: ${fieldName}`,
                    ).Then((renderer) =>
                      ValueOrErrors.Default.return([fieldName, renderer]),
                    );
                  },
                ),
            ),
          ).Then((fieldTuples) =>
            FormLayout.Operations.ParseLayout(validRecordForm)
              .Then((tabs) =>
                ValueOrErrors.Default.return(
                  RecordFormRenderer.Default(
                    type,
                    Map(fieldTuples.toArray()),
                    tabs,
                    validRecordForm.extends,
                    validRecordForm?.renderer,
                  ),
                ),
              )
              .MapErrors((errors) =>
                errors.map((error) => `${error}\n...When parsing tabs`),
              ),
          ),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When parsing as RecordForm renderer`,
          ),
        ),
  },
};
