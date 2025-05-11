import {
  DispatcherContext,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../../main";
import { RecordFieldRenderer } from "../../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/record/domains/recordFieldRenderer/state";
import { NestedDispatcher } from "../../nestedDispatcher/state";

export const RecordFieldDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      fieldName: string,
      renderer: RecordFieldRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> => {
      return NestedDispatcher.Operations.Dispatch(
        renderer,
        dispatcherContext,
        fieldName,
      ).MapErrors((errors) =>
        errors.map(
          (error) => `${error}\n...When dispatching field ${fieldName}`,
        ),
      );
    },
  },
};
