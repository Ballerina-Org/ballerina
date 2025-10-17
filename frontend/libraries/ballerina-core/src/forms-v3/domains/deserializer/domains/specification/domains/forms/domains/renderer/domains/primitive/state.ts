import {
  DispatchInjectablesTypes,
  DispatchPrimitiveType,
  isString,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";

export type SerializedPrimitive = string;

export const Primitive = {
  Default: <T>(renderer: string): SerializedPrimitive => renderer,
};

export type PrimitiveRenderer<T> = {
  kind: "primitiveRenderer";
  type: DispatchPrimitiveType<T>;
  concreteRenderer: string;
};

export const PrimitiveRenderer = {
  Default: <T extends DispatchInjectablesTypes<T>>(
    type: DispatchPrimitiveType<T>,
    concreteRenderer: string,
  ): PrimitiveRenderer<T> => ({
    kind: "primitiveRenderer",
    type,
    concreteRenderer,
  }),
  Operations: {
    Deserialize: <T extends DispatchInjectablesTypes<T>>(
      type: DispatchPrimitiveType<T>,
      serialized: unknown,
    ): ValueOrErrors<PrimitiveRenderer<T>, string> =>
      isString(serialized)
        ? ValueOrErrors.Default.return(
            PrimitiveRenderer.Default(type, serialized),
          )
        : ValueOrErrors.Default.throwOne(
            `serialized primitive is not a string`,
          ),
  },
};
