import {
  EnumAbstractRenderer,
  DispatcherContext,
  SearchableInfiniteStreamAbstractRenderer,
  ValueOrErrors,
  Guid,
  ValueRecord,
  unit,
  EnumReference,
  PredicateValue,
  DispatchInjectablesTypes,
  SingleSelectionType,
  LookupType,
} from "../../../../../../../../../main";
import { Template } from "../../../../../../../../template/state";
import { OrderedMap } from "immutable";
import { EnumRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/enum/state";
import { StreamRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/stream/state";

export const SingleSelectionDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: EnumRenderer<T> | StreamRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      dispatcherContext
        .getConcreteRendererKind(renderer.concreteRenderer)
        .Then((viewKind) =>
          viewKind == "enumSingleSelection" && renderer.kind == "enumRenderer"
            ? dispatcherContext
                .getConcreteRenderer(
                  "enumSingleSelection",
                  renderer.concreteRenderer,
                )
                .Then((concreteRenderer) =>
                  dispatcherContext
                    .enumOptionsSources(renderer.options)
                    .Then((optionsSource) =>
                      ValueOrErrors.Default.return<
                        Template<any, any, any, any>,
                        string
                      >(
                        EnumAbstractRenderer(
                          dispatcherContext.IdProvider,
                          dispatcherContext.ErrorRenderer,
                        )
                          .mapContext((_: any) => ({
                            ..._,
                            getOptions: (): Promise<
                              OrderedMap<Guid, ValueRecord>
                            > =>
                              optionsSource(unit).then((options) =>
                                OrderedMap(
                                  options.map((o: EnumReference) => [
                                    o.Value,
                                    PredicateValue.Default.record(
                                      OrderedMap(o),
                                    ),
                                  ]),
                                ),
                              ),
                          }))
                          .withView(concreteRenderer),
                      ),
                    ),
                )
                .MapErrors((errors) =>
                  errors.map(
                    (error) =>
                      `${error}\n...When dispatching nested enum single selection`,
                  ),
                )
            : viewKind == "streamSingleSelection" &&
                renderer.kind == "streamRenderer"
              ? dispatcherContext
                  .getConcreteRenderer(
                    "streamSingleSelection",
                    renderer.concreteRenderer,
                  )
                  .Then((concreteRenderer) =>
                    ValueOrErrors.Default.return<
                      Template<any, any, any, any>,
                      string
                    >(
                      SearchableInfiniteStreamAbstractRenderer(
                        dispatcherContext.IdProvider,
                        dispatcherContext.ErrorRenderer,
                      ).withView(concreteRenderer),
                    ),
                  )
                  .MapErrors((errors) =>
                    errors.map(
                      (error) =>
                        `${error}\n...When dispatching nested stream single selection`,
                    ),
                  )
              : ValueOrErrors.Default.throwOne(
                  `could not resolve view for ${viewKind}`,
                ),
        ),
  },
};
