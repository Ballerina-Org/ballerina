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
} from "../../../../../../../../../main";
import { Template } from "../../../../../../../../template/state";
import { OrderedMap } from "immutable";
import { StreamRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/stream/state";
import { EnumRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/enum/state";

export const MultiSelectionDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      renderer: EnumRenderer<T> | StreamRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.renderer.kind != "lookupRenderer"
        ? ValueOrErrors.Default.throwOne<Template<any, any, any, any>, string>(
            `received non lookup renderer kind when resolving defaultState for enum multi selection`,
          )
        : dispatcherContext
            .getConcreteRendererKind(renderer.renderer.renderer)
            .Then((viewKind) =>
              viewKind == "enumMultiSelection" &&
              renderer.kind == "enumRenderer" &&
              renderer.renderer.kind == "lookupRenderer"
                ? dispatcherContext
                    .getConcreteRenderer(
                      "enumMultiSelection",
                      renderer.renderer.renderer,
                    )
                    .Then((concreteRenderer) =>
                      dispatcherContext
                        .enumOptionsSources(renderer.options)
                        .Then((optionsSource) =>
                          ValueOrErrors.Default.return(
                            EnumMultiselectAbstractRenderer()
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
                  ? renderer.renderer.kind != "lookupRenderer"
                    ? ValueOrErrors.Default.throwOne<
                        Template<any, any, any, any>,
                        string
                      >(
                        `received non lookup renderer kind "${renderer.renderer.kind}" when resolving defaultState for stream multi selection`,
                      )
                    : dispatcherContext
                        .getConcreteRenderer(
                          "streamMultiSelection",
                          renderer.renderer.renderer,
                        )
                        .Then((concreteRenderer) =>
                          ValueOrErrors.Default.return(
                            InfiniteMultiselectDropdownFormAbstractRenderer().withView(
                              concreteRenderer,
                            ),
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
