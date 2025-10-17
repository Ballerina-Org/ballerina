import {
  EnumMultiselectAbstractRenderer,
  InfiniteMultiselectDropdownFormAbstractRenderer,
  ValueOrErrors,
  PredicateValue,
  EnumReference,
  Guid,
  ValueRecord,
  unit,
  DispatchInjectablesTypes,
} from "../../../../../../../../../main";
import { Template } from "../../../../../../../../template/state";
import { OrderedMap } from "immutable";
import { StreamRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/stream/state";
import { EnumRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/enum/state";
import { DispatcherContextWithApiSources } from "../../../../state";

export const MultiSelectionDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: EnumRenderer<T> | StreamRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      dispatcherContext
        .getConcreteRendererKind(renderer.concreteRenderer)
        .Then((viewKind) =>
          viewKind == "enumMultiSelection" && renderer.kind == "enumRenderer"
            ? dispatcherContext
                .getConcreteRenderer(
                  "enumMultiSelection",
                  renderer.concreteRenderer,
                )
                .Then((concreteRenderer) =>
                  dispatcherContext
                    .enumOptionsSources(renderer.options)
                    .Then((optionsSource) =>
                      ValueOrErrors.Default.return(
                        EnumMultiselectAbstractRenderer(
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
                      `${error}\n...When dispatching nested enum multi selection: ${renderer}`,
                  ),
                )
            : viewKind == "streamMultiSelection" &&
                renderer.kind == "streamRenderer"
              ? dispatcherContext
                  .getConcreteRenderer(
                    "streamMultiSelection",
                    renderer.concreteRenderer,
                  )
                  .Then((concreteRenderer) =>
                    ValueOrErrors.Default.return(
                      InfiniteMultiselectDropdownFormAbstractRenderer(
                        dispatcherContext.IdProvider,
                        dispatcherContext.ErrorRenderer,
                      )
                        .mapContext((_: any) => ({
                          ..._,
                          type: renderer.type,
                        }))
                        .withView(concreteRenderer),
                    ),
                  )
                  .MapErrors((errors) =>
                    errors.map(
                      (error) =>
                        `${error}\n...When dispatching nested stream multi selection: ${renderer}`,
                    ),
                  )
              : ValueOrErrors.Default.throwOne(
                  `could not resolve multi selection concrete renderer for ${viewKind}`,
                ),
        ),
  },
};
