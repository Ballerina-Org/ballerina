import {
  EnumMultiselectAbstractRenderer,
  DispatcherContext,
  InfiniteMultiselectDropdownFormAbstractRenderer,
  ValueOrErrors,
  PredicateValue,
  EnumReference,
  Guid,
  ValueRecord,
  unit,
  DispatchInjectablesTypes,
  StringSerializedType,
  MultiSelectionType,
} from "../../../../../../../../../main";
import { Template } from "../../../../../../../../template/state";
import { OrderedMap } from "immutable";
import { StreamRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/stream/state";
import { EnumRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/enum/state";

export const MultiSelectionDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: EnumRenderer<T> | StreamRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts
      >,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
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
                      ValueOrErrors.Default.return<
                        [Template<any, any, any, any>, StringSerializedType],
                        string
                      >([
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
                        MultiSelectionType.SerializeToString([
                          renderer.type.args[0] as unknown as string,
                        ]), // always a lookup type
                      ]),
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
                    ValueOrErrors.Default.return<
                      [Template<any, any, any, any>, StringSerializedType],
                      string
                    >([
                      InfiniteMultiselectDropdownFormAbstractRenderer(
                        dispatcherContext.IdProvider,
                        dispatcherContext.ErrorRenderer,
                      ).withView(concreteRenderer),
                      MultiSelectionType.SerializeToString([
                        renderer.type.args[0] as unknown as string,
                      ]), // always a lookup type]
                    ]),
                  )
                  .MapErrors((errors) =>
                    errors.map(
                      (error) =>
                        `${error}\n...When dispatching nested stream multi selection: ${renderer}`,
                    ),
                  )
              : ValueOrErrors.Default.throwOne<
                  [Template<any, any, any, any>, StringSerializedType],
                  string
                >(
                  `could not resolve multi selection concrete renderer for ${viewKind}`,
                ),
        ),
  },
};
