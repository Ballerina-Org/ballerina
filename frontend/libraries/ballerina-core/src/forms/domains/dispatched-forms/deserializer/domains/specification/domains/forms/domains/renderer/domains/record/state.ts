import { List, Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchIsObject,
  DispatchParsedType,
  FormLayout,
  MapRepo,
  PredicateFormLayout,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { RecordType } from "../../../../../../../../../../../../../main";
import { RecordFieldRenderer } from "./domains/recordFieldRenderer/state";
import { Renderer } from "../../state";

export type SerializedRecordRenderer = {
  type?: string;
  renderer?: unknown;
  fields: Map<string, unknown>;
  tabs: object;
  extends?: string[];
};

export type RecordRenderer<T> = {
  kind: "recordRenderer";
  renderer?: Renderer<T>;
  fields: Map<string, RecordFieldRenderer<T>>;
  type: RecordType<T>;
  tabs: PredicateFormLayout;
  isInlined: boolean;
};

export const RecordRenderer = {
  Default: <T>(
    type: RecordType<T>,
    fields: Map<string, RecordFieldRenderer<T>>,
    tabs: PredicateFormLayout,
    isInlined: boolean,
    renderer?: Renderer<T>,
  ): RecordRenderer<T> => ({
    kind: "recordRenderer",
    type,
    fields,
    tabs,
    renderer,
    isInlined,
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
              : "type" in _ && typeof _.type != "string"
                ? ValueOrErrors.Default.throwOne(
                    "top level record form type attribute is not a string",
                  )
                : ValueOrErrors.Default.return({
                    ..._,
                    type: "type" in _ ? (_.type as string) : undefined,
                    fields: Map(_.fields as object),
                    tabs: _.tabs as object,
                    extends:
                      "extends" in _ ? (_.extends as string[]) : undefined,
                  }),
    DeserializeRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      type: RecordType<T>,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts
      >,
      types: Map<string, DispatchParsedType<T>>,
      serialized?: unknown,
    ): ValueOrErrors<Renderer<T> | undefined, string> =>
      serialized
        ? Renderer.Operations.Deserialize(
            type,
            serialized,
            concreteRenderers,
            types,
            undefined,
            undefined,
          )
        : ValueOrErrors.Default.return(undefined),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      type: RecordType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts
      >,
      types: Map<string, DispatchParsedType<T>>,
      isInlined: boolean,
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
              RecordRenderer.Operations.DeserializeRenderer(
                type,
                concreteRenderers,
                types,
                validRecordForm.renderer,
              ).Then((renderer) =>
                FormLayout.Operations.ParseLayout(validRecordForm)
                  .Then((tabs) =>
                    ValueOrErrors.Default.return(
                      RecordRenderer.Default(
                        type,
                        Map(fieldTuples.toArray()),
                        tabs,
                        isInlined,
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
