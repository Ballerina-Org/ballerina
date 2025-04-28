import { ValueOrErrors } from "../../../../../../../../../../../../../../../main";
import { UnionType } from "../../../../../../../types/state";
import {
  NestedRenderer,
  BaseSerializedNestedRenderer,
  SerializedNestedRenderer,
  BaseNestedRenderer,
} from "../../state";
import { List, Map } from "immutable";

export type SerializedNestedUnionRenderer = {
  cases?: unknown;
} & BaseSerializedNestedRenderer;

export type NestedUnionRenderer<T> = BaseNestedRenderer & {
  kind: "nestedUnionRenderer";
  cases: Map<string, NestedRenderer<T>>;
  type: UnionType<T>;
  concreteRendererName: string;
};

export const NestedUnionRenderer = {
  Default: <T>(
    type: UnionType<T>,
    concreteRendererName: string,
    cases: Map<string, NestedRenderer<T>>,
    label?: string,
    tooltip?: string,
    details?: string,
  ): NestedUnionRenderer<T> => ({
    kind: "nestedUnionRenderer",
    type,
    concreteRendererName,
    cases,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderers: (
      serialized: SerializedNestedUnionRenderer,
    ): serialized is SerializedNestedUnionRenderer & {
      renderer: string;
      cases: Record<string, SerializedNestedRenderer>;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string" &&
      serialized.cases != undefined,
    tryAsValidNestedUnionRenderer: (
      serialized: SerializedNestedUnionRenderer,
    ): ValueOrErrors<
      Omit<SerializedNestedUnionRenderer, "renderer" | "cases"> & {
        renderer: string;
        cases: object;
      },
      string
    > => {
      const renderer = serialized.renderer;
      const cases = serialized.cases;
      if (renderer == undefined)
        return ValueOrErrors.Default.throwOne(`renderer is missing`);
      if (typeof renderer != "string")
        return ValueOrErrors.Default.throwOne(`renderer must be a string`);
      if (cases == undefined)
        return ValueOrErrors.Default.throwOne(`cases are missing`);
      if (typeof cases != "object")
        return ValueOrErrors.Default.throwOne(`cases must be an object`);
      if (Object.keys(cases).length == 0)
        return ValueOrErrors.Default.throwOne(
          `cases must have at least one case`,
        );

      return ValueOrErrors.Default.return({
        ...serialized,
        renderer,
        cases,
      });
    },
    Deserialize: <T>(
      type: UnionType<T>,
      serialized: SerializedNestedUnionRenderer,
      fieldViews: any,
    ): ValueOrErrors<NestedUnionRenderer<T>, string> =>
      NestedUnionRenderer.Operations.tryAsValidNestedUnionRenderer(serialized)
        .Then((serializedNestedUnionRenderer) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<[string, NestedRenderer<T>], string>>(
              Object.entries(serializedNestedUnionRenderer.cases).map(
                ([caseName, caseProp]) => {
                  const caseType = type.args.get(caseName);
                  if (caseType == undefined)
                    return ValueOrErrors.Default.throwOne(
                      `case ${caseName} not found`,
                    );

                  return NestedRenderer.Operations.DeserializeAs(
                    caseType.fields,
                    caseProp,
                    fieldViews,
                    `renderer for case ${caseName}`,
                  ).Then((deserializedCase) =>
                    ValueOrErrors.Default.return([caseName, deserializedCase]),
                  );
                },
              ),
            ),
          ).Then((deserializedCases) =>
            ValueOrErrors.Default.return(
              NestedUnionRenderer.Default(
                type,
                serializedNestedUnionRenderer.renderer,
                Map<string, NestedRenderer<T>>(deserializedCases),
                serializedNestedUnionRenderer.label,
                serializedNestedUnionRenderer.tooltip,
                serializedNestedUnionRenderer.details,
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Union renderer`),
        ),
  },
};
