import { List } from "immutable";
import {
  AsyncState,
  ApiResponseChecker,
  CoTypedFactory,
  Dispatcher,
  DispatchFormRunnerState,
  DispatchFormRunnerStatus,
  DispatchInjectablesTypes,
  HandleApiResponse,
  id,
  PredicateValue,
  replaceWith,
  Synchronize,
  unit,
  Unit,
} from "../../../../../../../../../main";
import {
  DispatchCreateFormLauncherContext,
  DispatchCreateFormLauncherForeignMutationsExpected,
  DispatchCreateFormLauncherState,
} from "../state";
import { DispatcherContextWithApiSources } from "../../../../coroutines/runner";

export const DispatchCreateFormRunner = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>() => {
  const Co = CoTypedFactory<
    DispatchCreateFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchCreateFormLauncherState<T, Flags>
  >();

  const init = Co.Seq([
    Co.SetState(
      DispatchCreateFormLauncherState<T, Flags>().Updaters.Core.status(
        replaceWith<DispatchFormRunnerStatus<T, Flags>>({ kind: "loading" }),
      ),
    ),
    Co.SetState(
      DispatchCreateFormLauncherState<T, Flags>().Updaters.Core.initApiChecker(
        ApiResponseChecker.Updaters().toUnchecked(),
      ),
    ),
    Co.GetState().then((current) =>
      Co.Seq([
        !AsyncState.Operations.hasValue(current.deserializedSpecification.sync)
          ? Co.Wait(0)
          : Co.Return((_: any) => {
              if (
                !AsyncState.Operations.hasValue(
                  current.deserializedSpecification.sync,
                )
              ) {
                return id;
              }

              if (
                current.deserializedSpecification.sync.value.kind == "errors"
              ) {
                const errors =
                  current.deserializedSpecification.sync.value.errors;
                return Co.UpdateState((_) =>
                  DispatchCreateFormLauncherState<
                    T,
                    Flags
                  >().Updaters.Core.status(
                    replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                      kind: "error",
                      errors,
                    }),
                  ),
                );
              }

              const dispatcherContext =
                current.deserializedSpecification.sync.value.value
                  .dispatcherContext;

              const createFormLauncher =
                current.deserializedSpecification.sync.value.value.launchers.create.get(
                  current.launcherRef.name,
                );

              if (createFormLauncher == undefined) {
                console.error(
                  `Cannot find form '${current.launcherRef.name}' in the create launchers`,
                );

                return Co.UpdateState((_) =>
                  DispatchCreateFormLauncherState<
                    T,
                    Flags
                  >().Updaters.Core.status(
                    replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                      kind: "error",
                      errors: List([
                        `Cannot find form '${current.launcherRef.name}' in the create launchers`,
                      ]),
                    }),
                  ),
                );
              }

              const defaultApi = () =>
                current.launcherRef.apiSources.entityApis.get(
                  createFormLauncher.api,
                )("");

              return Co.Seq([
                Co.All([
                  Synchronize<Unit, PredicateValue>(
                    () =>
                      defaultApi().then((raw) => {
                        const result = createFormLauncher.fromApiParser(raw);
                        return result.kind == "errors"
                          ? Promise.reject(result.errors)
                          : Promise.resolve(result.value);
                      }),
                    (_) => "transient failure",
                    5,
                    50,
                  ).embed(
                    (_) => _.entity,
                    DispatchCreateFormLauncherState<T, Flags>().Updaters.Core
                      .entity,
                  ),
                  // TODO: add global configuration api call?
                ]),
                Co.UpdateState((_) => {
                  if (_.entity.sync.kind == "error") {
                    return DispatchCreateFormLauncherState<
                      T,
                      Flags
                    >().Updaters.Core.status(
                      replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                        kind: "error",
                        errors: _.entity.sync.error,
                      }),
                    );
                  }

                  if (
                    _.entity.sync.kind == "loading" ||
                    _.entity.sync.kind == "unloaded" ||
                    _.entity.sync.kind == "reloading"
                  ) {
                    return id;
                  }

                  const dispatcherContextWithApiSources: DispatcherContextWithApiSources<
                    T,
                    Flags,
                    CustomPresentationContexts,
                    ExtraContext
                  > = {
                    ...dispatcherContext,
                    ...current.launcherRef.apiSources,
                    defaultState: dispatcherContext.defaultState(
                      current.launcherRef.apiSources.infiniteStreamSources,
                      current.launcherRef.apiSources.lookupSources,
                      current.launcherRef.apiSources.tableApiSources,
                    ),
                  };

                  const Form = Dispatcher.Operations.Dispatch(
                    createFormLauncher.renderer,
                    dispatcherContextWithApiSources,
                    false,
                    false,
                    undefined,
                  );

                  if (Form.kind == "errors") {
                    console.error(Form.errors.valueSeq().toArray().join("\n"));
                    return DispatchCreateFormLauncherState<
                      T,
                      Flags
                    >().Updaters.Core.status(
                      replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                        kind: "error",
                        errors: Form.errors,
                      }),
                    );
                  }

                  const initialState = dispatcherContext.defaultState(
                    current.launcherRef.apiSources.infiniteStreamSources,
                    current.launcherRef.apiSources.lookupSources,
                    current.launcherRef.apiSources.tableApiSources,
                  )(createFormLauncher.type, createFormLauncher.renderer);

                  if (initialState.kind == "errors") {
                    console.error(
                      initialState.errors.valueSeq().toArray().join("\n"),
                    );
                    return DispatchCreateFormLauncherState<
                      T,
                      Flags
                    >().Updaters.Core.status(
                      replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                        kind: "error",
                        errors: initialState.errors,
                      }),
                    );
                  }

                  return DispatchCreateFormLauncherState<T, Flags>()
                    .Updaters.Core.formState(replaceWith(initialState.value))
                    .then(
                      DispatchCreateFormLauncherState<
                        T,
                        Flags
                      >().Updaters.Core.status(
                        replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                          kind: "loaded",
                          Form: Form.value,
                        }),
                      ),
                    );
                }),
              ]);
            }),
        HandleApiResponse<
          DispatchCreateFormLauncherState<T, Flags>,
          DispatchCreateFormLauncherContext<
            T,
            Flags,
            CustomPresentationContexts,
            ExtraContext
          >,
          any
        >((_) => _.entity.sync, {
          handleSuccess: current.launcherRef.apiHandlers?.onDefaultSuccess,
          handleError: current.launcherRef.apiHandlers?.onDefaultError,
        }),
        Co.SetState(
          DispatchCreateFormLauncherState<
            T,
            Flags
          >().Updaters.Core.initApiChecker(
            ApiResponseChecker.Updaters().toChecked(),
          ),
        ),
      ]),
    ),
  ]);

  return Co.Template<DispatchCreateFormLauncherForeignMutationsExpected<T>>(
    Co.GetState().then((current) =>
      Co.Do(() => {
        console.log("DispatchCreateFormRunner", current);
      }),
    ),
    {
      runFilter: (_) =>
        !_.context.initApiChecker.apiResponseChecked ||
        _.context.entity.sync.kind != "loaded",
    },
  );
};
