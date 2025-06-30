import {
  BasicFun,
  DispatcherContext,
  Guid,
  OneAbstractRenderer,
  OneType,
  DispatchInjectablesTypes,
  Template,
  ValueOrErrors,
  StringSerializedType,
  LookupType,
  DispatchParsedType,
  LookupTypeAbstractRenderer,
} from "../../../../../../../../../main";
import { OneRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/one/state";
import { NestedDispatcher } from "../nestedDispatcher/state";

export const OneDispatcher = {
  Operations: {
    DispatchPreviewRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: OneRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<
      undefined | [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
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
      api: string | string[],
      dispatcherContext: DispatcherContext<
        any,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
    ): ValueOrErrors<BasicFun<Guid, Promise<any>>, string> =>
      typeof api == "string"
        ? ValueOrErrors.Default.throwOne(
            `Attempted to use a table API for a one, this is not allowed`,
          )
        : Array.isArray(api) &&
            api.length == 2 &&
            api.every((_) => typeof _ == "string")
          ? dispatcherContext.lookupSources == undefined
            ? ValueOrErrors.Default.throwOne(`lookup apis are undefined`)
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
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
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
                    .Then((concreteRenderer) => {
                      const serializedType = OneType.SerializeToString(
                        renderer.type.arg.name,
                      );
                      return ValueOrErrors.Default.return<
                        [Template<any, any, any, any>, StringSerializedType],
                        string
                      >([
                        OneAbstractRenderer(
                          LookupTypeAbstractRenderer<
                            CustomPresentationContexts,
                            Flags,
                            ExtraContext
                          >(
                            detailsRenderer[0],
                            dispatcherContext.IdProvider,
                            dispatcherContext.ErrorRenderer,
                            LookupType.SerializeToString(
                              renderer.type.arg.name,
                            ),
                          ).withView(dispatcherContext.lookupTypeRenderer()),
                          previewRenderer
                            ? LookupTypeAbstractRenderer<
                                CustomPresentationContexts,
                                Flags,
                                ExtraContext
                              >(
                                previewRenderer[0],
                                dispatcherContext.IdProvider,
                                dispatcherContext.ErrorRenderer,
                                LookupType.SerializeToString(
                                  renderer.type.arg.name,
                                ),
                              ).withView(dispatcherContext.lookupTypeRenderer())
                            : undefined,
                          dispatcherContext.IdProvider,
                          dispatcherContext.ErrorRenderer,
                          serializedType,
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
                        serializedType,
                      ]);
                    }),
                ),
              ),
            ),
      ),
  },
};
