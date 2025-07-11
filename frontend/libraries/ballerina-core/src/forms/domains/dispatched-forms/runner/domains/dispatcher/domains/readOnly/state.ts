import { DispatcherContext } from "../../../../../deserializer/state";
import {
  DispatchInjectablesTypes,
  ReadOnlyAbstractRenderer,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { ReadOnlyRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/readOnly/state";
import { NestedDispatcher } from "../nestedDispatcher/state";

export const ReadOnlyDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: ReadOnlyRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.DispatchAs(
        renderer.childRenderer,
        dispatcherContext,
        "readOnlyChild",
        isInlined,
        tableApi,
      )
        .Then((childTemplate) =>
          dispatcherContext
            .getConcreteRenderer("readOnly", renderer.concreteRenderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                ReadOnlyAbstractRenderer(
                  childTemplate,
                  renderer.type,
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
              ),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested readOnly`),
        ),
  },
}; 