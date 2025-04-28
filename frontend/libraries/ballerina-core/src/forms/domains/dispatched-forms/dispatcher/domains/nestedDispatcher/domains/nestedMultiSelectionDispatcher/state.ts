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
  DispatchParsedType,
} from "../../../../../../../../../main";
import { Template } from "../../../../../../../../template/state";

import { RecordFieldEnumRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/domains/recordFieldRenderer/domains/enum/state";
import { NestedEnumRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/nestedRenderer/domains/enum/state";
import { NestedStreamRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/nestedRenderer/domains/stream/state";
import { RecordFieldStreamRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/domains/recordFieldRenderer/domains/stream/state";
import { OrderedMap } from "immutable";

export const NestedMultiSelectionDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      viewKind: string,
      renderer:
        | RecordFieldEnumRenderer<T>
        | NestedEnumRenderer<T>
        | RecordFieldStreamRenderer<T>
        | NestedStreamRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> => {
      const result: ValueOrErrors<
        Template<any, any, any, any>,
        string
      > = (() => {
        if (
          viewKind == "enumMultiSelection" &&
          (renderer.kind == "recordFieldEnumRenderer" ||
            renderer.kind == "nestedEnumRenderer")
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
          (renderer.kind == "recordFieldStreamRenderer" ||
            renderer.kind == "nestedStreamRenderer")
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
          `could not resolve primitive view for ${viewKind}`,
        );
      })();

      return result.MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When dispatching nested primitive: ${renderer}`,
        ),
      );
    },
  },
};
