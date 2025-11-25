import {
  DispatchInjectablesTypes,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../../main";
import { RecordFieldRenderer } from "../../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/record/domains/recordFieldRenderer/state";
import { DispatcherContextWithApiSources } from "../../../../../state";
import { NestedDispatcher } from "../../nestedDispatcher/state";

export const RecordFieldDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      fieldName: string,
      renderer: RecordFieldRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      isInlined: boolean,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.Dispatch(
        renderer,
        dispatcherContext,
        isInlined,
      ).MapErrors((errors) =>
        errors.map(
          (error) => `${error}\n...When dispatching field ${fieldName}`,
        ),
      ),
  },
};
