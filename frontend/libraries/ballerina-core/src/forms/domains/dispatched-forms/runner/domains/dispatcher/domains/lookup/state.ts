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
import { LookupRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/lookup/state";
import { Dispatcher } from "../../state";

export const LookupDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: LookupRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts
      >,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      LookupRenderer.Operations.ResolveRenderer(
        renderer,
        dispatcherContext.forms,
      )
        .Then((resolvedRenderer) =>
          Dispatcher.Operations.Dispatch(
            resolvedRenderer,
            dispatcherContext,
            true,
            renderer.renderer.kind == "inlinedFormLookup"
              ? `inlined ${renderer.type.name as string}`
              : renderer.renderer.renderer,
            undefined,
            renderer.tableApi,
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
