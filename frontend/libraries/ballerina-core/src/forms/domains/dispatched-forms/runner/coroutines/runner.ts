import { AsyncState } from "../../../../../async/state";
import { CoTypedFactory } from "../../../../../coroutines/builder";
import {
  DispatchFormRunnerContext,
  DispatchFormRunnerForeignMutationsExpected,
  DispatchFormRunnerStatus,
} from "../state";
import { id } from "../../../../../fun/domains/id/state";
import { DispatchFormRunnerState } from "../state";
import { replaceWith } from "../../../../../fun/domains/updater/domains/replaceWith/state";
import { List } from "immutable";
import { Dispatcher } from "../../../../../../main";

export const DispatchFormRunner = <
  T extends { [key in keyof T]: { type: any; state: any } },
>() => {
  const Co = CoTypedFactory<
    DispatchFormRunnerContext<T>,
    DispatchFormRunnerState<T>
  >();

  return Co.Template<DispatchFormRunnerForeignMutationsExpected>(
    Co.Seq([
      Co.SetState(
        DispatchFormRunnerState<T>().Updaters.status(
          replaceWith<DispatchFormRunnerStatus<T>>({ kind: "loading" }),
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
                return DispatchFormRunnerState<T>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T>>({
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
                return DispatchFormRunnerState<T>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T>>({
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
                return DispatchFormRunnerState<T>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T>>({
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

                return DispatchFormRunnerState<T>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T>>({
                    kind: "error",
                    errors: List([
                      `Cannot find form '${launcherRef.name}' in the launchers`,
                    ]),
                  }),
                );
              }

              const Form = Dispatcher.Operations.Dispatch(
                passthroughFormLauncher.type,
                passthroughFormLauncher.renderer,
                dispatcherContext,
                false,
                passthroughFormLauncher.formName,
                launcherRef.name,
                // TODO: inject api here for table launchers
              );

              if (Form.kind == "errors") {
                console.error(Form.errors.valueSeq().toArray().join("\n"));
                return DispatchFormRunnerState<T>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T>>({
                    kind: "error",
                    errors: Form.errors,
                  }),
                );
              }

              const initialState = dispatcherContext.defaultState(
                passthroughFormLauncher.type,
                passthroughFormLauncher.renderer,
              );

              if (initialState.kind == "errors") {
                console.error(
                  initialState.errors.valueSeq().toArray().join("\n"),
                );
                return DispatchFormRunnerState<T>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T>>({
                    kind: "error",
                    errors: initialState.errors,
                  }),
                );
              }
              return DispatchFormRunnerState<T>()
                .Updaters.formState(replaceWith(initialState.value))
                .then(
                  DispatchFormRunnerState<T>().Updaters.status(
                    replaceWith<DispatchFormRunnerStatus<T>>({
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
