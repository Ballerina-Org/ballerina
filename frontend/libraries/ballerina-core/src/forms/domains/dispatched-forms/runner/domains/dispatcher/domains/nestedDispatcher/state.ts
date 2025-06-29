import {
  DispatchInjectablesTypes,
  StringSerializedType,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { NestedRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/nestedRenderer/state";
import { DispatcherContext } from "../../../../../deserializer/state";
import { Dispatcher } from "../../state";

export const NestedDispatcher = {
  Operations: {
    DispatchAs: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: NestedRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      as: string,
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
        errors.map((error) => `${error}\n...When dispatching as ${as}`),
      ),
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: NestedRenderer<T>,
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
      Dispatcher.Operations.Dispatch(
        renderer.renderer,
        dispatcherContext,
        true,
        isInlined,
        tableApi,
      )
        .Then((template) =>
          ValueOrErrors.Default.return<
            [Template<any, any, any, any>, StringSerializedType],
            string
          >([
            template[0].mapContext((_: any) => ({
              ..._,
              label: renderer.label,
              tooltip: renderer.tooltip,
              details: renderer.details,
            })),
            template[1],
          ]),
        )
        .MapErrors((errors) =>
          errors.map(
            (error) => `${error}\n...When dispatching nested renderer`,
          ),
        ),
  },
};
