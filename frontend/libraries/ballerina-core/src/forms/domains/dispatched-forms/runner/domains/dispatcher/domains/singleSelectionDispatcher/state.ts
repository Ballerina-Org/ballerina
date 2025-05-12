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
} from "../../../../../../../../../main";
import { Template } from "../../../../../../../../template/state";
import { OrderedMap } from "immutable";
import { EnumRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/enum/state";
import { StreamRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/stream/state";

export const SingleSelectionDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      renderer: EnumRenderer<T> | StreamRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      renderer.renderer.kind != "lookupRenderer"
        ? ValueOrErrors.Default.throwOne<Template<any, any, any, any>, string>(
            `received non lookup renderer kind when resolving defaultState for enum single selection`,
          )
        : dispatcherContext
            .getConcreteRendererKind(renderer.renderer.renderer)
            .Then((viewKind) =>
              viewKind == "enumSingleSelection" &&
              renderer.kind == "enumRenderer" &&
              renderer.renderer.kind == "lookupRenderer"
                ? dispatcherContext
                    .getConcreteRenderer(
                      "enumSingleSelection",
                      renderer.renderer.renderer,
                    )
                    .Then((concreteRenderer) =>
                      dispatcherContext
                        .enumOptionsSources(renderer.options)
                        .Then((optionsSource) =>
                          ValueOrErrors.Default.return(
                            EnumAbstractRenderer(
                              dispatcherContext.IdWrapper,
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
                    renderer.kind == "streamRenderer" &&
                    renderer.renderer.kind == "lookupRenderer"
                  ? dispatcherContext
                      .getConcreteRenderer(
                        "streamSingleSelection",
                        renderer.renderer.renderer,
                      )
                      .Then((concreteRenderer) =>
                        ValueOrErrors.Default.return(
                          SearchableInfiniteStreamAbstractRenderer(
                            dispatcherContext.IdWrapper,
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
