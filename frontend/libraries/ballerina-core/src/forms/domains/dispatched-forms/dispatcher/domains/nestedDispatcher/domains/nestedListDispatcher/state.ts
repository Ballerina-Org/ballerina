import { ListType } from "../../../../../deserializer/domains/specification/domains/types/state";
import { DispatcherContext } from "../../../../../deserializer/state";
import {
  ListAbstractRenderer,
  NestedDispatcher,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { RecordFieldListRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/domains/recordFieldRenderer/domains/list/state";
import { NestedListRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/nestedRenderer/domains/list/state";

export const NestedListDispatcher = {
  Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
    type: ListType<T>,
    renderer: RecordFieldListRenderer<T> | NestedListRenderer<T>,
    dispatcherContext: DispatcherContext<T>,
  ): ValueOrErrors<Template<any, any, any, any>, string> =>
    NestedDispatcher.Operations.DispatchAs(
      type.args[0],
      renderer.elementRenderer,
      dispatcherContext,
      "element",
    )
      .Then((elementTemplate) =>
        dispatcherContext
          .defaultState(type.args[0], renderer.elementRenderer)
          .Then((defaultElementState) =>
            dispatcherContext
              .defaultValue(type.args[0], renderer.elementRenderer)
              .Then((defaultElementValue) =>
                dispatcherContext
                  .getConcreteRenderer("list", renderer.concreteRendererName)
                  .Then((concreteRenderer) =>
                    ValueOrErrors.Default.return(
                      ListAbstractRenderer(
                        () => defaultElementState,
                        () => defaultElementValue,
                        elementTemplate,
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
          ),
      )
      .MapErrors((errors) =>
        errors.map((error) => `${error}\n...When dispatching nested list`),
      ),
};
