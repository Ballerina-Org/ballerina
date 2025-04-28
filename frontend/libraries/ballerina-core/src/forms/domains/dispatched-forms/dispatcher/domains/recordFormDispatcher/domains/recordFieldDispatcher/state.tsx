import { NestedDispatcher } from "../../../nestedDispatcher/state";
import { ValueOrErrors } from "../../../../../../../../../main";

import { Template } from "../../../../../../../../../main";
import { RecordFieldRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/domains/recordFieldRenderer/state";
import { DispatcherContext } from "../../../../../../../../../main";
import { DispatchParsedType } from "../../../../../deserializer/domains/specification/domains/types/state";

export const RecordFieldDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      fieldName: string,
      type: DispatchParsedType<T>,
      fieldRenderer: RecordFieldRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.Dispatch(
        type,
        fieldRenderer,
        dispatcherContext,
      ).MapErrors((errors) =>
        errors.map(
          (error) => `${error}\n...When dispatching record field: ${fieldName}`,
        ),
      ),
  },
};
