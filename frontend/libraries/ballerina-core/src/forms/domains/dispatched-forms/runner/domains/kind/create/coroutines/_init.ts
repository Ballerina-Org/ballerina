import { List } from "immutable";
import {
  replaceWith,
  id,
  Synchronize,
  Unit,
  HandleApiResponse,
  Synchronized,
  unit,
} from "../../../../../../../../../main";
import { ApiResponseChecker } from "../../../../../../../../api-response-handler/state";
import { AsyncState } from "../../../../../../../../async/state";
import { PredicateValue } from "../../../../../../parser/domains/predicates/state";
import { DispatcherContextWithApiSources } from "../../../../coroutines/runner";
import { DispatchFormRunnerStatus } from "../../../../state";
import { DispatchInjectablesTypes } from "../../../abstract-renderers/injectables/state";
import { Co } from "../../../abstract-renderers/table/coroutines/builder";
import { Dispatcher } from "../../../dispatcher/state";
import {
  DispatchCreateFormLauncherState,
  DispatchCreateFormLauncherContext,
} from "../state";
import { CreateCoBuilder } from "./builder";

export const initCo = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>(
  Co: ReturnType<
    typeof CreateCoBuilder<T, Flags, CustomPresentationContexts, ExtraContext>
  >,
) => {
  const setChecked = (checked: boolean) =>
    Co.SetState(
      DispatchCreateFormLauncherState<
        T,
        Flags
      >().Updaters.Core.apiChecker.children.init(
        checked
          ? ApiResponseChecker.Updaters().toChecked()
          : ApiResponseChecker.Updaters().toUnchecked(),
      ),
    );

  return Co.Seq([
    Co.SetState(
      DispatchCreateFormLauncherState<T, Flags>().Updaters.Core.status(
        replaceWith<DispatchFormRunnerStatus<T, Flags>>({ kind: "loading" }),
      ),
    ),
    setChecked(false),
    Co.GetState().then((current) => {
      if (
        !AsyncState.Operations.hasValue(current.deserializedSpecification.sync)
      ) {
        return Co.SetState(id);
      }

      if (current.deserializedSpecification.sync.value.kind == "errors") {
        const errors = current.deserializedSpecification.sync.value.errors;
        return Co.SetState(
          DispatchCreateFormLauncherState<T, Flags>().Updaters.Core.status(
            replaceWith<DispatchFormRunnerStatus<T, Flags>>({
              kind: "error",
              errors,
            }),
          ),
        );
      }

      const dispatcherContext =
        current.deserializedSpecification.sync.value.value.dispatcherContext;

      const createFormLauncher =
        current.deserializedSpecification.sync.value.value.launchers.create.get(
          current.launcherRef.name,
        );

      if (createFormLauncher == undefined) {
        console.error(
          `Cannot find form '${current.launcherRef.name}' in the create launchers`,
        );

        return Co.SetState(
          DispatchCreateFormLauncherState<T, Flags>().Updaters.Core.status(
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
        current.launcherRef.apiSources.entityApis.get(createFormLauncher.api)(
          "",
        );
      const getGlobalConfig =
        current.launcherRef.config.source == "api" &&
        current.launcherRef.config.getGlobalConfig
          ? current.launcherRef.config.getGlobalConfig
          : () =>
              current.launcherRef.apiSources.entityApis.get(
                createFormLauncher.configApi,
              )("");

      return Co.Seq([
        Co.Seq([
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
              DispatchCreateFormLauncherState<T, Flags>().Updaters.Core.entity,
            ),
            current.launcherRef.config.source == "api"
              ? Synchronize<Unit, PredicateValue>(() =>
                  getGlobalConfig().then((raw) => {
                    const result =
                      createFormLauncher.parseGlobalConfigurationFromApi(raw);
                    return result.kind == "errors"
                      ? Promise.reject(result.errors)
                      : Promise.resolve(result.value);
                  }),
                ).embed(
                  (_) => _.config,
                  DispatchCreateFormLauncherState<T, Flags>().Updaters.Core
                    .config,
                )
              : Co.SetState(
                  DispatchCreateFormLauncherState<
                    T,
                    Flags
                  >().Updaters.Core.config(
                    replaceWith(
                      current.launcherRef.config.value.kind == "l" &&
                        current.launcherRef.config.value.value.kind == "value"
                        ? Synchronized.Default(
                            unit,
                            AsyncState.Default.loaded(
                              current.launcherRef.config.value.value.value,
                            ),
                          )
                        : Synchronized.Default(unit),
                    ),
                  ),
                ),
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

            if (_.config.sync.kind == "error") {
              return DispatchCreateFormLauncherState<
                T,
                Flags
              >().Updaters.Core.status(
                replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                  kind: "error",
                  errors: _.config.sync.error,
                }),
              );
            }

            if (
              _.config.sync.kind == "loading" ||
              _.config.sync.kind == "reloading"
            ) {
              return id;
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
              .thenMany([
                DispatchCreateFormLauncherState<
                  T,
                  Flags
                >().Updaters.Core.status(
                  replaceWith<DispatchFormRunnerStatus<T, Flags>>({
                    kind: "loaded",
                    Form: Form.value,
                  }),
                ),
                DispatchCreateFormLauncherState<
                  T,
                  Flags
                >().Updaters.Core.config(replaceWith(_.config)),
              ]);
          }),
        ]),
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
        setChecked(true),
      ]);
    }),
  ]);
};
