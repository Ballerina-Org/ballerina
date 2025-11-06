import { Map } from "immutable";
import { NestedRenderer } from "../nestedRenderer/state";
import { DispatchParsedType, TupleType } from "../../../../../types/state";
import { Renderer } from "../../state";
import { ValueOrErrors } from "../../../../../../../../../../../../collections/domains/valueOrErrors/state";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  isObject,
  isString,
} from "../../../../../../../../../../../../../main";

export type SerializedTupleRenderer = {
  renderer: string;
  itemRenderers: Array<unknown>;
};

export type TupleRenderer<T> = {
  kind: "tupleRenderer";
  concreteRenderer: string;
  itemRenderers: Array<NestedRenderer<T>>;
  type: TupleType<T>;
};

export const TupleRenderer = {
  Default: <T>(
    type: TupleType<T>,
    concreteRenderer: string,
    itemRenderers: Array<NestedRenderer<T>>,
  ): TupleRenderer<T> => ({
    kind: "tupleRenderer",
    type,
    concreteRenderer,
    itemRenderers,
  }),
  Operations: {
    tryAsValidTupleRenderer: (
      serialized: unknown,
    ): ValueOrErrors<SerializedTupleRenderer, string> =>
      !isObject(serialized)
        ? ValueOrErrors.Default.throwOne(`serialized must be an object`)
        : !("renderer" in serialized)
          ? ValueOrErrors.Default.throwOne(`renderer is required`)
          : !isString(serialized.renderer)
            ? ValueOrErrors.Default.throwOne(`renderer must be a string`)
            : !("itemRenderers" in serialized)
              ? ValueOrErrors.Default.throwOne(`itemRenderers is required`)
              : !Array.isArray(serialized.itemRenderers)
                ? ValueOrErrors.Default.throwOne(
                    `itemRenderers must be an array`,
                  )
                : serialized.itemRenderers.length == 0
                  ? ValueOrErrors.Default.throwOne(
                      `itemRenderers must have at least one item`,
                    )
                  : ValueOrErrors.Default.return({
                      renderer: serialized.renderer,
                      itemRenderers: serialized.itemRenderers,
                    }),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      type: TupleType<T>,
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
    ): ValueOrErrors<[TupleRenderer<T>, Map<string, Renderer<T>>], string> =>
      TupleRenderer.Operations.tryAsValidTupleRenderer(serialized)
        .Then((validatedSerialized) =>
          validatedSerialized.itemRenderers
            .reduce<
              ValueOrErrors<
                [Array<NestedRenderer<T>>, Map<string, Renderer<T>>],
                string
              >
            >((acc, itemRenderer, index) => acc.Then(([itemRenderersArray, accumulatedAlreadyParsedForms]) => NestedRenderer.Operations.DeserializeAs(type.args[index], itemRenderer, concreteRenderers, `Item ${index + 1}`, types, forms, accumulatedAlreadyParsedForms).Then(([deserializedItemRenderer, newAlreadyParsedForms]) => ValueOrErrors.Default.return<[Array<NestedRenderer<T>>, Map<string, Renderer<T>>], string>([[...itemRenderersArray, deserializedItemRenderer], newAlreadyParsedForms]))), ValueOrErrors.Default.return<[Array<NestedRenderer<T>>, Map<string, Renderer<T>>], string>([[], alreadyParsedForms]))
            .Then(([itemRenderersArray, accumulatedAlreadyParsedForms]) =>
              ValueOrErrors.Default.return<
                [TupleRenderer<T>, Map<string, Renderer<T>>],
                string
              >([
                TupleRenderer.Default(
                  type,
                  validatedSerialized.renderer,
                  itemRenderersArray,
                ),
                accumulatedAlreadyParsedForms,
              ]),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When parsing as Tuple renderer`),
        ),
  },
};
