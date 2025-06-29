import {
  DispatchInjectablesTypes,
  DispatcherContext,
  StringSerializedType,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../../main";
import { RecordFieldRenderer } from "../../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/record/domains/recordFieldRenderer/state";
import { NestedDispatcher } from "../../nestedDispatcher/state";

export const RecordFieldDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      fieldName: string,
      renderer: RecordFieldRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      NestedDispatcher.Operations.Dispatch(
        renderer,
        dispatcherContext,
        isInlined,
        tableApi,
      ).MapErrors((errors) =>
        errors.map(
          (error) => `${error}\n...When dispatching field ${fieldName}`,
        ),
      ),
  },
};
