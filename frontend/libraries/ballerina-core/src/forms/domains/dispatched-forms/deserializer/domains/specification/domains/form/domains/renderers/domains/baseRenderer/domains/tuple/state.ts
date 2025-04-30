import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import { TupleType } from "../../../../../../../types/state";

import {
  BaseBaseRenderer,
  BaseSerializedBaseRenderer,
  BaseRenderer,
  SerializedBaseRenderer,
  ParentContext,
} from "../../state";
import { List } from "immutable";

export type SerializedBaseTupleRenderer = {
  itemRenderers?: unknown;
} & BaseSerializedBaseRenderer;

export type BaseTupleRenderer<T> = BaseBaseRenderer & {
  kind: "baseTupleRenderer";
  itemRenderers: Array<BaseRenderer<T>>;
  type: TupleType<T>;
  concreteRendererName: string;
};

export const BaseTupleRenderer = {
  Default: <T>(
    type: TupleType<T>,
    concreteRendererName: string,
    itemRenderers: Array<BaseRenderer<T>>,
    visible?: Expr,
    disabled?: Expr,
    label?: string,
    tooltip?: string,
    details?: string,
  ): BaseTupleRenderer<T> => ({
    kind: "baseTupleRenderer",
    type,
    concreteRendererName,
    itemRenderers,
    visible,
    disabled,
    label,
    tooltip,
    details,
  }),
  Operations: {
    hasRenderers: (
      serialized: SerializedBaseTupleRenderer,
    ): serialized is SerializedBaseTupleRenderer & {
      renderer: string;
      itemRenderers: Array<SerializedBaseRenderer>;
    } =>
      serialized.renderer != undefined &&
      typeof serialized.renderer == "string" &&
      serialized.itemRenderers != undefined,
    tryAsValidBaseTupleRenderer: (
      serialized: SerializedBaseTupleRenderer,
    ): ValueOrErrors<
      Omit<SerializedBaseTupleRenderer, "renderer" | "itemRenderers"> & {
        renderer: string;
        itemRenderers: Array<SerializedBaseRenderer>;
      },
      string
    > => {
      if (!BaseTupleRenderer.Operations.hasRenderers(serialized))
        return ValueOrErrors.Default.throwOne(
          `renderer and itemRenderers are required`,
        );
      if (!Array.isArray(serialized.itemRenderers)) {
        return ValueOrErrors.Default.throwOne(`itemRenderers must be an array`);
      }
      if (serialized.itemRenderers.length == 0) {
        return ValueOrErrors.Default.throwOne(
          `itemRenderers must have at least one item`,
        );
      }
      if (
        serialized.itemRenderers.some(
          (itemRenderer) => typeof itemRenderer != "object",
        )
      ) {
        return ValueOrErrors.Default.throwOne(`itemRenderers must be objects`);
      }
      const itemRenderers =
        serialized.itemRenderers as Array<SerializedBaseRenderer>;

      return ValueOrErrors.Default.return({
        ...serialized,
        itemRenderers: itemRenderers,
      });
    },
    Deserialize: <T>(
      type: TupleType<T>,
      serialized: SerializedBaseTupleRenderer,
      fieldViews: any,
      renderingContext: ParentContext,
    ): ValueOrErrors<BaseTupleRenderer<T>, string> =>
      BaseTupleRenderer.Operations.tryAsValidBaseTupleRenderer(serialized)
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
                List<ValueOrErrors<BaseRenderer<T>, string>>(
                  renderer.itemRenderers.map((itemRenderer, index) =>
                    BaseRenderer.Operations.DeserializeAs(
                      type.args[index],
                      itemRenderer,
                      fieldViews,
                      "nested",
                      `Item ${index + 1}`,
                    ).Then((deserializedItemRenderer) =>
                      ValueOrErrors.Default.return(deserializedItemRenderer),
                    ),
                  ),
                ),
              ).Then((deserializedItemRenderers) =>
                ValueOrErrors.Default.return(
                  BaseTupleRenderer.Default(
                    type,
                    renderer.renderer,
                    deserializedItemRenderers.toArray(),
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
          errors.map((error) => `${error}\n...When parsing as Tuple renderer`),
        ),
  },
};
