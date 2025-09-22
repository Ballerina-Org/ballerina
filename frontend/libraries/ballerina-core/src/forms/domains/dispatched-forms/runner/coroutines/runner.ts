import { AsyncState } from "../../../../../async/state";
import { CoTypedFactory } from "../../../../../coroutines/builder";
import {
  ApiSources,
  DispatchFormRunnerContext,
  DispatchFormRunnerForeignMutationsExpected,
  DispatchFormRunnerStatus,
} from "../state";
import { id } from "../../../../../fun/domains/id/state";
import { DispatchFormRunnerState } from "../state";
import { replaceWith } from "../../../../../fun/domains/updater/domains/replaceWith/state";
import { List } from "immutable";
import {
  DispatchInjectablesTypes,
  Dispatcher,
  DispatcherContext,
  DispatchParsedType,
  Renderer,
  ValueOrErrors,
} from "../../../../../../main";

export type DispatcherContextWithApiSources<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
> = Omit<
  DispatcherContext<T, Flags, CustomPresentationContexts, ExtraContext>,
  "defaultState"
> &
  ApiSources & {
    defaultState: (
      t: DispatchParsedType<T>,
      renderer: Renderer<T>,
    ) => ValueOrErrors<any, string>;
  };

export const DispatchFormRunner = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>() => {
  const Co = CoTypedFactory<
    DispatchFormRunnerContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchFormRunnerState<T, Flags>
  >();

  return Co.Template<DispatchFormRunnerForeignMutationsExpected>(
    Co.Seq([
      Co.SetState(
        DispatchFormRunnerState<T, Flags>().Updaters.status(
          replaceWith<DispatchFormRunnerStatus<T, Flags>>({ kind: "loading" }),
        ),
      ),
      Co.GetState().then((current) =>
        !AsyncState.Operations.hasValue(current.deserializedSpecification.sync)
          ? Co.Wait(0)
          : Co.UpdateState((_) => {
              if (
                !AsyncState.Operations.hasValue(
                  current.deserializedSpecification.sync,
                ) ||
                current.launcherRef.entity.kind == "r" ||
                current.launcherRef.config.kind == "r"
              )
                return id;

              if (current.deserializedSpecification.sync.value.kind == "errors")
                return DispatchFormRunnerState<T, Flags>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                    kind: "error",
                    errors: current.deserializedSpecification.sync.value.errors,
                  }),
                );

              if (current.launcherRef.entity.value.kind == "errors") {
                console.error(
                  `Error parsing entity for form '${
                    current.launcherRef.name
                  }': ${current.launcherRef.entity.value.errors
                    .valueSeq()
                    .toArray()
                    .join("\n")}`,
                );
                return DispatchFormRunnerState<T, Flags>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                    kind: "error",
                    errors: current.launcherRef.entity.value.errors,
                  }),
                );
              }

              if (current.launcherRef.config.value.kind == "errors") {
                console.error(
                  `Error parsing global configuration for form '${
                    current.launcherRef.name
                  }': ${current.launcherRef.config.value.errors
                    .valueSeq()
                    .toArray()
                    .join("\n")}`,
                );
                return DispatchFormRunnerState<T, Flags>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                    kind: "error",
                    errors: current.launcherRef.config.value.errors,
                  }),
                );
              }

              const launcherRef = current.launcherRef;
              const dispatcherContext =
                current.deserializedSpecification.sync.value.value
                  .dispatcherContext;

              const passthroughFormLauncher =
                current.deserializedSpecification.sync.value.value.launchers.passthrough.get(
                  launcherRef.name,
                );

              if (passthroughFormLauncher == undefined) {
                console.error(
                  `Cannot find form '${launcherRef.name}' in the launchers`,
                );

                return DispatchFormRunnerState<T, Flags>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                    kind: "error",
                    errors: List([
                      `Cannot find form '${launcherRef.name}' in the launchers`,
                    ]),
                  }),
                );
              }

              const dispatcherContextWithApiSources: DispatcherContextWithApiSources<
                T,
                Flags,
                CustomPresentationContexts,
                ExtraContext
              > = {
                ...dispatcherContext,
                ...launcherRef.apiSources,
                defaultState: dispatcherContext.defaultState(
                  launcherRef.apiSources.infiniteStreamSources,
                  launcherRef.apiSources.lookupSources,
                  launcherRef.apiSources.tableApiSources,
                ),
              };

              const Form = Dispatcher.Operations.Dispatch(
                passthroughFormLauncher.renderer,
                dispatcherContextWithApiSources,
                false,
                false,
                undefined,
              );

              if (Form.kind == "errors") {
                console.error(Form.errors.valueSeq().toArray().join("\n"));
                return DispatchFormRunnerState<T, Flags>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                    kind: "error",
                    errors: Form.errors,
                  }),
                );
              }

              const initialState = dispatcherContext.defaultState(
                launcherRef.apiSources.infiniteStreamSources,
                launcherRef.apiSources.lookupSources,
                launcherRef.apiSources.tableApiSources,
              )(passthroughFormLauncher.type, passthroughFormLauncher.renderer);

              if (initialState.kind == "errors") {
                console.error(
                  initialState.errors.valueSeq().toArray().join("\n"),
                );
                return DispatchFormRunnerState<T, Flags>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                    kind: "error",
                    errors: initialState.errors,
                  }),
                );
              }
              return DispatchFormRunnerState<T, Flags>()
                .Updaters.formState(replaceWith(initialState.value))
                .then(
                  DispatchFormRunnerState<T, Flags>().Updaters.status(
                    replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                      kind: "loaded",
                      Form: Form.value,
                    }),
                  ),
                );
            }),
      ),
    ]),

    {
      interval: 15,
      runFilter: (props) => {
        return (
          AsyncState.Operations.hasValue(
            props.context.deserializedSpecification.sync,
          ) &&
          props.context.launcherRef.entity.kind != "r" &&
          props.context.launcherRef.config.kind != "r" &&
          (props.context.status.kind == "not initialized" ||
            props.context.status.kind == "loading")
        );
      },
    },
  );
};
