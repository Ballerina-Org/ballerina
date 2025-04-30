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
import { BaseEnumRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/domains/enum/state";
import { BaseStreamRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/domains/stream/state";
import { OrderedMap } from "immutable";

export const NestedMultiSelectionDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      viewKind: string,
      renderer: BaseEnumRenderer<T> | BaseStreamRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> => {
      const result: ValueOrErrors<
        Template<any, any, any, any>,
        string
      > = (() => {
        if (
          viewKind == "enumMultiSelection" &&
          renderer.kind == "baseEnumRenderer"
        ) {
          return dispatcherContext
            .getConcreteRenderer(
              "enumMultiSelection",
              renderer.concreteRendererName,
            )
            .Then((concreteRenderer) =>
              dispatcherContext
                .enumOptionsSources(renderer.options)
                .Then((optionsSource) =>
                  ValueOrErrors.Default.return(
                    EnumMultiselectAbstractRenderer()
                      .mapContext((_: any) => ({
                        ..._,
                        type: renderer.type,
                        label: renderer.label,
                        tooltip: renderer.tooltip,
                        details: renderer.details,
                        getOptions: (): Promise<
                          OrderedMap<Guid, ValueRecord>
                        > =>
                          optionsSource(unit).then((options) =>
                            OrderedMap(
                              options.map((o: EnumReference) => [
                                o.Value,
                                PredicateValue.Default.record(OrderedMap(o)),
                              ]),
                            ),
                          ),
                      }))
                      .withView(concreteRenderer),
                  ),
                ),
            );
        }
        if (
          viewKind == "streamMultiSelection" &&
          renderer.kind == "baseStreamRenderer"
        ) {
          return dispatcherContext
            .getConcreteRenderer(
              "streamMultiSelection",
              renderer.concreteRendererName,
            )
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                InfiniteMultiselectDropdownFormAbstractRenderer()
                  .mapContext((_: any) => ({
                    ..._,
                    type: renderer.type,
                    label: renderer.label,
                    tooltip: renderer.tooltip,
                    details: renderer.details,
                  }))
                  .withView(concreteRenderer),
              ),
            );
        }
        return ValueOrErrors.Default.throwOne(
          `could not resolve multi selection concrete renderer for ${viewKind}`,
        );
      })();

      return result.MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When dispatching nested multi selection: ${renderer}`,
        ),
      );
    },
  },
};
