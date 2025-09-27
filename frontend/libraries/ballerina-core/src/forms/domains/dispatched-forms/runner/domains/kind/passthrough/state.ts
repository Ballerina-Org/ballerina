import {
  DispatchCommonFormRunnerState,
  DispatchFormRunnerContext,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  PassthroughLauncherRef,
  simpleUpdater,
  Unit,
} from "../../../../../../../../main";

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

export type DispatchPassthroughFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
> = DispatchCommonFormRunnerState<T, Flags>;

export type DispatchPassthroughFormLauncherForeignMutationsExpected<T> = {};

export const DispatchPassthroughFormLauncherState = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
>() => ({
  Default: (): DispatchPassthroughFormLauncherState<T, Flags> => ({
    ...DispatchCommonFormRunnerState<T, Flags>().Default(),
  }),
  Updaters: {
    ...simpleUpdater<DispatchPassthroughFormLauncherState<T, Flags>>()(
      "status",
    ),
    ...simpleUpdater<DispatchPassthroughFormLauncherState<T, Flags>>()(
      "formState",
    ),
  },
});
