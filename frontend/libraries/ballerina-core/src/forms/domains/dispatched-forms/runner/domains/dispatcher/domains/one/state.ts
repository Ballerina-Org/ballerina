import {
  BasicFun,
  DispatcherContext,
  DispatchOneSource,
  DispatchTableApiSource,
  Guid,
  OneAbstractRenderer,
  OneType,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { OneRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/one/state";
import { NestedDispatcher } from "../nestedDispatcher/state";

export const OneDispatcher = {
  Operations: {
    DispatchPreviewRenderer: <
      T extends { [key in keyof T]: { type: any; state: any } },
    >(
      renderer: OneRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<undefined | Template<any, any, any, any>, string> =>
      renderer.previewRenderer == undefined
        ? ValueOrErrors.Default.return(undefined)
        : NestedDispatcher.Operations.DispatchAs(
            renderer.previewRenderer,
            dispatcherContext,
            "previewRenderer",
            "previewRenderer",
          ),
    GetApi: (
      api: string | string[],
      dispatcherContext: DispatcherContext<any>,
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
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: OneType<T>,
      renderer: OneRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      OneDispatcher.Operations.DispatchPreviewRenderer(
        renderer,
        dispatcherContext,
      ).Then((previewRenderer) =>
        NestedDispatcher.Operations.DispatchAs(
          renderer.detailsRenderer,
          dispatcherContext,
          "detailsRenderer",
          "detailsRenderer",
        ).Then((detailsRenderer) =>
          OneDispatcher.Operations.GetApi(renderer.api, dispatcherContext).Then(
            (getApi) =>
              renderer.renderer.kind != "lookupRenderer"
                ? ValueOrErrors.Default.throwOne<
                    Template<any, any, any, any>,
                    string
                  >(
                    `received non lookup renderer kind when resolving defaultState for one`,
                  )
                : dispatcherContext
                    .getConcreteRenderer("one", renderer.renderer.renderer)
                    .Then((concreteRenderer) =>
                      ValueOrErrors.Default.return<
                        Template<any, any, any, any>,
                        string
                      >(
                        OneAbstractRenderer(
                          detailsRenderer,
                          previewRenderer,
                          dispatcherContext.IdProvider,
                          dispatcherContext.ErrorRenderer,
                        )
                          .mapContext((_: any) => ({
                            ..._,
                            getApi,
                            fromApiParser: dispatcherContext.parseFromApiByType(
                              type.args,
                            ),
                          }))
                          .withView(concreteRenderer),
                      ),
                    ),
          ),
        ),
      ),
  },
};
