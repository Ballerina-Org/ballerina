import { Map } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchParsedType,
  ListType,
  MapRepo,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { NestedRenderer } from "../nestedRenderer/state";
import { Renderer } from "../../state";

export type SerializedFormLookupRenderer = string;

export type FormLookupRenderer<T> = {
  kind: "formLookupRenderer";
  renderer: Renderer<T>;
  type: DispatchParsedType<T>;
};

export const FormLookupRenderer = {
  Default: <T>(
    type: DispatchParsedType<T>,
    renderer: Renderer<T>,
  ): FormLookupRenderer<T> => ({
    kind: "formLookupRenderer",
    type,
    renderer,
  }),
  Operations: {
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      type: ListType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts
      >,
      types: Map<string, DispatchParsedType<T>>,
      api?: string | string[],
    ): ValueOrErrors<FormLookupRenderer<T>, string> =>
        MapRepo.Operations.tryFindWithError(
            type.name,
            types,
            () => `cannot find lookup type ${type.typeName} in types`,
          ).Then((lookupType) =>
            Renderer.Operations.Deserialize(
              lookupType,
              serialized,
              concreteRenderers,
              types,
              api,
              false,
            ),
          ).

      FormLookupRenderer.Operations.tryAsValidBaseFormLookupRenderer(
        serialized,
        type,
      )
        .Then((serializedRenderer) =>
          NestedRenderer.Operations.DeserializeAs(
            type.args[0],
            serializedRenderer.elementRenderer,
            concreteRenderers,
            "list element",
            types,
          ).Then((elementRenderer) =>
            Renderer.Operations.Deserialize(
              type,
              serializedRenderer.renderer,
              concreteRenderers,
              types,
            ).Then((renderer) =>
              ValueOrErrors.Default.return(
                FormLookupRenderer.Default(type, renderer, elementRenderer),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as List`),
        ),
  },
};
