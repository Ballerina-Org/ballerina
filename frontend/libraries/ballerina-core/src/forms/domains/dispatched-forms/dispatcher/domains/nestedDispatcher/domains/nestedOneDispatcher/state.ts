import {
  BaseOneRenderer,
  DispatcherContext,
  DispatchOneSource,
  DispatchTableApiSource,
  OneAbstractRenderer,
  OneType,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";

export const NestedTableDispatcher = {
  Operations: {
    GetApi: (
      api: string | string[],
      dispatcherContext: DispatcherContext<any>,
    ): ValueOrErrors<
      | { kind: "table"; source: DispatchTableApiSource }
      | { kind: "one"; source: DispatchOneSource },
      string
    > =>
      typeof api == "string"
        ? dispatcherContext.tableApiSources == undefined
          ? ValueOrErrors.Default.throwOne(`table apis are undefined`)
          : dispatcherContext.tableApiSources(api).Then((source) =>
              ValueOrErrors.Default.return({
                kind: "table",
                source,
              }),
            )
        : Array.isArray(api) &&
          api.length == 2 &&
          api.every((_) => typeof _ == "string")
        ? dispatcherContext.lookupSources == undefined
          ? ValueOrErrors.Default.throwOne(`lookup apis are undefined`)
          : dispatcherContext.lookupSources(api[0]).Then((lookupSource) =>
              lookupSource.one == undefined
                ? ValueOrErrors.Default.throwOne(
                    `lookup source missing "one" api`,
                  )
                : lookupSource.one(api[1]).Then((source) =>
                    ValueOrErrors.Default.return({
                      kind: "one",
                      source: source,
                    }),
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
      dispatcherContext
        .getConcreteRenderer("one", renderer.concreteRendererName)
        .Then((concreteRenderer) =>
          NestedTableDispatcher.Operations.GetApi(
            renderer.api,
            dispatcherContext,
          ).Then((api) =>
            ValueOrErrors.Default.return<Template<any, any, any, any>, string>(
              OneAbstractRenderer.mapContext((_: any) => ({
                ..._,
                type,
                api,
              })).withView(concreteRenderer),
            ),
          ),
        ),
  },
};
