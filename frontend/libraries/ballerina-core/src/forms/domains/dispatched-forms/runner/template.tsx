import { Map } from "immutable";

import {
  CreateLauncherRef,
  DispatchCommonFormRunnerState,
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
import { DispatchCreateFormLauncherState } from "./domains/kind/create/state";
import { DispatchEditFormLauncherState } from "./domains/kind/edit/state";
import { DispatchPassthroughFormLauncherState } from "./domains/kind/passthrough/state";

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
    if (
      props.context.launcherRef.kind === "passthrough" &&
      props.context.innerFormState.kind === "passthrough"
    ) {
      return (
        <InstantiatedPassthroughFormLauncherTemplate
          {...props}
          context={{
            ...props.context,
            ...props.context.innerFormState.state,
            launcherRef: props.context
              .launcherRef as PassthroughLauncherRef<Flags>,
          }}
          setState={(_) =>
            props.setState(
              DispatchFormRunnerState<T, Flags>().Updaters.Template.passthrough(
                _,
              ),
            )
          }
        />
      );
    }

    if (
      props.context.launcherRef.kind === "edit" &&
      props.context.innerFormState.kind === "edit"
    ) {
      return (
        <InstantiatedEditFormLauncherTemplate
          {...props}
          context={{
            ...props.context,
            ...props.context.innerFormState.state,
            launcherRef: props.context.launcherRef as EditLauncherRef<Flags>,
          }}
          setState={(_) =>
            props.setState(
              DispatchFormRunnerState<T, Flags>().Updaters.Template.edit(_),
            )
          }
        />
      );
    }

    if (
      props.context.launcherRef.kind === "create" &&
      props.context.innerFormState.kind === "create"
    ) {
      return (
        <InstantiatedCreateFormLauncherTemplate
          {...props}
          context={{
            ...props.context,
            ...props.context.innerFormState.state,
            launcherRef: props.context.launcherRef as CreateLauncherRef<Flags>,
          }}
          setState={(_) =>
            props.setState(
              DispatchFormRunnerState<T, Flags>().Updaters.Template.create(_),
            )
          }
        />
      );
    }

    console.error("Unknown launcher / form state kinds", {
      launcherRefKind: props.context.launcherRef.kind,
      innerFormStateKind: props.context.innerFormState.kind,
    });
    return (
      props.context.errorComponent ?? <>Error: Check console for details</>
    );
  }).any([DispatchFormRunner()]);
};
