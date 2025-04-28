import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../../../main";
import { UnionType } from "../../../../../../../../../types/state";

import {
  RecordFieldBaseRenderer,
  BaseSerializedRecordFieldRenderer,
  SerializedRecordFieldRenderer,
  RecordFieldRenderer,
} from "../../state";
import { List, Map } from "immutable";

export type SerializedUnionRecordFieldRenderer = {
  cases?: unknown;
} & BaseSerializedRecordFieldRenderer;

export type RecordFieldUnionRenderer<T> = RecordFieldBaseRenderer<T> & {
  kind: "recordFieldUnionRenderer";
  cases: Map<string, RecordFieldRenderer<T>>;
  type: UnionType<T>;
  concreteRendererName: string;
};

export const RecordFieldUnionRenderer = {
  Default: <T>(
    type: UnionType<T>,
    fieldName: string,
    concreteRendererName: string,
    cases: Map<string, RecordFieldRenderer<T>>,
    visible: Expr,
    disabled: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): RecordFieldUnionRenderer<T> => ({
    kind: "recordFieldUnionRenderer",
    type,
    fieldName,
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
      serialized: SerializedUnionRecordFieldRenderer,
    ): serialized is SerializedUnionRecordFieldRenderer & {
      renderer: string;
      cases: Record<string, SerializedRecordFieldRenderer>;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string" &&
      serialized.cases != undefined,
    tryAsValidUnionRecordFieldRenderer: (
      fieldName: string,
      serialized: SerializedUnionRecordFieldRenderer,
    ): ValueOrErrors<
      Omit<SerializedUnionRecordFieldRenderer, "renderer" | "cases"> & {
        renderer: string;
        cases: Record<string, SerializedRecordFieldRenderer>;
      },
      string
    > => {
      if (!RecordFieldUnionRenderer.Operations.hasRenderers(serialized))
        return ValueOrErrors.Default.throwOne(
          `renderer and cases are required for renderer ${fieldName}`,
        );
      if (typeof serialized.cases != "object") {
        return ValueOrErrors.Default.throwOne(
          `cases must be an object for renderer ${fieldName}`,
        );
      }
      if (Object.keys(serialized.cases).length == 0) {
        return ValueOrErrors.Default.throwOne(
          `cases must have at least one case for renderer ${fieldName}`,
        );
      }
      if (
        Object.values(serialized.cases).some(
          (caseProp) => typeof caseProp != "object",
        )
      ) {
        return ValueOrErrors.Default.throwOne(
          `cases must be objects for renderer ${fieldName}`,
        );
      }
      const cases = serialized.cases as Record<
        string,
        SerializedRecordFieldRenderer
      >;

      return ValueOrErrors.Default.return({
        ...serialized,
        cases: cases,
      });
    },
    Deserialize: <T>(
      type: UnionType<T>,
      fieldName: string,
      serialized: SerializedUnionRecordFieldRenderer,
      fieldViews: any,
    ): ValueOrErrors<RecordFieldUnionRenderer<T>, string> =>
      RecordFieldUnionRenderer.Operations.tryAsValidUnionRecordFieldRenderer(
        fieldName,
        serialized,
      )
        .Then((serializedUnionRecordFieldRenderer) =>
          Expr.Operations.parseAsVisibilityExpression(
            serializedUnionRecordFieldRenderer.visible,
          ).Then((visibleExpr) =>
            Expr.Operations.parseAsDisabledExpression(
              serializedUnionRecordFieldRenderer.disabled,
            ).Then((disabledExpr) =>
              ValueOrErrors.Operations.All(
                List<ValueOrErrors<[string, RecordFieldRenderer<T>], string>>(
                  Object.entries(serializedUnionRecordFieldRenderer.cases).map(
                    ([caseName, caseProp]) => {
                      const caseType = type.args.get(caseName);
                      if (!caseType) {
                        return ValueOrErrors.Default.throwOne(
                          `case ${caseName} not found in union ${fieldName}`,
                        );
                      }
                      return RecordFieldRenderer.Operations.Deserialize(
                        caseType.fields,
                        `Union Case ${caseName}`,
                        caseProp,
                        fieldViews,
                      ).Then((deserializedCase) =>
                        ValueOrErrors.Default.return([
                          caseName,
                          deserializedCase,
                        ]),
                      );
                    },
                  ),
                ),
              ).Then((deserializedCases) =>
                ValueOrErrors.Default.return(
                  RecordFieldUnionRenderer.Default(
                    type,
                    fieldName,
                    serializedUnionRecordFieldRenderer.renderer,
                    Map<string, RecordFieldRenderer<T>>(deserializedCases),
                    visibleExpr,
                    disabledExpr,
                    serializedUnionRecordFieldRenderer.label,
                    serializedUnionRecordFieldRenderer.tooltip,
                    serializedUnionRecordFieldRenderer.details,
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
