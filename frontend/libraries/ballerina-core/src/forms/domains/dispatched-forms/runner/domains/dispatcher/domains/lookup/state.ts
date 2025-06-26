import { Map } from "immutable";
import { DispatchInjectablesTypes, MapRepo, Template } from "../../../../../../../../../main";
import {
  DispatcherContext,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { LookupRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/lookup/state";
import { Renderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/state";
import { Dispatcher } from "../../state";

export const LookupDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: LookupRenderer<T>,
      dispatcherContext: DispatcherContext<T, Flags, CustomPresentationContexts>,
      renderers: Map<string, Renderer<T>>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      MapRepo.Operations.tryFindWithError(
        renderer.renderer,
        renderers,
        () => `cannot find renderer "${renderer.renderer}"`,
      )
        .Then((resolvedRenderer) =>
          Dispatcher.Operations.Dispatch(
            resolvedRenderer.type,
            resolvedRenderer,
            dispatcherContext,
            true,
            renderer.renderer,
            undefined,
            renderer.api,
          ),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When dispatching lookup renderer`,
          ),
        ),
  },
};
