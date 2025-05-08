import {
  BaseOneRenderer,
  BasicFun,
  DispatcherContext,
  DispatchOneSource,
  DispatchTableApiSource,
  Guid,
  NestedDispatcher,
  NestedTableDispatcher,
  OneAbstractRenderer,
  OneType,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";

export const NestedOneDispatcher = {
  Operations: {
    DispatchPreviewRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      renderer: BaseOneRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<undefined | Template<any, any, any, any>, string> =>
      renderer.previewRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedDispatcher.Operations.Dispatch(
            renderer.type.args[0],
            renderer.previewRenderer,
            dispatcherContext,
          ),
    GetApi: (
      api: string | string[],
      dispatcherContext: DispatcherContext<any>,
    ): ValueOrErrors<BasicFun<Guid, Promise<any>>, string> =>
      typeof api == "string"
        ? dispatcherContext.tableApiSources == undefined
          ? ValueOrErrors.Default.throwOne(`table apis are undefined`)
          : dispatcherContext
              .tableApiSources(api)
              .Then((source) => ValueOrErrors.Default.return(source.get))
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
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: OneType<T>,
      renderer: BaseOneRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedOneDispatcher.Operations.DispatchPreviewRenderer(
        renderer,
        dispatcherContext,
      ).Then((previewRenderer) =>
        NestedDispatcher.Operations.Dispatch(
          type,
          renderer.detailsRenderer,
          dispatcherContext,
        ).Then((detailsRenderer) =>
          NestedOneDispatcher.Operations.GetApi(
            renderer.api,
            dispatcherContext,
          ).Then((getApi) =>
            dispatcherContext
              .getConcreteRenderer("one", renderer.concreteRendererName)
              .Then((concreteRenderer) =>
                ValueOrErrors.Default.return<
                  Template<any, any, any, any>,
                  string
                >(
                  OneAbstractRenderer(detailsRenderer, previewRenderer)
                    .mapContext((_: any) => ({
                      ..._,
                      type,
                      getApi,
                      fromApiParser: dispatcherContext.parseFromApiByType(type),
                    }))
                    .withView(concreteRenderer),
                ),
              ),
          ),
        ),
      ),
  },
};
