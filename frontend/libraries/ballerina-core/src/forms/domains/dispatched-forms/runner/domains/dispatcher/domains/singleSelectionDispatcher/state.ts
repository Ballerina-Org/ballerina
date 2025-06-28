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
  StringSerializedType,
  SingleSelectionType,
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
      renderer.renderer.kind != "lookupRenderer"
        ? ValueOrErrors.Default.throwOne<
            [Template<any, any, any, any>, StringSerializedType],
            string
          >(
            `received non lookup renderer kind when resolving defaultState for enum single selection`,
          )
        : renderer.renderer.renderer.kind != "concreteLookup"
          ? ValueOrErrors.Default.throwOne<
              [Template<any, any, any, any>, StringSerializedType],
              string
            >(
              `received non concrete lookup renderer kind "${renderer.renderer.renderer.kind}" when resolving defaultState for list`,
            )
          : dispatcherContext
              .getConcreteRendererKind(renderer.renderer.renderer.renderer)
              .Then((viewKind) =>
                viewKind == "enumSingleSelection" &&
                renderer.kind == "enumRenderer" &&
                renderer.renderer.kind == "lookupRenderer" &&
                renderer.renderer.renderer.kind == "concreteLookup"
                  ? dispatcherContext
                      .getConcreteRenderer(
                        "enumSingleSelection",
                        renderer.renderer.renderer.renderer,
                      )
                      .Then((concreteRenderer) =>
                        dispatcherContext
                          .enumOptionsSources(renderer.options)
                          .Then((optionsSource) =>
                            ValueOrErrors.Default.return<
                              [
                                Template<any, any, any, any>,
                                StringSerializedType,
                              ],
                              string
                            >([
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
                              SingleSelectionType.SerializeToString([
                                renderer.type.args[0] as unknown as string,
                              ]), // always a lookup type
                            ]),
                          ),
                      )
                      .MapErrors((errors) =>
                        errors.map(
                          (error) =>
                            `${error}\n...When dispatching nested enum single selection`,
                        ),
                      )
                  : viewKind == "streamSingleSelection" &&
                      renderer.kind == "streamRenderer" &&
                      renderer.renderer.kind == "lookupRenderer" &&
                      renderer.renderer.renderer.kind == "concreteLookup"
                    ? dispatcherContext
                        .getConcreteRenderer(
                          "streamSingleSelection",
                          renderer.renderer.renderer.renderer,
                        )
                        .Then((concreteRenderer) =>
                          ValueOrErrors.Default.return<
                            [
                              Template<any, any, any, any>,
                              StringSerializedType,
                            ],
                            string
                          >([
                            SearchableInfiniteStreamAbstractRenderer(
                              dispatcherContext.IdProvider,
                              dispatcherContext.ErrorRenderer,
                            ).withView(concreteRenderer),
                            SingleSelectionType.SerializeToString([
                              renderer.type.args[0] as unknown as string,
                            ]), // always a lookup type
                          ]),
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
