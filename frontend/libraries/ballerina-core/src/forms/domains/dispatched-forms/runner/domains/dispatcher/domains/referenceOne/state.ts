import {
  BasicFun,
  Guid,
  ReferenceOneAbstractRenderer,
  DispatchInjectablesTypes,
  Template,
  ValueOrErrors,
  DispatchParsedType,
} from "../../../../../../../../../main";
import { ReferenceOneRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/referenceOne/state";
import { NestedDispatcher } from "../nestedDispatcher/state";
import { DispatcherContextWithApiSources } from "../../../../state";

export const ReferenceOneDispatcher = {
  Operations: {
    DispatchPreviewRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: ReferenceOneRenderer<T>,
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
    DispatchDetailsRenderer: < //TODO Suzan: unused at time of writing; remove?
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: ReferenceOneRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      isInlined: boolean,
    ): ValueOrErrors<undefined | Template<any, any, any, any>, string> =>
      renderer.detailsRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedDispatcher.Operations.DispatchAs(
            renderer.detailsRenderer,
            dispatcherContext,
            "detailsRenderer",
            isInlined,
          ),
    GetApi: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      entityName: string,
      dispatcherContext: DispatcherContextWithApiSources<
        any,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
    ): ValueOrErrors<BasicFun<Guid, Promise<any>> | undefined, string> => //TODO Suzan: get from correct place
      typeof entityName != "string"
        ? ValueOrErrors.Default.throwOne(`entityName must be a string`)
        : dispatcherContext.referenceSources == undefined
          ? ValueOrErrors.Default.throwOne(
              `referenceSources api sources are undefined`,
            )
          : dispatcherContext
              .referenceSources(entityName)
              .Then((referenceSource) =>
                referenceSource.referenceOne == undefined
                  ? ValueOrErrors.Default.throwOne(
                      `lookup source missing "referenceOne" api`,
                    )
                  : referenceSource
                      .referenceOne(entityName)
                      .Then((source) =>
                        ValueOrErrors.Default.return(source.get)
                      ),
              ),
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: ReferenceOneRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      isInlined: boolean,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      DispatchParsedType.Operations.ResolveLookupType(
        renderer.type.detailsType.name,
        dispatcherContext.types,
      ).Then((referenceOneEntityType) =>
        referenceOneEntityType.kind != "record"
          ? ValueOrErrors.Default.throwOne(
              `expected a record type, but got a ${referenceOneEntityType.kind} type`,
            )
          : ReferenceOneDispatcher.Operations.DispatchPreviewRenderer(
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
                ReferenceOneDispatcher.Operations.GetApi(
                  renderer.entityName,
                  dispatcherContext,
                ).Then((getApi) =>
                  dispatcherContext
                    .getConcreteRenderer("referenceOne", renderer.concreteRenderer)
                    .Then((concreteRenderer) =>
                      ValueOrErrors.Default.return(
                        ReferenceOneAbstractRenderer(
                          detailsRenderer,
                          previewRenderer,
                          dispatcherContext.IdProvider,
                          dispatcherContext.ErrorRenderer,
                          renderer.detailsRenderer,
                          renderer.previewRenderer,
                          referenceOneEntityType,
                        )
                          .mapContext((_: any) => ({
                            ..._,
                            getApi,
                            fromApiParser: dispatcherContext.parseFromApiByType(
                              renderer.type.previewType,
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
