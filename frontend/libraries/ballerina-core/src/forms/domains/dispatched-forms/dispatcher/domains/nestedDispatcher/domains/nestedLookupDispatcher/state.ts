import {
  DispatcherContext,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { FormDispatcher } from "../../../../state";
import { BaseLookupRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/domains/lookup/state";
import { Form } from "../../../../../deserializer/domains/specification/domains/form/state";

export const NestedLookupDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      form: Form<T>,
      renderer: BaseLookupRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      FormDispatcher.Operations.Dispatch(
        renderer.lookupRendererName,
        form.type,
        form,
        dispatcherContext,
        true,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When dispatching nested lookup`),
      ),
  },
};
