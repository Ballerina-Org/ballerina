import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  isString,
  MapRepo,
  ValueOrErrors,
  SpecVersion,
} from "../../../../../../../../../../../../../main";
import { DispatchIsObject, UnionType } from "../../../../../types/state";
import { DispatchParsedType } from "../../../../../types/state";
import { Map } from "immutable";
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
      CustomPresentationContext,
      ExtraContext,
    >(
      type: UnionType<T>,
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
      specVersionContext: SpecVersion,
    ): ValueOrErrors<[UnionRenderer<T>, Map<string, Renderer<T>>], string> =>
      UnionRenderer.Operations.tryAsValidUnionForm(serialized)
        .Then((validSerialized) =>
          validSerialized.cases
            .entrySeq()
            .reduce<
              ValueOrErrors<
                [Map<string, Renderer<T>>, Map<string, Renderer<T>>],
                string
              >
            >(
              (acc, [caseName, caseRenderer]) =>
                acc.Then(([casesMap, accumulatedAlreadyParsedForms]) =>
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
                      forms,
                      accumulatedAlreadyParsedForms,
                      specVersionContext,
                    ).Then(([caseRenderer, newAlreadyParsedForms]) =>
                      ValueOrErrors.Default.return<
                        [Map<string, Renderer<T>>, Map<string, Renderer<T>>],
                        string
                      >([
                        casesMap.set(caseName, caseRenderer),
                        newAlreadyParsedForms,
                      ]),
                    ),
                  ),
                ),
              ValueOrErrors.Default.return<
                [Map<string, Renderer<T>>, Map<string, Renderer<T>>],
                string
              >([Map<string, Renderer<T>>(), alreadyParsedForms]),
            )
            .Then(([casesMap, accumulatedAlreadyParsedForms]) =>
              ValueOrErrors.Default.return<
                [UnionRenderer<T>, Map<string, Renderer<T>>],
                string
              >([
                UnionRenderer.Default(type, casesMap, validSerialized.renderer),
                accumulatedAlreadyParsedForms,
              ]),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as union form`),
        ),
  },
};
