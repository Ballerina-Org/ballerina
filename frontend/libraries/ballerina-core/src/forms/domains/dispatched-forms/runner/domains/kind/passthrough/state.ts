import { DispatchInjectablesTypes, Unit } from "../../../../../../../../main";
import {
  DispatchFormRunnerContext,
  PassthroughLauncherRef,
} from "../../../state";

export type DispatchPassthroughFormLauncherContext<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
> = Omit<
  DispatchFormRunnerContext<T, Flags, CustomPresentationContexts, ExtraContext>,
  "launcherRef"
> & {
  launcherRef: PassthroughLauncherRef<Flags>;
};

export type DispatchPassthroughFormLauncherState<T> = {};
export type DispatchPassthroughFormLauncherForeignMutationsExpected<T> = {};
