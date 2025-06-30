import {
  DispatchInjectablesTypes,
  LookupType,
  LookupTypeAbstractRenderer,
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
      ExtraContext,
    >(
      renderer: LookupRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      tableApi: string | undefined,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      renderer.kind == "inlinedType-lookupRenderer"
        ? ValueOrErrors.Default.throwOne(
            `inlined type lookup renderer should not have been dispatched to a lookup renderer`,
          )
        : LookupRenderer.Operations.ResolveRenderer(
            renderer,
            dispatcherContext.forms,
          )
            .Then((resolvedRenderer) =>
              Dispatcher.Operations.Dispatch(
                resolvedRenderer,
                dispatcherContext,
                true,
                renderer.kind == "lookupType-inlinedRenderer",
                renderer.tableApi ?? tableApi,
              ),
            )
            .Then((template) => {
              const serializedType = LookupType.SerializeToString(
                renderer.type.name,
              );
              return ValueOrErrors.Default.return<
                [Template<any, any, any, any>, StringSerializedType],
                string
              >([
                LookupTypeAbstractRenderer<
                  CustomPresentationContexts,
                  Flags,
                  ExtraContext
                >(
                  template[0],
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                  serializedType,
                ).withView(dispatcherContext.lookupTypeRenderer()),
                serializedType,
              ]);
            })
            .MapErrors((errors) =>
              errors.map(
                (error) => `${error}\n...When dispatching lookup renderer`,
              ),
            ),
  },
};
