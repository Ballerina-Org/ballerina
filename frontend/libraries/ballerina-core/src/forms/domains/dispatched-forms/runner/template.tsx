import { Map } from "immutable";

import {
  DispatchFormRunnerContext,
  DispatchFormRunnerForeignMutationsExpected,
  DispatchFormRunnerState,
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
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
>() =>
  Template.Default<
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
    if (props.context.status.kind !== "loaded") {
      if (
        props.context.status.kind === "loading" ||
        props.context.status.kind == "not initialized"
      ) {
        return props.context.loadingComponent ?? <>Loading...</>;
      }

      return (
        props.context.errorComponent ?? <>Error: Check console for details</>
      );
    }

    const InstantiatedTemplate =
      props.context.launcherRef.kind == "passthrough"
        ? DispatchPassthroughFormLauncherTemplate<
            T,
            Flags,
            CustomPresentationContexts,
            ExtraContext
          >()
        : props.context.launcherRef.kind == "edit"
          ? DispatchEditFormLauncherTemplate<T>()
          : props.context.launcherRef.kind == "create"
            ? DispatchCreateFormLauncherTemplate<T>()
            : "Unknown launcher kind";

    if (InstantiatedTemplate == "Unknown launcher kind") {
      console.error("Unknown launcher kind", props.context.launcherRef);
      return (
        props.context.errorComponent ?? <>Error: Check console for details</>
      );
    }

    return (
      <InstantiatedTemplate
        context={{
          ...props.context,
          launcherRef: props.context.launcherRef as any,
        }}
        setState={(_: BasicUpdater<any>) =>
          props.setState(
            DispatchFormRunnerState<T, Flags>().Updaters.formState(_),
          )
        }
        view={unit}
        foreignMutations={props.foreignMutations}
      />
    );
  }).any([DispatchFormRunner()]);
