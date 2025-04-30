import {
  Expr,
  TableFormRenderer,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import { RecordFormRenderer } from "../../../recordFormRenderer/state";
import {
  DispatchParsedType,
  UnionType,
} from "../../../../../../../types/state";

import {
  BaseBaseRenderer,
  BaseSerializedBaseRenderer,
  SerializedBaseRenderer,
  BaseRenderer,
  ParentContext,
} from "../../state";
import { List, Map } from "immutable";

export type SerializedBaseUnionRenderer = {
  cases?: unknown;
} & BaseSerializedBaseRenderer;

export type BaseUnionRenderer<T> = BaseBaseRenderer & {
  kind: "baseUnionRenderer";
  cases: Map<
    string,
    BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>
  >;
  type: UnionType<T>;
  concreteRendererName: string;
};

export const BaseUnionRenderer = {
  Default: <T>(
    type: UnionType<T>,
    concreteRendererName: string,
    cases: Map<
      string,
      BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>
    >,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseUnionRenderer<T> => ({
    kind: "baseUnionRenderer",
    type,
    concreteRendererName,
    cases,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderers: (
      serialized: SerializedBaseUnionRenderer,
    ): serialized is SerializedBaseUnionRenderer & {
      renderer: string;
      cases: Record<string, SerializedBaseRenderer>;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string" &&
      serialized.cases != undefined,
    tryAsValidBaseUnionRenderer: (
      serialized: SerializedBaseUnionRenderer,
    ): ValueOrErrors<
      Omit<SerializedBaseUnionRenderer, "renderer" | "cases"> & {
        renderer: string;
        cases: Record<string, SerializedBaseRenderer>;
      },
      string
    > => {
      if (!BaseUnionRenderer.Operations.hasRenderers(serialized))
        return ValueOrErrors.Default.throwOne(
          `renderer and cases are required`,
        );
      if (typeof serialized.cases != "object") {
        return ValueOrErrors.Default.throwOne(`cases must be an object`);
      }
      if (Object.keys(serialized.cases).length == 0) {
        return ValueOrErrors.Default.throwOne(
          `cases must have at least one case`,
        );
      }
      if (
        Object.values(serialized.cases).some(
          (caseProp) => typeof caseProp != "object",
        )
      ) {
        return ValueOrErrors.Default.throwOne(`cases must be objects`);
      }
      const cases = serialized.cases as Record<string, SerializedBaseRenderer>;

      return ValueOrErrors.Default.return({
        ...serialized,
        cases: cases,
      });
    },
    Deserialize: <T>(
      type: UnionType<T>,
      serialized: SerializedBaseUnionRenderer,
      fieldViews: any,
      renderingContext: ParentContext,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<BaseUnionRenderer<T>, string> =>
      BaseUnionRenderer.Operations.tryAsValidBaseUnionRenderer(serialized)
        .Then((renderer) =>
          BaseRenderer.Operations.ComputeVisibility(
            renderer.visible,
            renderingContext,
          ).Then((visibleExpr) =>
            BaseRenderer.Operations.ComputeDisabled(
              renderer.disabled,
              renderingContext,
            ).Then((disabledExpr) =>
              ValueOrErrors.Operations.All(
                List<
                  ValueOrErrors<
                    [
                      string,
                      (
                        | BaseRenderer<T>
                        | TableFormRenderer<T>
                        | RecordFormRenderer<T>
                      ),
                    ],
                    string
                  >
                >(
                  Object.entries(renderer.cases).map(([caseName, caseProp]) => {
                    const caseType = type.args.get(caseName);
                    if (!caseType) {
                      return ValueOrErrors.Default.throwOne(
                        `case ${caseName} not found in union`,
                      );
                    }
                    return BaseRenderer.Operations.DeserializeAs(
                      caseType.fields,
                      caseProp,
                      fieldViews,
                      "nested",
                      `case ${caseName}`,
                      types,
                    ).Then((deserializedCase) =>
                      ValueOrErrors.Default.return([
                        caseName,
                        deserializedCase,
                      ]),
                    );
                  }),
                ),
              ).Then((deserializedCases) =>
                ValueOrErrors.Default.return(
                  BaseUnionRenderer.Default(
                    type,
                    renderer.renderer,
                    Map<
                      string,
                      | BaseRenderer<T>
                      | TableFormRenderer<T>
                      | RecordFormRenderer<T>
                    >(deserializedCases),
                    visibleExpr,
                    disabledExpr,
                    renderer.label,
                    renderer.tooltip,
                    renderer.details,
                  ),
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Union renderer`),
        ),
  },
};
