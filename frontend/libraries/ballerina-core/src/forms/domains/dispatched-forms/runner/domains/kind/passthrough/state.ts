import {
  DispatchCommonFormRunnerState,
  DispatchFormRunnerContext,
  DispatchInjectablesTypes,
  PassthroughLauncherRef,
  simpleUpdater,
} from "../../../../../../../../main";

export type DispatchPassthroughFormLauncherContext<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
> = Omit<
  DispatchFormRunnerContext<T, Flags, CustomPresentationContext, ExtraContext>,
  "launcherRef"
> & {
  launcherRef: PassthroughLauncherRef<Flags>;
};

export type DispatchPassthroughFormLauncherState<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
> = DispatchCommonFormRunnerState<
  T,
  Flags,
  CustomPresentationContext,
  ExtraContext
>;

export type DispatchPassthroughFormLauncherForeignMutationsExpected<T> = {};

export const DispatchPassthroughFormLauncherState = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() => ({
  Default: (): DispatchPassthroughFormLauncherState<
    T,
    Flags,
    CustomPresentationContext,
    ExtraContext
  > => ({
    ...DispatchCommonFormRunnerState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >().Default(),
  }),
  Updaters: {
    ...simpleUpdater<
      DispatchPassthroughFormLauncherState<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >
    >()("status"),
    ...simpleUpdater<
      DispatchPassthroughFormLauncherState<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >
    >()("formState"),
  },
});
