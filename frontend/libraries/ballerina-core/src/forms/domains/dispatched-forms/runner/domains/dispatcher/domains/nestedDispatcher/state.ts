import { Template, ValueOrErrors } from "../../../../../../../../../main";
import { NestedRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/nestedRenderer/state";
import { DispatcherContext } from "../../../../../deserializer/state";
import { Dispatcher } from "../../state";

export const NestedDispatcher = {
  Operations: {
    DispatchAs: <T extends { [key in keyof T]: { type: any; state: any } }>(
      renderer: NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
      as: string,
      formName?: string,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      NestedDispatcher.Operations.Dispatch(
        renderer,
        dispatcherContext,
        formName,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When dispatching as ${as}`),
      ),
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      renderer: NestedRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
      formName?: string,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      Dispatcher.Operations.Dispatch(
        renderer.renderer.type,
        renderer.renderer,
        dispatcherContext,
        true,
        formName,
      )
        .Then((template) =>
          ValueOrErrors.Default.return(
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
