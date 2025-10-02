import {
  ApiErrors,
  DispatchInjectablesTypes,
  HandleApiResponse,
  id,
  Synchronize,
  Unit,
} from "../../../../../../../../../main";
import { ApiResponseChecker } from "../../../../../../../../api-response-handler/state";
import {
  DispatchEditFormLauncherContext,
  DispatchEditFormLauncherState,
} from "../state";
import { EditCoBuilder } from "./builder";

export const syncCo = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>(
  Co: ReturnType<
    typeof EditCoBuilder<T, Flags, CustomPresentationContexts, ExtraContext>
  >,
) => {
  const setChecked = (checked: boolean) =>
    Co.SetState(
      DispatchEditFormLauncherState<
        T,
        Flags
      >().Updaters.Core.apiChecker.children.update(
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

    const editFormLauncher =
      current.deserializedSpecification.sync.value.value.launchers.edit.get(
        current.launcherRef.name,
      );

    if (!editFormLauncher) {
      return Co.SetState(id);
    }

    const update = current.launcherRef.apiSources.entityApis.update(
      editFormLauncher.api,
    );

    return Co.Seq([
      setChecked(false),
      Synchronize<Unit, ApiErrors, Unit>(
        (_) => {
          if (current.entity.sync.kind !== "loaded") {
            return Promise.resolve([]);
          }

          const parsed = editFormLauncher?.toApiParser(
            current.entity.sync.value,
            editFormLauncher?.type,
            current.formState,
          );

          return !parsed || parsed?.kind == "errors"
            ? Promise.reject(parsed?.errors)
            : update(current.launcherRef.entityId, parsed.value).then(() => []);
        },
        (_) => "transient failure",
        3,
        50,
      ).embed(
        (_) => _.apiRunner,
        DispatchEditFormLauncherState<T, Flags>().Updaters.Core.apiRunner,
      ),
      HandleApiResponse<
        DispatchEditFormLauncherState<T, Flags>,
        DispatchEditFormLauncherContext<
          T,
          Flags,
          CustomPresentationContexts,
          ExtraContext
        >,
        ApiErrors
      >((_) => _.apiRunner.sync, {
        handleSuccess: current.launcherRef.apiHandlers?.onUpdateSuccess,
        handleError: current.launcherRef.apiHandlers?.onUpdateError,
      }),
      setChecked(true),
    ]);
  });
};
