import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  isString,
  MapRepo,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { DispatchIsObject, UnionType } from "../../../../../types/state";
import { DispatchParsedType } from "../../../../../types/state";
import { List, Map } from "immutable";
import { Renderer } from "../../state";

export type SerializedUnionRenderer = {
  renderer: string;
  cases: Map<string, unknown>;
};

export type UnionRenderer<T> = {
  kind: "unionRenderer";
  concreteRenderer: string;
  type: UnionType<T>;
  cases: Map<string, Renderer<T>>;
};

export const UnionRenderer = {
  Default: <T>(
    type: UnionType<T>,
    cases: Map<string, Renderer<T>>,
    concreteRenderer: string,
  ): UnionRenderer<T> => ({
    kind: "unionRenderer",
    type,
    cases,
    concreteRenderer,
  }),
  Operations: {
    hasCases: (_: unknown): _ is { cases: Record<string, object> } =>
      DispatchIsObject(_) && "cases" in _ && DispatchIsObject(_.cases),
    tryAsValidUnionForm: <T>(
      serialized: unknown,
    ): ValueOrErrors<SerializedUnionRenderer, string> =>
      !UnionRenderer.Operations.hasCases(serialized)
        ? ValueOrErrors.Default.throwOne(
            `union form is missing the required cases attribute`,
          )
        : !("renderer" in serialized)
          ? ValueOrErrors.Default.throwOne(
              `union form is missing the required renderer attribute`,
            )
          : !isString(serialized.renderer)
            ? ValueOrErrors.Default.throwOne(
                `union form is missing the required renderer attribute`,
              )
            : ValueOrErrors.Default.return({
                renderer: serialized.renderer,
                cases: Map(serialized.cases),
              }),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: UnionType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<UnionRenderer<T>, string> =>
      UnionRenderer.Operations.tryAsValidUnionForm(serialized)
        .Then((validSerialized) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<[string, Renderer<T>], string>>(
              validSerialized.cases
                .entrySeq()
                .toArray()
                .map(([caseName, caseRenderer]) =>
                  MapRepo.Operations.tryFindWithError(
                    caseName,
                    type.args,
                    () =>
                      `case ${caseName} not found in type ${JSON.stringify(type, null, 2)}`,
                  ).Then((caseType) =>
                    Renderer.Operations.Deserialize(
                      caseType,
                      // TODO likely the cases should be typed as nested renderers to avoid this
                      typeof caseRenderer === "object" &&
                        caseRenderer !== null &&
                        "renderer" in caseRenderer
                        ? caseRenderer.renderer
                        : caseRenderer,
                      concreteRenderers,
                      types,
                      undefined,
                    ).Then((caseRenderer) =>
                      ValueOrErrors.Default.return([caseName, caseRenderer]),
                    ),
                  ),
                ),
            ),
          ).Then((caseTuples) =>
            ValueOrErrors.Default.return(
              UnionRenderer.Default(
                type,
                Map(caseTuples),
                validSerialized.renderer,
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as union form`),
        ),
  },
};
