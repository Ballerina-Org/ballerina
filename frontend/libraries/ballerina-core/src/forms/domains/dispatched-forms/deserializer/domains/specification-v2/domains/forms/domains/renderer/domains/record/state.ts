import { List, Map } from "immutable";
import {
  ConcreteRenderers,
  DisabledFields,
  DispatchInjectablesTypes,
  DispatchIsObject,
  DispatchParsedType,
  FormLayout,
  isObject,
  isString,
  MapRepo,
  PredicateFormLayout,
  PredicateComputedOrInlined,
  ValueOrErrors,
  Renderer,
} from "../../../../../../../../../../../../../main";
import { RecordType } from "../../../../../../../../../../../../../main";
import { RecordFieldRenderer } from "./domains/recordFieldRenderer/state";

export type SerializedRecordRenderer = {
  type: string;
  renderer?: string;
  fields: Map<string, unknown>;
  tabs: object;
  extends?: string[];
};

export type RecordRenderer<T> = {
  kind: "recordRenderer";
  concreteRenderer?: string;
  fields: Map<string, RecordFieldRenderer<T>>;
  type: RecordType<T>;
  tabs: PredicateFormLayout;
  disabledFields: PredicateComputedOrInlined;
};

export const RecordRenderer = {
  Default: <T>(
    type: RecordType<T>,
    fields: Map<string, RecordFieldRenderer<T>>,
    tabs: PredicateFormLayout,
    disabledFields: PredicateComputedOrInlined,
    concreteRenderer?: string,
  ): RecordRenderer<T> => ({
    kind: "recordRenderer",
    type,
    fields,
    tabs,
    disabledFields,
    concreteRenderer,
  }),
  Operations: {
    hasValidExtends: (_: unknown): _ is string[] =>
      Array.isArray(_) &&
      (_.length == 0 || _.every((e) => typeof e == "string")),
    tryAsValidRecordForm: <T>(
      _: unknown,
    ): ValueOrErrors<SerializedRecordRenderer, string> =>
      !DispatchIsObject(_)
        ? (console.debug("record form is not an object", _), ValueOrErrors.Default.throwOne("record form is not an object"))
        : !("fields" in _)
          ? ValueOrErrors.Default.throwOne(
              "record form is missing the required fields attribute",
            )
          : !isObject(_.fields)
            ? ValueOrErrors.Default.throwOne(
                "fields attribute is not an object",
              )
            : !("tabs" in _)
              ? ValueOrErrors.Default.throwOne(
                  "record form is missing the required tabs attribute",
                )
              : !isObject(_.tabs)
                ? ValueOrErrors.Default.throwOne(
                    "tabs attribute is not an object",
                  )
                : "extends" in _ &&
                    !RecordRenderer.Operations.hasValidExtends(_.extends)
                  ? ValueOrErrors.Default.throwOne(
                      "extends attribute is not an array of strings",
                    )
                  : !("type" in _)
                    ? ValueOrErrors.Default.throwOne(
                        "top level record form type attribute is not a string",
                      )
                    : !isString(_.type)
                      ? ValueOrErrors.Default.throwOne(
                          "type attribute is not a string",
                        )
                      : "renderer" in _ && typeof _.renderer != "string"
                        ? ValueOrErrors.Default.throwOne(
                            "renderer attribute is not a string",
                          )
                        : ValueOrErrors.Default.return({
                            type: _.type,
                            renderer:
                              "renderer" in _
                                ? (_.renderer as string)
                                : undefined,
                            fields: Map(_.fields),
                            tabs: _.tabs,
                            extends:
                              "extends" in _
                                ? (_.extends as string[])
                                : undefined,
                            disabledFields:
                              "disabledFields" in _
                                ? _.disabledFields
                                : undefined,
                          }),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      type: RecordType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
      forms: object,
      alreadyParsedForms: Map<string, Renderer<T>>,
    ): ValueOrErrors<[RecordRenderer<T>, Map<string, Renderer<T>>], string> =>
      RecordRenderer.Operations.tryAsValidRecordForm(serialized).Then(
        (validRecordForm) =>
          validRecordForm.fields
            .toArray()
            .reduce<
              ValueOrErrors<
                [
                  Map<string, RecordFieldRenderer<T>>,
                  Map<string, Renderer<T>>,
                ],
                string
              >
            >(
              (acc, [fieldName, recordFieldRenderer]: [string, unknown]) =>
                acc.Then(
                  ([fieldsMap, accumulatedAlreadyParsedForms]) =>
                    MapRepo.Operations.tryFindWithError(
                      fieldName,
                      type.fields,
                      () => `Cannot find field type for ${fieldName} in fields`,
                    ).Then((fieldType) =>
                      RecordFieldRenderer.Deserialize(
                        fieldType,
                        recordFieldRenderer,
                        concreteRenderers,
                        types,
                        fieldName,
                        forms,
                        accumulatedAlreadyParsedForms,
                      ).Then(([renderer, newAlreadyParsedForms]) =>
                        ValueOrErrors.Default.return<
                          [
                            Map<string, RecordFieldRenderer<T>>,
                            Map<string, Renderer<T>>,
                          ],
                          string
                        >([
                          fieldsMap.set(fieldName, renderer),
                          newAlreadyParsedForms,
                        ]),
                      ),
                    ),
                ),
              ValueOrErrors.Default.return<
                [
                  Map<string, RecordFieldRenderer<T>>,
                  Map<string, Renderer<T>>,
                ],
                string
              >([Map<string, RecordFieldRenderer<T>>(), alreadyParsedForms]),
            )
            .Then(([fieldsMap, accumulatedAlreadyParsedForms]) =>
              ValueOrErrors.Operations.All(
                List<
                  ValueOrErrors<
                    PredicateFormLayout | PredicateComputedOrInlined,
                    string
                  >
                >([
                  FormLayout.Operations.ParseLayout(validRecordForm).MapErrors(
                    (errors) =>
                      errors.map((error) => `${error}\n...When parsing tabs`),
                  ),
                  DisabledFields.Operations.ParseLayout(
                    validRecordForm,
                  ).MapErrors((errors) =>
                    errors.map(
                      (error) => `${error}\n...When parsing disabled fields`,
                    ),
                  ),
                ]),
              ).Then(([tabs, disabledFields]) =>
                ValueOrErrors.Default.return<
                  [RecordRenderer<T>, Map<string, Renderer<T>>],
                  string
                >([
                  RecordRenderer.Default(
                    type,
                    fieldsMap,
                    tabs as PredicateFormLayout,
                    disabledFields as PredicateComputedOrInlined,
                    validRecordForm.renderer,
                  ),
                  accumulatedAlreadyParsedForms,
                ]),
              ),
            )
            .MapErrors((errors) =>
              errors.map(
                (error) => `${error}\n...When parsing as RecordForm renderer`,
              ),
            ),
      ),
  },
};
