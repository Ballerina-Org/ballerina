import {
  CreateLauncherRef,
  DispatchFormRunnerContext,
  DispatchFormRunnerForeignMutationsExpected,
  DispatchFormRunnerState,
  EditLauncherRef,
  PassthroughLauncherRef,
} from "./state";

import { Template, DispatchInjectablesTypes } from "../../../../../main";

import { DispatchPassthroughFormLauncherTemplate } from "./domains/kind/passthrough/template";
import { DispatchEditFormLauncherTemplate } from "./domains/kind/edit/template";
import { DispatchCreateFormLauncherTemplate } from "./domains/kind/create/template";

export const DispatchFormRunnerTemplate = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() => {
  const InstantiatedPassthroughFormLauncherTemplate =
    DispatchPassthroughFormLauncherTemplate<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >();
  const InstantiatedEditFormLauncherTemplate = DispatchEditFormLauncherTemplate<
    T,
    Flags,
    CustomPresentationContext,
    ExtraContext
  >();
  const InstantiatedCreateFormLauncherTemplate =
    DispatchCreateFormLauncherTemplate<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >();

  return Template.Default<
    DispatchFormRunnerContext<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    > &
      DispatchFormRunnerState<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
    DispatchFormRunnerState<T, Flags, CustomPresentationContext, ExtraContext>,
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
              DispatchFormRunnerState<
                T,
                Flags,
                CustomPresentationContext,
                ExtraContext
              >().Updaters.Template.passthrough(_),
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
            launcherRef: props.context.launcherRef as EditLauncherRef,
          }}
          setState={(_) =>
            props.setState(
              DispatchFormRunnerState<
                T,
                Flags,
                CustomPresentationContext,
                ExtraContext
              >().Updaters.Template.edit(_),
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
            launcherRef: props.context.launcherRef as CreateLauncherRef,
          }}
          setState={(_) =>
            props.setState(
              DispatchFormRunnerState<
                T,
                Flags,
                CustomPresentationContext,
                ExtraContext
              >().Updaters.Template.create(_),
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
  });
};
