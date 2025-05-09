import {
  MapRepo,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { DispatchIsObject, UnionType } from "../../../../../types/state";

import { List, Map } from "immutable";
import { DispatchParsedType } from "../../../../../types/state";
import { Renderer } from "../../../renderer/state";

export type SerializedUnionRenderer = {
  renderer: unknown;
  cases: Map<string, unknown>;
};

export type UnionRenderer<T> = {
  kind: "unionRenderer";
  renderer: Renderer<T>;
  type: DispatchParsedType<T>;
  cases: Map<string, Renderer<T>>;
};

export const UnionFormRenderer = {
  Default: <T>(
    type: DispatchParsedType<T>,
    cases: Map<string, Renderer<T>>,
    renderer: Renderer<T>,
  ): UnionRenderer<T> => ({ kind: "unionRenderer", type, renderer, cases }),
  Operations: {
    hasCases: (_: unknown): _ is { cases: Record<string, object> } =>
      DispatchIsObject(_) && "cases" in _ && DispatchIsObject(_.cases),
    tryAsValidUnionForm: <T>(
      serialized: unknown,
    ): ValueOrErrors<SerializedUnionRenderer, string> =>
      !UnionFormRenderer.Operations.hasCases(serialized)
        ? ValueOrErrors.Default.throwOne(
            `union form is missing the required cases attribute`,
          )
        : !("renderer" in serialized)
        ? ValueOrErrors.Default.throwOne(
            `union form is missing the required renderer attribute`,
          )
        : ValueOrErrors.Default.return({
            ...serialized,
            cases: Map(serialized.cases),
          }),
    Deserialize: <T>(
      type: UnionType<T>,
      serialized: unknown,
      fieldViews: any,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<UnionRenderer<T>, string> =>
      UnionFormRenderer.Operations.tryAsValidUnionForm(serialized)
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
                    () => `case ${caseName} not found in type ${type.typeName}`,
                  ).Then((caseType) =>
                    Renderer.Operations.Deserialize(
                      caseType,
                      caseName,
                      caseRenderer,
                      fieldViews,
                    ).Then((caseRenderer) =>
                      ValueOrErrors.Default.return([caseName, caseRenderer]),
                    ),
                  ),
                ),
            ),
          ).Then((caseTuples) =>
            Renderer.Operations.Deserialize(
              type,
              validSerialized.renderer,
              fieldViews,
              types,
            ).Then((renderer) =>
              ValueOrErrors.Default.return(
                UnionFormRenderer.Default(type, Map(caseTuples), renderer),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as union form`),
        ),
  },
};
