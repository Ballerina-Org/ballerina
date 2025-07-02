import { List, Map, Set } from "immutable";
import {
  ConcreteRenderers,
  ErrorRendererProps,
  Guid,
  IdWrapperProps,
  PredicateValue,
  StringSerializedType,
  Template,
  Unit,
  ValueOrErrors,
  View,
} from "../../../../../../../../main";

export type InjectedAbstractRenderer = (
  IdWrapper: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => Template<any, any, any, any>;

export type DispatchInjectablePrimitive<T extends DispatchInjectablesTypes<T>> =
  {
    name: keyof T;
    defaultValue: PredicateValue;
    abstractRenderer: InjectedAbstractRenderer;
    defaultState?: any;
  };

export type DispatchInjectedPrimitive<T> = {
  name: Guid;
  renderers: Set<keyof T>;
  defaultValue: PredicateValue;
  abstractRenderer: InjectedAbstractRenderer;
  defaultState: any;
};

export const DispatchInjectedPrimitive = {
  Default: <T extends DispatchInjectablesTypes<T>>(
    name: keyof T,
    abstractRenderer: InjectedAbstractRenderer,
    defaultValue: PredicateValue,
    defaultState?: any,
  ): DispatchInjectablePrimitive<T> => ({
    name,
    defaultValue,
    abstractRenderer,
    defaultState,
  }),
};

export type DispatchInjectableType = {
  type: any;
  state: any;
  abstractRenderer: InjectedAbstractRenderer;
  view: any;
};

export type DispatchInjectablesTypes<T> = {
  [key in keyof T]: DispatchInjectableType;
};

export type DispatchInjectables<T extends DispatchInjectablesTypes<T>> = Array<
  DispatchInjectablePrimitive<T>
>;

export type DispatchInjectedPrimitives<T> = Map<
  keyof T,
  DispatchInjectedPrimitive<T>
>;

export const injectedPrimitivesFromConcreteRenderers = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
>(
  concreteRenderers: ConcreteRenderers<
    T,
    Flags,
    CustomPresentationContexts,
    ExtraContext
  >,
  injectables: DispatchInjectables<T>,
): ValueOrErrors<DispatchInjectedPrimitives<T>, string> =>
  ValueOrErrors.Operations.All(
    List<ValueOrErrors<[keyof T, DispatchInjectedPrimitive<T>], string>>(
      injectables.map((injectable) =>
        concreteRenderers[injectable.name] == undefined
          ? ValueOrErrors.Default.throwOne<
              [keyof T, DispatchInjectedPrimitive<T>],
              string
            >(
              `Injectable renderer kind ${
                injectable.name as string
              } not found in concreteRenderers`,
            )
          : typeof concreteRenderers[injectable.name] !== "object"
            ? ValueOrErrors.Default.throwOne<
                [keyof T, DispatchInjectedPrimitive<T>],
                string
              >(
                `Injectable renderer kind ${
                  injectable.name as string
                } is not an object in concreteRenderers`,
              )
            : ValueOrErrors.Default.return<
                [keyof T, DispatchInjectedPrimitive<T>],
                string
              >([
                injectable.name,
                {
                  name: injectable.name as string,
                  renderers: Set(
                    Object.keys(concreteRenderers[injectable.name]),
                  ) as Set<keyof T>,
                  defaultValue: injectable.defaultValue,
                  abstractRenderer: injectable.abstractRenderer,
                  defaultState: injectable.defaultState,
                },
              ]),
      ),
    ),
  ).Then((injectableTuples) =>
    ValueOrErrors.Default.return(Map(injectableTuples)),
  );
