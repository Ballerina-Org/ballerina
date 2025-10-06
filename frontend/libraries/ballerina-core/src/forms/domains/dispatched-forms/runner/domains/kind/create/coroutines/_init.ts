import { List } from "immutable";
import {
  replaceWith,
  id,
  Synchronize,
  Unit,
  HandleApiResponse,
  Synchronized,
  unit,
  DispatchParsedCreateLauncher,
} from "../../../../../../../../../main";
import { ApiResponseChecker } from "../../../../../../../../api-response-handler/state";
import { AsyncState } from "../../../../../../../../async/state";
import { PredicateValue } from "../../../../../../parser/domains/predicates/state";
import {
  DispatcherContextWithApiSources,
  DispatchFormRunnerStatus,
} from "../../../../state";
import { DispatchInjectablesTypes } from "../../../abstract-renderers/injectables/state";
import { Dispatcher } from "../../../dispatcher/state";
import {
  DispatchCreateFormLauncherState,
  DispatchCreateFormLauncherContext,
} from "../state";
import { CreateCoBuilder } from "./builder";

export const initCo = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>(
  Co: ReturnType<
    typeof CreateCoBuilder<T, Flags, CustomPresentationContext, ExtraContext>
  >,
) => {
  const setChecked = (checked: boolean) =>
    Co.SetState(
      DispatchCreateFormLauncherState<
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

  const defaultValueCo = (
    defaultApi: () => Promise<PredicateValue>,
    createFormLauncher: DispatchParsedCreateLauncher<T>,
  ) =>
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
      (
        _: DispatchCreateFormLauncherState<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >,
      ) => _.entity,
      DispatchCreateFormLauncherState<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >().Updaters.Core.entity,
    );

  const configValueCo = (
    getGlobalConfig: () => Promise<PredicateValue>,
    createFormLauncher: DispatchParsedCreateLauncher<T>,
    current: DispatchCreateFormLauncherContext<
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
            _: DispatchCreateFormLauncherState<
              T,
              Flags,
              CustomPresentationContext,
              ExtraContext
            >,
          ) => _.config,
          DispatchCreateFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >().Updaters.Core.config,
        )
      : Co.SetState(
          DispatchCreateFormLauncherState<
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
    DispatchCreateFormLauncherState<
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
      DispatchCreateFormLauncherState<
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

      const createFormLauncher =
        current.deserializedSpecification.sync.value.value.launchers.create.get(
          current.launcherRef.name,
        );

      if (createFormLauncher == undefined) {
        console.error(
          `Cannot find form '${current.launcherRef.name}' in the create launchers`,
        );

        return Co.SetState(
          errorUpd(
            List([
              `Cannot find form '${current.launcherRef.name}' in the create launchers`,
            ]),
          ),
        );
      }

      const defaultApi = () =>
        current.launcherRef.apiSources.entityApis.default(
          createFormLauncher.api,
        )("");
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
            defaultValueCo(defaultApi, createFormLauncher),
            configValueCo(getGlobalConfig, createFormLauncher, current),
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
              createFormLauncher.renderer,
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
            )(createFormLauncher.type, createFormLauncher.renderer);

            if (initialState.kind == "errors") {
              console.error(
                initialState.errors.valueSeq().toArray().join("\n"),
              );
              return errorUpd(initialState.errors);
            }

            return DispatchCreateFormLauncherState<
              T,
              Flags,
              CustomPresentationContext,
              ExtraContext
            >()
              .Updaters.Core.formState(replaceWith(initialState.value))
              .thenMany([
                DispatchCreateFormLauncherState<
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
                DispatchCreateFormLauncherState<
                  T,
                  Flags,
                  CustomPresentationContext,
                  ExtraContext
                >().Updaters.Core.config(replaceWith(_.config)),
              ]);
          }),
        ]),
        HandleApiResponse<
          DispatchCreateFormLauncherState<
            T,
            Flags,
            CustomPresentationContext,
            ExtraContext
          >,
          DispatchCreateFormLauncherContext<
            T,
            Flags,
            CustomPresentationContext,
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
