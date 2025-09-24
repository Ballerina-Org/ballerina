import { DispatchInjectablesTypes } from "../../../abstract-renderers/injectables/state";
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
) => Co.Wait(0);
