import { List } from "immutable";
import {
  DispatchInjectablesTypes,
  Unit,
  PredicateValue,
  Synchronize,
  DispatchParsedEditLauncher,
  Synchronized,
  unit,
  AsyncState,
  replaceWith,
  DispatchFormRunnerStatus,
  id,
  Dispatcher,
  HandleApiResponse,
  DispatcherContextWithApiSources,
} from "../../../../../../../../../main";
import { ApiResponseChecker } from "../../../../../../../../api-response-handler/state";
import {
  DispatchEditFormLauncherContext,
  DispatchEditFormLauncherState,
} from "../state";
import { EditCoBuilder } from "./builder";

export const initCo = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>(
  Co: ReturnType<
    typeof EditCoBuilder<T, Flags, CustomPresentationContext, ExtraContext>
  >,
) => {
  const setChecked = (checked: boolean) =>
    Co.SetState(
      DispatchEditFormLauncherState<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >().Updaters.Core.apiChecker.children.init(
        checked
          ? ApiResponseChecker.Updaters().toChecked()
          : ApiResponseChecker.Updaters().toUnchecked(),
      ),
    );

  const getValueCo = (
    getValueApi: () => Promise<PredicateValue>,
    editFormLauncher: DispatchParsedEditLauncher<T>,
  ) =>
    Synchronize<Unit, PredicateValue>(
      () =>
        getValueApi().then((raw) => {
          const result = editFormLauncher.fromApiParser(raw);
          return result.kind == "errors"
            ? Promise.reject(result.errors)
            : Promise.resolve(result.value);
        }),
      (_) => "transient failure",
      5,
      50,
    ).embed(
      (
        _: DispatchEditFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >,
      ) => _.entity,
      DispatchEditFormLauncherState<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >().Updaters.Core.entity,
    );

  const configValueCo = (
    getGlobalConfig: () => Promise<PredicateValue>,
    createFormLauncher: DispatchParsedEditLauncher<T>,
    current: DispatchEditFormLauncherContext<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
  ) =>
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
          (
            _: DispatchEditFormLauncherState<
              T,
              Flags,
              CustomPresentationContext,
              ExtraContext
            >,
          ) => _.config,
          DispatchEditFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >().Updaters.Core.config,
        )
      : Co.SetState(
          DispatchEditFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
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
        );

  const errorUpd = (errors: List<string>) =>
    DispatchEditFormLauncherState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >().Updaters.Core.status(
      replaceWith<
        DispatchFormRunnerStatus<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >
      >({
        kind: "error",
        errors,
      }),
    );

  return Co.Seq([
    Co.SetState(
      DispatchEditFormLauncherState<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >().Updaters.Core.status(
        replaceWith<
          DispatchFormRunnerStatus<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >
        >({ kind: "loading" }),
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
        return Co.SetState(
          errorUpd(current.deserializedSpecification.sync.value.errors),
        );
      }

      const dispatcherContext =
        current.deserializedSpecification.sync.value.value.dispatcherContext;

      const editFormLauncher =
        current.deserializedSpecification.sync.value.value.launchers.edit.get(
          current.launcherRef.name,
        );

      if (editFormLauncher == undefined) {
        console.error(
          `Cannot find form '${current.launcherRef.name}' in the edit launchers`,
        );

        return Co.SetState(
          errorUpd(
            List([
              `Cannot find form '${current.launcherRef.name}' in the edit launchers`,
            ]),
          ),
        );
      }

      const getApi = () =>
        current.launcherRef.apiSources.entityApis.get(editFormLauncher.api)(
          current.launcherRef.entityId,
        );
      const getGlobalConfig =
        current.launcherRef.config.source == "api" &&
        current.launcherRef.config.getGlobalConfig
          ? current.launcherRef.config.getGlobalConfig
          : () =>
              current.launcherRef.apiSources.entityApis.get(
                editFormLauncher.configApi,
              )("");

      return Co.Seq([
        Co.Seq([
          Co.All([
            getValueCo(getApi, editFormLauncher),
            configValueCo(getGlobalConfig, editFormLauncher, current),
          ]),
          Co.UpdateState((_) => {
            if (_.entity.sync.kind == "error") {
              return errorUpd(List([_.entity.sync.error]));
            }

            if (_.config.sync.kind == "error") {
              return errorUpd(List([_.config.sync.error]));
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
              CustomPresentationContext,
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
              editFormLauncher.renderer,
              dispatcherContextWithApiSources,
              false,
              false,
              undefined,
            );

            if (Form.kind == "errors") {
              console.error(Form.errors.valueSeq().toArray().join("\n"));
              return errorUpd(Form.errors);
            }

            const initialState = dispatcherContext.defaultState(
              current.launcherRef.apiSources.infiniteStreamSources,
              current.launcherRef.apiSources.lookupSources,
              current.launcherRef.apiSources.tableApiSources,
            )(editFormLauncher.type, editFormLauncher.renderer);

            if (initialState.kind == "errors") {
              console.error(
                initialState.errors.valueSeq().toArray().join("\n"),
              );
              return errorUpd(initialState.errors);
            }

            return DispatchEditFormLauncherState<
              T,
              Flags,
              CustomPresentationContext,
              ExtraContext
            >()
              .Updaters.Core.formState(replaceWith(initialState.value))
              .thenMany([
                DispatchEditFormLauncherState<
                  T,
                  Flags,
                  CustomPresentationContext,
                  ExtraContext
                >().Updaters.Core.status(
                  replaceWith<
                    DispatchFormRunnerStatus<
                      T,
                      Flags,
                      CustomPresentationContext,
                      ExtraContext
                    >
                  >({
                    kind: "loaded",
                    Form: Form.value,
                  }),
                ),
                DispatchEditFormLauncherState<
                  T,
                  Flags,
                  CustomPresentationContext,
                  ExtraContext
                >().Updaters.Core.formName(
                  replaceWith(editFormLauncher.formName),
                ),
                DispatchEditFormLauncherState<
                  T,
                  Flags,
                  CustomPresentationContext,
                  ExtraContext
                >().Updaters.Core.config(replaceWith(_.config)),
              ]);
          }),
        ]),
        HandleApiResponse<
          DispatchEditFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >,
          DispatchEditFormLauncherContext<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >,
          any
        >((_) => _.entity.sync, {
          handleSuccess: current.launcherRef.apiHandlers?.onGetSuccess,
          handleError: current.launcherRef.apiHandlers?.onGetError,
        }),
        setChecked(true),
      ]);
    }),
  ]);
};
