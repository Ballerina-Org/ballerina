import { List, Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchIsObject,
  DispatchParsedType,
  FormLayout,
  isObject,
  isString,
  MapRepo,
  PredicateFormLayout,
  ValueOrErrors,
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
};

export const RecordRenderer = {
  Default: <T>(
    type: RecordType<T>,
    fields: Map<string, RecordFieldRenderer<T>>,
    tabs: PredicateFormLayout,
    concreteRenderer?: string,
  ): RecordRenderer<T> => ({
    kind: "recordRenderer",
    type,
    fields,
    tabs,
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
        ? ValueOrErrors.Default.throwOne("record form is not an object")
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
                          }),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: RecordType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<RecordRenderer<T>, string> =>
      RecordRenderer.Operations.tryAsValidRecordForm(serialized).Then(
        (validRecordForm) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<[string, RecordFieldRenderer<T>], string>>(
              validRecordForm.fields
                .toArray()
                .map(([fieldName, recordFieldRenderer]: [string, unknown]) =>
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
                    ).Then((renderer) =>
                      ValueOrErrors.Default.return([fieldName, renderer]),
                    ),
                  ),
                ),
            ),
          )
            .Then((fieldTuples) =>
              FormLayout.Operations.ParseLayout(validRecordForm).Then((tabs) =>
                ValueOrErrors.Default.return(
                  RecordRenderer.Default(
                    type,
                    Map(fieldTuples.toArray()),
                    tabs,
                    validRecordForm.renderer,
                  ),
                ).MapErrors((errors) =>
                  errors.map((error) => `${error}\n...When parsing tabs`),
                ),
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
