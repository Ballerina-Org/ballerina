import {
  DispatcherContext,
  MapRepo,
  TableFormDispatcher,
  TableType,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { BaseTableRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/domains/table/state";
import { Form } from "../../../../../deserializer/domains/specification/domains/form/state";

export const NestedTableDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      form: Form<T>,
      renderer: BaseTableRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      form.kind == "tableForm"
        ? TableFormDispatcher.Operations.Dispatch(
            form.type,
            form,
            dispatcherContext,
            renderer.api,
            true,
          )
        : ValueOrErrors.Default.throwOne<Template<any, any, any, any>, string>(
            `expected table form but got ${form.kind} form`,
          ),
  },
};
