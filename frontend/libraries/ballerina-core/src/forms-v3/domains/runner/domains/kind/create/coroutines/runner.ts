import {
  DispatchInjectablesTypes,
  Debounced,
  ApiResponseChecker,
} from "../../../../../../../../../main";
import { DispatchCreateFormLauncherForeignMutationsExpected } from "../state";
import { CreateCoBuilder } from "./builder";
import { initCo } from "./_init";
import { syncCo } from "./_sync";

export const DispatchCreateFormRunner = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() => {
  const Co = CreateCoBuilder<
    T,
    Flags,
    CustomPresentationContext,
    ExtraContext
  >();

  const init = initCo<T, Flags, CustomPresentationContext, ExtraContext>(Co);
  const sync = syncCo<T, Flags, CustomPresentationContext, ExtraContext>(Co);

  return Co.Template<DispatchCreateFormLauncherForeignMutationsExpected<T>>(
    init,
    {
      runFilter: (_) =>
        !ApiResponseChecker.Operations.checked(_.context.apiChecker.init) ||
        _.context.entity.sync.kind != "loaded",
    },
  ).any([
    Co.Template<DispatchCreateFormLauncherForeignMutationsExpected<T>>(sync, {
      runFilter: (_) =>
        _.context.entity.sync.kind == "loaded" &&
        (_.context.apiRunner.sync.kind === "loading" ||
          _.context.apiRunner.sync.kind === "reloading" ||
          _.context.apiRunner.sync.kind === "loaded") &&
        !ApiResponseChecker.Operations.checked(_.context.apiChecker.create),
    }),
  ]);
};
