import { List, Map } from "immutable";

import {
  DispatcherContext,
  DispatchTupleAbstractRenderer,
  Template,
  ValueOrErrors,
  DispatchInjectablesTypes,
} from "../../../../../../../../../main";

import { TupleType } from "../../../../../deserializer/domains/specification/domains/types/state";
import { TupleRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/tuple/state";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { DispatcherContextWithApiSources } from "../../../../coroutines/runner";

export const TupleDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: TupleRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      ValueOrErrors.Operations.All(
        List<ValueOrErrors<[number, Template<any, any, any, any>], string>>(
          renderer.type.args.map((_, index) =>
            NestedDispatcher.Operations.DispatchAs(
              renderer.itemRenderers[index],
              dispatcherContext,
              `Item ${index + 1}`,
              isInlined,
              tableApi,
            ).Then((template) =>
              ValueOrErrors.Default.return([index, template]),
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
              .Then((concreteRenderer) =>
                ValueOrErrors.Default.return(
                  DispatchTupleAbstractRenderer(
                    Map(ItemDefaultStates).map((state) => () => state),
                    Map(
                      templates.map((template) => [template[0], template[1]]),
                    ),
                    dispatcherContext.IdProvider,
                    dispatcherContext.ErrorRenderer,
                  ).withView(concreteRenderer),
                ),
              ),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested tuple`),
        ),
  },
};
