import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import {
  BaseBaseRenderer,
  BaseSerializedBaseRenderer,
  ParentContext,
  BaseRenderer,
} from "../../state";
import { SingleSelectionType } from "../../../../../../../types/state";
import { MultiSelectionType } from "../../../../../../../types/state";

export type SerializedEnumRecordFieldRenderer = {
  options?: unknown;
} & BaseSerializedBaseRenderer;

export type BaseEnumRenderer<T> = BaseBaseRenderer & {
  kind: "baseEnumRenderer";
  options: string;
  type: SingleSelectionType<T> | MultiSelectionType<T>;
  concreteRendererName: string;
};

export const BaseEnumRenderer = {
  Default: <T>(
    type: SingleSelectionType<T> | MultiSelectionType<T>,
    options: string,
    concreteRendererName: string,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseEnumRenderer<T> => ({
    kind: "baseEnumRenderer",
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
    tryAsValidEnumBaseRenderer: (
      serialized: SerializedEnumRecordFieldRenderer,
    ): ValueOrErrors<
      Omit<SerializedEnumRecordFieldRenderer, "renderer" | "options"> & {
        renderer: string;
        options: string;
      },
      string
    > =>
      !BaseEnumRenderer.Operations.hasRenderer(serialized)
        ? ValueOrErrors.Default.throwOne(`renderer is required`)
        : !BaseEnumRenderer.Operations.hasOptions(serialized)
          ? ValueOrErrors.Default.throwOne(`options are required`)
          : ValueOrErrors.Default.return(serialized),
    Deserialize: <T>(
      type: SingleSelectionType<T> | MultiSelectionType<T>,
      serialized: SerializedEnumRecordFieldRenderer,
      renderingContext: ParentContext,
    ): ValueOrErrors<BaseEnumRenderer<T>, string> =>
      BaseEnumRenderer.Operations.tryAsValidEnumBaseRenderer(serialized)
        .Then((renderer) =>
          BaseRenderer.Operations.ComputeVisibility(
            renderer.visible,
            renderingContext,
          ).Then((visibilityExpr) =>
            BaseRenderer.Operations.ComputeDisabled(
              renderer.disabled,
              renderingContext,
            ).Then((disabledExpr) =>
              ValueOrErrors.Default.return(
                BaseEnumRenderer.Default(
                  type,
                  renderer.options,
                  renderer.renderer,
                  visibilityExpr,
                  disabledExpr,
                  renderer.label,
                  renderer.tooltip,
                  renderer.details,
                ),
              ),
            ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Enum`),
        ),
  },
};
