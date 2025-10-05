import {
  DispatchInjectablesTypes,
  LookupType,
  LookupTypeAbstractRenderer,
  Template,
} from "../../../../../../../../../main";
import { ValueOrErrors } from "../../../../../../../../../main";
import { LookupRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/lookup/state";
import { Dispatcher } from "../../state";
import { DispatcherContextWithApiSources } from "../../../../state";

export const LookupDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: LookupRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      tableApi: string | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
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
            .Then((template) =>
              ValueOrErrors.Default.return(
                LookupTypeAbstractRenderer<
                  CustomPresentationContext,
                  Flags,
                  ExtraContext
                >(
                  template,
                  renderer.type,
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                )
                  .mapContext((_: any) => ({
                    ..._,
                    type: renderer.type,
                  }))
                  .withView(dispatcherContext.lookupTypeRenderer()),
              ),
            )
            .MapErrors((errors) =>
              errors.map(
                (error) => `${error}\n...When dispatching lookup renderer`,
              ),
            ),
  },
};
