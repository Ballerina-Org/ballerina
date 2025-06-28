import {
  DispatchParsedType,
  ListType,
  StringSerializedType,
} from "../../../../../deserializer/domains/specification/domains/types/state";
import { DispatcherContext } from "../../../../../deserializer/state";
import {
  Dispatcher,
  DispatchInjectablesTypes,
  ListAbstractRenderer,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { ListRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/list/state";
import { NestedDispatcher } from "../nestedDispatcher/state";

//TODO check type
export const ListDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: ListRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      NestedDispatcher.Operations.DispatchAs(
        renderer.elementRenderer,
        dispatcherContext,
        "listElement",
        isInlined,
        tableApi,
      )
        .Then((elementTemplate) =>
          dispatcherContext
            .defaultState(
              renderer.type.args[0],
              renderer.elementRenderer.renderer,
            )
            .Then((defaultElementState) =>
              dispatcherContext
                .defaultValue(
                  renderer.type.args[0],
                  renderer.elementRenderer.renderer,
                )
                .Then((defaultElementValue) =>
                  dispatcherContext
                    .getConcreteRenderer(
                      "list",
                      renderer.concreteRenderer,
                    )
                    .Then((concreteRenderer) =>
                      ValueOrErrors.Default.return<
                        [Template<any, any, any, any>, StringSerializedType],
                        string
                      >([
                        ListAbstractRenderer(
                          () => defaultElementState,
                          () => defaultElementValue,
                          elementTemplate[0],
                          dispatcherContext.IdProvider,
                          dispatcherContext.ErrorRenderer,
                        ).withView(concreteRenderer),
                        ListType.SerializeToString([elementTemplate[1]]),
                      ]),
                    ),
                ),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested list`),
        ),
  },
};
