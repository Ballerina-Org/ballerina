import {
  DispatchInjectablesTypes,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { NestedRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/nestedRenderer/state";
import { Dispatcher } from "../../state";
import { DispatcherContextWithApiSources } from "../../../../state";

export const NestedDispatcher = {
  Operations: {
    DispatchAs: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: NestedRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      as: string,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.Dispatch(
        renderer,
        dispatcherContext,
        isInlined,
        tableApi,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When dispatching as ${as}`),
      ),
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: NestedRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      isInlined: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      Dispatcher.Operations.Dispatch(
        renderer.renderer,
        dispatcherContext,
        true,
        isInlined,
        tableApi,
      )
        .Then((template) =>
          ValueOrErrors.Default.return<Template<any, any, any, any>, string>(
            template.mapContext((_: any) => ({
              ..._,
              label: renderer.label,
              tooltip: renderer.tooltip,
              details: renderer.details,
            })),
          ),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When dispatching nested renderer`,
          ),
        ),
  },
};
