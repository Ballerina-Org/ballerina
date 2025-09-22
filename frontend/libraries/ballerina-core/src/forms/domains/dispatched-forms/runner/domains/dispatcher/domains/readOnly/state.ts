import {
  DispatchInjectablesTypes,
  ReadOnlyAbstractRenderer,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { ReadOnlyRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/readOnly/state";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { DispatcherContextWithApiSources } from "../../../../coroutines/runner";

export const ReadOnlyDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: ReadOnlyRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
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
            .defaultState(renderer.type.arg, renderer.childRenderer.renderer)
            .Then((defaultChildState) =>
              dispatcherContext
                .getConcreteRenderer("readOnly", renderer.concreteRenderer)
                .Then((concreteRenderer) =>
                  ValueOrErrors.Default.return(
                    ReadOnlyAbstractRenderer(
                      () => defaultChildState,
                      childTemplate,
                      dispatcherContext.IdProvider,
                      dispatcherContext.ErrorRenderer,
                    ).withView(concreteRenderer),
                  ),
                ),
            ),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When dispatching nested readOnly`,
          ),
        ),
  },
};
