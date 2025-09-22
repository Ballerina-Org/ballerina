import {
  BasicFun,
  Guid,
  OneAbstractRenderer,
  DispatchInjectablesTypes,
  Template,
  ValueOrErrors,
  DispatchParsedType,
  LookupTypeAbstractRenderer,
} from "../../../../../../../../../main";
import { OneRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/one/state";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { DispatcherContextWithApiSources } from "../../../../coroutines/runner";

export const OneDispatcher = {
  Operations: {
    DispatchPreviewRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: OneRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<undefined | Template<any, any, any, any>, string> =>
      renderer.previewRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedDispatcher.Operations.DispatchAs(
            renderer.previewRenderer,
            dispatcherContext,
            "previewRenderer",
            isInlined,
            tableApi,
          ),
    GetApi: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      api: string[],
      dispatcherContext: DispatcherContextWithApiSources<
        any,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
    ): ValueOrErrors<BasicFun<Guid, Promise<any>> | undefined, string> =>
      Array.isArray(api) &&
      api.length == 2 &&
      api.every((_) => typeof _ == "string")
        ? dispatcherContext.specApis.lookups == undefined
          ? ValueOrErrors.Default.return(undefined)
          : dispatcherContext.specApis.lookups.get(api[0]) == undefined
            ? ValueOrErrors.Default.return(undefined)
            : dispatcherContext.specApis.lookups.get(api[0])?.one == undefined
              ? ValueOrErrors.Default.return(undefined)
              : dispatcherContext.specApis.lookups.get(api[0])?.one.get(api[1])
                    ?.methods.get == false
                ? ValueOrErrors.Default.return(undefined)
                : dispatcherContext.lookupSources == undefined
                  ? ValueOrErrors.Default.throwOne(
                      `lookup api sources are undefined`,
                    )
                  : dispatcherContext
                      .lookupSources(api[0])
                      .Then((lookupSource) =>
                        lookupSource.one == undefined
                          ? ValueOrErrors.Default.throwOne(
                              `lookup source missing "one" api`,
                            )
                          : lookupSource
                              .one(api[1])
                              .Then((source) =>
                                ValueOrErrors.Default.return(source.get),
                              ),
                      )
        : ValueOrErrors.Default.throwOne(
            `api must be a string or an array of strings`,
          ),
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: OneRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      DispatchParsedType.Operations.ResolveLookupType(
        renderer.type.arg.name,
        dispatcherContext.types,
      ).Then((oneEntityType) =>
        oneEntityType.kind != "record"
          ? ValueOrErrors.Default.throwOne(
              `expected a record type, but got a ${oneEntityType.kind} type`,
            )
          : OneDispatcher.Operations.DispatchPreviewRenderer(
              renderer,
              dispatcherContext,
              isInlined,
              tableApi,
            ).Then((previewRenderer) =>
              NestedDispatcher.Operations.DispatchAs(
                renderer.detailsRenderer,
                dispatcherContext,
                "detailsRenderer",
                isInlined,
                tableApi,
              ).Then((detailsRenderer) =>
                OneDispatcher.Operations.GetApi(
                  renderer.api,
                  dispatcherContext,
                ).Then((getApi) =>
                  dispatcherContext
                    .getConcreteRenderer("one", renderer.concreteRenderer)
                    .Then((concreteRenderer) =>
                      ValueOrErrors.Default.return(
                        OneAbstractRenderer(
                          LookupTypeAbstractRenderer<
                            CustomPresentationContexts,
                            Flags,
                            ExtraContext
                          >(
                            detailsRenderer,
                            renderer.type.arg,
                            dispatcherContext.IdProvider,
                            dispatcherContext.ErrorRenderer,
                          ).withView(dispatcherContext.lookupTypeRenderer()),
                          previewRenderer
                            ? LookupTypeAbstractRenderer<
                                CustomPresentationContexts,
                                Flags,
                                ExtraContext
                              >(
                                previewRenderer,
                                renderer.type.arg,
                                dispatcherContext.IdProvider,
                                dispatcherContext.ErrorRenderer,
                              ).withView(dispatcherContext.lookupTypeRenderer())
                            : undefined,
                          dispatcherContext.IdProvider,
                          dispatcherContext.ErrorRenderer,
                          oneEntityType,
                        )
                          .mapContext((_: any) => ({
                            ..._,
                            getApi,
                            fromApiParser: dispatcherContext.parseFromApiByType(
                              renderer.type.arg,
                            ),
                          }))
                          .withView(concreteRenderer),
                      ),
                    ),
                ),
              ),
            ),
      ),
  },
};
