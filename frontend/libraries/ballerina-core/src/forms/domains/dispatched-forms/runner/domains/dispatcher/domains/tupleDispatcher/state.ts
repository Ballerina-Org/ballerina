import { List, Map } from "immutable";

import {
  DispatcherContext,
  DispatchTupleAbstractRenderer,
  Template,
  ValueOrErrors,
  DispatchInjectablesTypes,
} from "../../../../../../../../../main";

import {
  StringSerializedType,
  TupleType,
} from "../../../../../deserializer/domains/specification/domains/types/state";
import { TupleRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/tuple/state";
import { NestedDispatcher } from "../nestedDispatcher/state";

export const TupleDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: TupleRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      ValueOrErrors.Operations.All(
        List<
          ValueOrErrors<
            [number, Template<any, any, any, any>, StringSerializedType],
            string
          >
        >(
          renderer.type.args.map((_, index) =>
            NestedDispatcher.Operations.DispatchAs(
              renderer.itemRenderers[index],
              dispatcherContext,
              `Item ${index + 1}`,
              isInlined,
              tableApi,
            ).Then((template) =>
              ValueOrErrors.Default.return<
                [number, Template<any, any, any, any>, StringSerializedType],
                string
              >([index, template[0], template[1]]),
            ),
          ),
        ),
      )
        .Then((templates) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<[number, any], string>>(
              renderer.type.args.map((arg, index) =>
                dispatcherContext
                  .defaultState(arg, renderer.itemRenderers[index].renderer)
                  .Then((defaultState) =>
                    ValueOrErrors.Default.return([index, defaultState]),
                  ),
              ),
            ),
          ).Then((ItemDefaultStates) =>
            dispatcherContext
              .getConcreteRenderer("tuple", renderer.concreteRenderer)
              .Then((concreteRenderer) => {
                const serializedType = TupleType.SerializeToString(
                  templates.map((template) => template[2]).toArray(),
                );
                return ValueOrErrors.Default.return<
                  [Template<any, any, any, any>, StringSerializedType],
                  string
                >([
                  DispatchTupleAbstractRenderer(
                    Map(ItemDefaultStates).map((state) => () => state),
                    Map(
                      templates.map((template) => [template[0], template[1]]),
                    ),
                    dispatcherContext.IdProvider,
                    dispatcherContext.ErrorRenderer,
                    serializedType,
                  ).withView(concreteRenderer),
                  serializedType,
                ]);
              }),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested tuple`),
        ),
  },
};
