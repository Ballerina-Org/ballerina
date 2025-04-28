import { Map, Set } from "immutable";
import { Unit } from "../../../../../../main";

export type InjectablePrimitive<T, U = Unit> = {
  defaultValue: T;
  fieldView: any;
  defaultState?: U;
};

export type InjectedPrimitive<T> = {
  renderers: Set<keyof T>;
  defaultValue: any;
  abstractRenderer: any;
  defaultState: any;
};

export type Injectables<
  T extends { [key in keyof T]: { type: any; state: any } },
> = Map<keyof T, InjectablePrimitive<T[keyof T]["type"], T[keyof T]["state"]>>;

export type InjectedPrimitives<T> = {
  injectedPrimitives: Map<keyof T, InjectedPrimitive<T>>;
  renderers: {
    [key in keyof T]: Set<string>;
  };
};

export const injectablesFromFieldViews = <
  T extends { [key in keyof T]: { type: any; state: any } },
>(
  fieldViews: any,
  injectables: Injectables<T>,
): InjectedPrimitives<T> => {
  let result = {
    injectedPrimitives: Map<string, InjectedPrimitive<T>>(),
    renderers: {},
  } as InjectedPrimitives<T>;
  result.injectedPrimitives = injectables.map(
    (injectable, key) =>
      ({
        renderers: Set([key]),
        defaultValue: injectable.defaultValue,
        abstractRenderer: injectable.fieldView,
        defaultState: injectable.defaultState,
      }) as InjectedPrimitive<T>,
  );
  result.renderers = {
    ...injectables
      .map((_, key) => ({ [key]: Set<string>() }))
      .valueSeq()
      .toArray()
      .reduce((acc, x) => ({ ...acc, ...x }), {}),
  } as InjectedPrimitives<T>["renderers"];
  Object.keys(result.renderers).forEach((_categoryName) => {
    const categoryName = _categoryName;
    if (categoryName in fieldViews) {
      Object.keys(fieldViews[categoryName]).forEach((viewName) => {
        result.renderers[categoryName as keyof T] =
          result.renderers[categoryName as keyof T].add(viewName);
      });
    }
  });
  return result;
};
