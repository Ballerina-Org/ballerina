import {
  ApiResponseChecker,
  DispatchInjectablesTypes,
} from "../../../../../../../../../main";
import { initCo } from "./_init";
import { syncCo } from "./_sync";
import { DispatchEditFormLauncherForeignMutationsExpected } from "../state";
import { EditCoBuilder } from "./builder";

export const DispatchEditFormRunner = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>() => {
  const Co = EditCoBuilder<
    T,
    Flags,
    CustomPresentationContexts,
    ExtraContext
  >();

  const init = initCo<T, Flags, CustomPresentationContexts, ExtraContext>(Co);
  const sync = syncCo<T, Flags, CustomPresentationContexts, ExtraContext>(Co);

  return Co.Template<DispatchEditFormLauncherForeignMutationsExpected<T>>(
    init,
    {
      runFilter: (_) =>
        !ApiResponseChecker.Operations.checked(_.context.apiChecker.init) ||
        _.context.entity.sync.kind != "loaded",
    },
  ).any([
    Co.Template<DispatchEditFormLauncherForeignMutationsExpected<T>>(sync, {
      runFilter: (_) =>
        _.context.entity.sync.kind == "loaded" &&
        (_.context.apiRunner.sync.kind !== "loaded" ||
          !ApiResponseChecker.Operations.checked(_.context.apiChecker.update)),
    }),
  ]);
};
