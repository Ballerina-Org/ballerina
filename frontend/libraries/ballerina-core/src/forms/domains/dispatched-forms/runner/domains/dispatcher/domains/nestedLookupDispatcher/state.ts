import {
  DispatcherContext,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { FormDispatcher } from "../../state";
import { BaseLookupRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/nestedRenderer/domains/lookup/state";
import { FormRenderer } from "../../../../../deserializer/domains/specification/domains/forms/state";

export const NestedLookupDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      form: FormRenderer<T>,
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
