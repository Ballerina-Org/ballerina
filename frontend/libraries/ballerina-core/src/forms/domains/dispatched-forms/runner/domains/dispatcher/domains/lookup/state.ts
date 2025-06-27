import { Map } from "immutable";
import {
  DispatchInjectablesTypes,
  LookupType,
  LookupTypeAbstractRenderer,
  MapRepo,
  StringSerializedType,
  Template,
} from "../../../../../../../../../main";
import {
  DispatcherContext,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { ConcreteLookupRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/concrete-lookup/state";
import { Renderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/state";
import { Dispatcher } from "../../state";

export const LookupDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: ConcreteLookupRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts
      >,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      MapRepo.Operations.tryFindWithError(
        renderer.renderer,
        dispatcherContext.forms,
        () => `cannot find renderer "${renderer.renderer}"`,
      )
        .Then((resolvedRenderer) =>
          Dispatcher.Operations.Dispatch(
            resolvedRenderer,
            dispatcherContext,
            true,
            renderer.renderer,
            undefined,
            renderer.api,
          ),
        )
        .Then((template) =>
          ValueOrErrors.Default.return<
            [Template<any, any, any, any>, StringSerializedType],
            string
          >([
            LookupTypeAbstractRenderer(
              template[0],
              dispatcherContext.IdProvider,
              dispatcherContext.ErrorRenderer,
            ).withView(dispatcherContext.lookupTypeRenderer()),
            LookupType.SerializeToString(renderer.type.name as string),
          ]),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When dispatching lookup renderer`,
          ),
        ),
  },
};
