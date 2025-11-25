import {
  BasicFun,
  Guid,
  OneAbstractRenderer,
  DispatchInjectablesTypes,
  Template,
  ValueOrErrors,
  DispatchParsedType,
} from "../../../../../../../../../main";
import { OneRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/one/state";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { DispatcherContextWithApiSources } from "../../../../state";

export const OneDispatcher = {
  Operations: {
    DispatchPreviewRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: OneRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      isInlined: boolean,
    ): ValueOrErrors<undefined | Template<any, any, any, any>, string> =>
      renderer.previewRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedDispatcher.Operations.DispatchAs(
            renderer.previewRenderer,
            dispatcherContext,
            "previewRenderer",
            isInlined,
          ),
    GetApi: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      api: string[],
      dispatcherContext: DispatcherContextWithApiSources<
        any,
        Flags,
        CustomPresentationContext,
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
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: OneRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      isInlined: boolean,
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
            ).Then((previewRenderer) =>
              NestedDispatcher.Operations.DispatchAs(
                renderer.detailsRenderer,
                dispatcherContext,
                "detailsRenderer",
                isInlined,
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
                          detailsRenderer,
                          previewRenderer,
                          dispatcherContext.IdProvider,
                          dispatcherContext.ErrorRenderer,
                          renderer.detailsRenderer,
                          renderer.previewRenderer,
                          oneEntityType,
                        )
                          .mapContext((_: any) => ({
                            ..._,
                            getApi,
                            fromApiParser: dispatcherContext.parseFromApiByType(
                              renderer.type.arg,
                            ),
                            type: renderer.type,
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
