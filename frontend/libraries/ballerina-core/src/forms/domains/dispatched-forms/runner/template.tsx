import { Map } from "immutable";

import {
  CreateLauncherRef,
  DispatchFormRunnerContext,
  DispatchFormRunnerForeignMutationsExpected,
  DispatchFormRunnerState,
  EditLauncherRef,
  LauncherRef,
  PassthroughLauncherRef,
} from "./state";

import { DispatchFormRunner } from "./coroutines/runner";
import {
  Template,
  unit,
  BasicUpdater,
  DispatchInjectablesTypes,
  Unit,
} from "../../../../../main";

import { DispatchPassthroughFormLauncherTemplate } from "./domains/kind/passthrough/template";
import { DispatchEditFormLauncherTemplate } from "./domains/kind/edit/template";
import { DispatchCreateFormLauncherTemplate } from "./domains/kind/create/template";

export const DispatchFormRunnerTemplate = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>() => {
  const InstantiatedPassthroughFormLauncherTemplate =
    DispatchPassthroughFormLauncherTemplate<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >();
  const InstantiatedEditFormLauncherTemplate = DispatchEditFormLauncherTemplate<
    T,
    Flags,
    CustomPresentationContexts,
    ExtraContext
  >();
  const InstantiatedCreateFormLauncherTemplate =
    DispatchCreateFormLauncherTemplate<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >();

  return Template.Default<
    DispatchFormRunnerContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    > &
      DispatchFormRunnerState<T, Flags>,
    DispatchFormRunnerState<T, Flags>,
    DispatchFormRunnerForeignMutationsExpected
  >((props) => {
    if (props.context.launcherRef.kind === "passthrough") {
      return (
        <InstantiatedPassthroughFormLauncherTemplate
          {...props}
          context={{
            ...props.context,
            launcherRef: props.context
              .launcherRef as PassthroughLauncherRef<Flags>,
          }}
        />
      );
    }

    if (props.context.launcherRef.kind === "edit") {
      return (
        <InstantiatedEditFormLauncherTemplate
          {...props}
          context={{
            ...props.context,
            launcherRef: props.context.launcherRef as EditLauncherRef<Flags>,
          }}
        />
      );
    }

    if (props.context.launcherRef.kind === "create") {
      return (
        <InstantiatedCreateFormLauncherTemplate
          {...props}
          context={{
            ...props.context,
            api: undefined,
            launcherRef: props.context.launcherRef as CreateLauncherRef<Flags>,
          }}
        />
      );
    }

    console.error("Unknown launcher kind", props.context.launcherRef);
    return (
      props.context.errorComponent ?? <>Error: Check console for details</>
    );
  }).any([DispatchFormRunner()]);
};
