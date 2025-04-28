import { List } from "immutable";
import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../../../main";
import {
  RecordFieldBaseRenderer,
  BaseSerializedRecordFieldRenderer,
} from "../../state";
export type SerializedEnumRecordFieldRenderer = {
  options?: unknown;
} & BaseSerializedRecordFieldRenderer;
import { SingleSelectionType } from "../../../../../../../../../types/state";
import { MultiSelectionType } from "../../../../../../../../../types/state";

export type RecordFieldEnumRenderer<T> = RecordFieldBaseRenderer<T> & {
  kind: "recordFieldEnumRenderer";
  options: string;
  type: SingleSelectionType<T> | MultiSelectionType<T>;
  concreteRendererName: string;
};

export const RecordFieldEnumRenderer = {
  Default: <T>(
    fieldName: string,
    type: SingleSelectionType<T> | MultiSelectionType<T>,
    options: string,
    concreteRendererName: string,
    visible: Expr,
    disabled: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): RecordFieldEnumRenderer<T> => ({
    kind: "recordFieldEnumRenderer",
    fieldName,
    type,
    options,
    concreteRendererName,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderer: (
      serialized: SerializedEnumRecordFieldRenderer,
    ): serialized is SerializedEnumRecordFieldRenderer & {
      renderer: string;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string",

    hasOptions: (
      serialized: SerializedEnumRecordFieldRenderer,
    ): serialized is SerializedEnumRecordFieldRenderer & {
      options: string;
    } =>
      serialized.options != undefined && typeof serialized.options == "string",
    tryAsValidEnumRecordField: (
      fieldName: string,
      serialized: SerializedEnumRecordFieldRenderer,
    ): ValueOrErrors<
      Omit<SerializedEnumRecordFieldRenderer, "renderer" | "options"> & {
        renderer: string;
        options: string;
      },
      string
    > =>
      !RecordFieldEnumRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(
            `renderer is required for field ${fieldName}`,
          )
        : !RecordFieldEnumRenderer.Operations.hasOptions(serialized)
          ? ValueOrErrors.Default.throwOne(
              `options are required for field ${fieldName}`,
            )
          : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SingleSelectionType<T> | MultiSelectionType<T>,
      fieldName: string,
      serialized: SerializedEnumRecordFieldRenderer,
    ): ValueOrErrors<RecordFieldEnumRenderer<T>, string> =>
      RecordFieldEnumRenderer.Operations.tryAsValidEnumRecordField(
        fieldName,
        serialized,
      )
        .Then((enumRecordFieldRenderer) =>
          Expr.Operations.parseAsVisibilityExpression(
            enumRecordFieldRenderer.visible ?? true,
          ).Then((visibilityExpr) =>
            Expr.Operations.parseAsDisabledExpression(
              enumRecordFieldRenderer.disabled ?? false,
            ).Then((disabledExpr) => {
              return ValueOrErrors.Default.return(
                RecordFieldEnumRenderer.Default(
                  fieldName,
                  type,
                  enumRecordFieldRenderer.options,
                  enumRecordFieldRenderer.renderer,
                  visibilityExpr,
                  disabledExpr,
                  enumRecordFieldRenderer.label,
                  enumRecordFieldRenderer.tooltip,
                  enumRecordFieldRenderer.details,
                ),
              );
            }),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Enum renderer`),
        ),
  },
};
