import { List, Map } from "immutable";

import {
  Bindings,
  DispatcherContext,
  DispatchTupleAbstractRenderer,
  PredicateValue,
  Template,
  ValueOrErrors,
  ValueTuple,
} from "../../../../../../../../../main";

import { NestedDispatcher } from "../../state";
import { TupleType } from "../../../../../deserializer/domains/specification/domains/types/state";
import { NestedTupleRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/nestedRenderer/domains/tuple/state";
import { RecordFieldTupleRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/domains/recordFieldRenderer/domains/tuple/state";

export const NestedTupleDispatcher = {
  Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
    type: TupleType<T>,
    renderer: RecordFieldTupleRenderer<T> | NestedTupleRenderer<T>,
    dispatcherContext: DispatcherContext<T>,
  ): ValueOrErrors<Template<any, any, any, any>, string> =>
    ValueOrErrors.Operations.All(
      List<ValueOrErrors<[number, Template<any, any, any, any>], string>>(
        type.args.map((arg, index) =>
          NestedDispatcher.Operations.DispatchAs(
            arg,
            renderer.itemRenderers[index],
            dispatcherContext,
            `Item ${index + 1}`,
          ).Then((template) => ValueOrErrors.Default.return([index, template])),
        ),
      ),
    )
      .Then((templates) =>
        ValueOrErrors.Operations.All(
          List<ValueOrErrors<[number, any], string>>(
            type.args.map((arg, index) =>
              dispatcherContext
                .defaultState(arg, renderer.itemRenderers[index])
                .Then((defaultState) =>
                  ValueOrErrors.Default.return([index, defaultState]),
                ),
            ),
          ),
        ).Then((ItemDefaultStates) =>
          dispatcherContext
            .getConcreteRenderer("tuple", renderer.concreteRendererName)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                DispatchTupleAbstractRenderer(
                  Map(ItemDefaultStates).map((state) => () => state),
                  Map(templates),
                )
                  .mapContext((_: any) => ({
                    ..._,
                    type: renderer.type,
                    label: renderer.label,
                    tooltip: renderer.tooltip,
                    details: renderer.details,
                  }))
                  .withView(concreteRenderer),
              ),
            ),
        ),
      )
      .MapErrors((errors) =>
        errors.map((error) => `${error}\n...When dispatching nested tuple`),
      ),
};
