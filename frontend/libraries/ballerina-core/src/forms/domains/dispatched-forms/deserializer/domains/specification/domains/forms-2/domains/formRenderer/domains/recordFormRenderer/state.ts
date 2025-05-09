import { List, Map } from "immutable";
import {
  DispatchIsObject,
  DispatchParsedType,
  FormLayout,
  MapRepo,
  PredicateFormLayout,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { RecordType } from "../../../../../../../../../../../../../main";
import {
  RecordFieldRenderer,
  SerializedRecordFieldRenderer,
} from "./domains/recordFieldRenderer/state";
import { Renderer } from "../../../renderer/state";

export type SerializedRecordRenderer = {
  type: string;
  renderer?: unknown;
  fields: Map<string, unknown>;
  tabs: object;
  extends?: string[];
};

export type RecordRenderer<T> = {
  kind: "recordRenderer";
  renderer?: unknown;
  fields: Map<string, RecordFieldRenderer<T>>;
  type: RecordType<T>;
  tabs: PredicateFormLayout;
  extendsForms?: string[];
};

export const RecordRenderer = {
  Default: <T>(
    type: RecordType<T>,
    fields: Map<string, RecordFieldRenderer<T>>,
    tabs: PredicateFormLayout,
    extendsForms?: string[],
    renderer?: Renderer<T>,
  ): RecordRenderer<T> => ({
    kind: "recordRenderer",
    type,
    fields,
    tabs,
    extendsForms,
    renderer,
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
        : !("fields" in _) || typeof _.fields != "object"
        ? ValueOrErrors.Default.throwOne(
            "record form is missing the required fields attribute",
          )
        : !("tabs" in _) || typeof _.tabs != "object"
        ? ValueOrErrors.Default.throwOne(
            "record form is missing the required tabs attribute",
          )
        : "extends" in _ &&
          (!Array.isArray(_.extends) ||
            (Array.isArray(_.extends) &&
              _.extends.some((e) => typeof e != "string")))
        ? ValueOrErrors.Default.throwOne(
            "record form extends attribute is not an array of strings",
          )
        : !("type" in _) || ("type" in _ && typeof _.type != "string")
        ? ValueOrErrors.Default.throwOne(
            "record form is missing the required type attribute",
          )
        : ValueOrErrors.Default.return({
            ..._,
            type: _.type as string,
            fields: Map(_.fields as object),
            tabs: _.tabs as object,
            extends: "extends" in _ ? (_.extends as string[]) : undefined,
          }),
    DeserializeRenderer: <T>(
      type: RecordType<T>,
      types: Map<string, DispatchParsedType<T>>,
      fieldViews: any,
      serialized?: unknown,
    ): ValueOrErrors<Renderer<T> | undefined, string> =>
      serialized
        ? Renderer.Operations.Deserialize(type, serialized, types, fieldViews)
        : ValueOrErrors.Default.return(undefined),

    Deserialize: <T>(
      type: RecordType<T>,
      serialized: unknown,
      types: Map<string, DispatchParsedType<T>>,
      fieldViews: any,
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
                    types,
                    () => `Cannot find field type for ${fieldName} in types`,
                  ).Then((fieldType) =>
                    RecordFieldRenderer.Deserialize(
                      fieldType,
                      recordFieldRenderer,
                      fieldViews,
                      types,
                    ).Then((renderer) =>
                      ValueOrErrors.Default.return([fieldName, renderer]),
                    ),
                  ),
                ),
            ),
          )
            .Then((fieldTuples) =>
              RecordRenderer.Operations.DeserializeRenderer(
                type,
                types,
                fieldViews,
                validRecordForm.renderer,
              ).Then((renderer) =>
                FormLayout.Operations.ParseLayout(validRecordForm)
                  .Then((tabs) =>
                    ValueOrErrors.Default.return(
                      RecordRenderer.Default(
                        type,
                        Map(fieldTuples.toArray()),
                        tabs,
                        validRecordForm.extends,
                        renderer,
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
      ),
  },
};
