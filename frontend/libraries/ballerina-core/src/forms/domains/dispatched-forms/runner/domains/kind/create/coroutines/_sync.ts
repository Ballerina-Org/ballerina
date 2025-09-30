import {
  ApiErrors,
  ApiResponseChecker,
  AsyncState,
  HandleApiResponse,
  id,
  Synchronize,
  Synchronized,
  Unit,
} from "../../../../../../../../../main";
import { DispatchInjectablesTypes } from "../../../abstract-renderers/injectables/state";
import {
  DispatchCreateFormLauncherContext,
  DispatchCreateFormLauncherState,
} from "../state";
import { CreateCoBuilder } from "./builder";

export const syncCo = <
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
      >().Updaters.Core.apiChecker.children.create(
        checked
          ? ApiResponseChecker.Updaters().toChecked()
          : ApiResponseChecker.Updaters().toUnchecked(),
      ),
    );

  return Co.GetState().then((current) => {
    if (current.deserializedSpecification.sync.kind !== "loaded") {
      return Co.SetState(id);
    }

    if (current.deserializedSpecification.sync.value.kind == "errors") {
      return Co.SetState(id);
    }

    const createFormLauncher =
      current.deserializedSpecification.sync.value.value.launchers.create.get(
        current.launcherRef.name,
      );

    if (!createFormLauncher) {
      return Co.SetState(id);
    }

    const create = current.launcherRef.apiSources.entityApis.create(
      createFormLauncher.api,
    );

    return Co.Seq([
      setChecked(false),
      Synchronize<Unit, ApiErrors, Unit>(
        (_) => {
          if (current.entity.sync.kind !== "loaded") {
            return Promise.resolve([]);
          }

          const parsed = createFormLauncher?.toApiParser(
            current.entity.sync.value,
            createFormLauncher?.type,
            current.formState,
          );

          return !parsed || parsed?.kind == "errors"
            ? Promise.reject(parsed?.errors)
            : create(parsed.value).then(() => []);
        },
        (_) => "transient failure",
        3,
        50,
      ).embed(
        (_) => _.apiRunner,
        DispatchCreateFormLauncherState<T, Flags>().Updaters.Core.apiRunner,
      ),
      HandleApiResponse<
        DispatchCreateFormLauncherState<T, Flags>,
        DispatchCreateFormLauncherContext<
          T,
          Flags,
          CustomPresentationContexts,
          ExtraContext
        >,
        ApiErrors
      >((_) => _.apiRunner.sync, {
        handleSuccess: current.launcherRef.apiHandlers?.onCreateSuccess,
        handleError: current.launcherRef.apiHandlers?.onCreateError,
      }),
      setChecked(true),
    ]);
  });
};
