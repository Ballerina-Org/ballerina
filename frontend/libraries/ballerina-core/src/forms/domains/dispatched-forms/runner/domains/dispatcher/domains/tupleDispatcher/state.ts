import { List, Map } from "immutable";

import {
  DispatcherContext,
  DispatchTupleAbstractRenderer,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";

import { TupleType } from "../../../../../deserializer/domains/specification/domains/types/state";
import { TupleRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/tuple/state";
import { NestedDispatcher } from "../nestedDispatcher/state";

export const TupleDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: TupleType<T>,
      renderer: TupleRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      ValueOrErrors.Operations.All(
        List<ValueOrErrors<[number, Template<any, any, any, any>], string>>(
          type.args.map((_, index) =>
            NestedDispatcher.Operations.DispatchAs(
              renderer.itemRenderers[index],
              dispatcherContext,
              `Item ${index + 1}`,
              `Item ${index + 1}`,
            ).Then((template) =>
              ValueOrErrors.Default.return([index, template]),
            ),
          ),
        ),
      )
        .Then((templates) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<[number, any], string>>(
              type.args.map((arg, index) =>
                dispatcherContext
                  .defaultState(arg, renderer.itemRenderers[index].renderer)
                  .Then((defaultState) =>
                    ValueOrErrors.Default.return([index, defaultState]),
                  ),
              ),
            ),
          ).Then((ItemDefaultStates) =>
            renderer.renderer.kind != "lookupRenderer"
              ? ValueOrErrors.Default.throwOne<
                  Template<any, any, any, any>,
                  string
                >(
                  `received non lookup renderer kind "${renderer.renderer.kind}" when resolving defaultState for tuple`,
                )
              : dispatcherContext
                  .getConcreteRenderer("tuple", renderer.renderer.renderer)
                  .Then((concreteRenderer) =>
                    ValueOrErrors.Default.return(
                      DispatchTupleAbstractRenderer(
                        Map(ItemDefaultStates).map((state) => () => state),
                        Map(templates),
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
