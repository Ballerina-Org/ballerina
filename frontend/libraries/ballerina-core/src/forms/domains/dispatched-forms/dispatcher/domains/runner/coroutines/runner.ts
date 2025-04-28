import { AsyncState } from "../../../../../../../async/state";
import { CoTypedFactory } from "../../../../../../../coroutines/builder";
import {
  DispatchFormRunnerContext,
  DispatchFormRunnerForeignMutationsExpected,
  DispatchFormRunnerStatus,
} from "../state";
import { id } from "../../../../../../../fun/domains/id/state";
import { DispatchFormRunnerState } from "../state";
import { Sum } from "../../../../../../../collections/domains/sum/state";
import { replaceWith } from "../../../../../../../fun/domains/updater/domains/replaceWith/state";
import { DispatcherContext } from "../../../../deserializer/state";
import { DispatchParsedLauncher } from "../../../../deserializer/state";
import { List } from "immutable";
import { FormDispatcher } from "../../../state";

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
                current.formRef.entity.kind == "r" ||
                current.formRef.config.kind == "r"
              )
                return id;

              if (current.deserializedSpecification.sync.value.kind == "errors")
                return DispatchFormRunnerState<T>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T>>({
                    kind: "error",
                    errors: current.deserializedSpecification.sync.value.errors,
                  }),
                );

              if (current.formRef.entity.value.kind == "errors") {
                console.error(
                  `Error parsing entity for form '${
                    current.formRef.formName
                  }': ${current.formRef.entity.value.errors
                    .valueSeq()
                    .toArray()
                    .join("\n")}`,
                );
                return DispatchFormRunnerState<T>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T>>({
                    kind: "error",
                    errors: current.formRef.entity.value.errors,
                  }),
                );
              }

              if (current.formRef.config.value.kind == "errors") {
                console.error(
                  `Error parsing global configuration for form '${
                    current.formRef.formName
                  }': ${current.formRef.config.value.errors
                    .valueSeq()
                    .toArray()
                    .join("\n")}`,
                );
                return DispatchFormRunnerState<T>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T>>({
                    kind: "error",
                    errors: current.formRef.config.value.errors,
                  }),
                );
              }

              const formRef = current.formRef;
              const dispatcherContext =
                current.deserializedSpecification.sync.value.value
                  .dispatcherContext;

              const passthroughFormLauncher =
                current.deserializedSpecification.sync.value.value.launchers.passthrough.get(
                  formRef.formName,
                );

              if (passthroughFormLauncher == undefined) {
                console.error(
                  `Cannot find form '${formRef.formName}' in the launchers`,
                );

                return DispatchFormRunnerState<T>().Updaters.status(
                  replaceWith<DispatchFormRunnerStatus<T>>({
                    kind: "error",
                    errors: List([
                      `Cannot find form '${formRef.formName}' in the launchers`,
                    ]),
                  }),
                );
              }

              const Form = FormDispatcher.Operations.Dispatch(
                formRef.formName,
                passthroughFormLauncher.type,
                passthroughFormLauncher.renderer,
                dispatcherContext,
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
          props.context.formRef.entity.kind != "r" &&
          props.context.formRef.config.kind != "r" &&
          (props.context.status.kind == "not initialized" ||
            props.context.status.kind == "loading")
        );
      },
    },
  );
};
