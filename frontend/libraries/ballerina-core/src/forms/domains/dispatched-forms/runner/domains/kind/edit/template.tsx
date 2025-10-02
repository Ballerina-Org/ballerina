import {
  DispatchEditFormLauncherContext,
  DispatchEditFormLauncherState,
  DispatchEditFormLauncherForeignMutationsExpected,
} from "./state";
import {
  DispatchInjectablesTypes,
  Template,
  Unit,
  AsyncState,
  Bindings,
  unit,
  DispatchParsedType,
} from "../../../../../../../../main";
import React from "react";
import { DispatchEditFormRunner } from "./coroutines/runner";
import { Map } from "immutable";

export const DispatchEditFormLauncherTemplate = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() =>
  Template.Default<
    DispatchEditFormLauncherContext<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    DispatchEditFormLauncherState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    DispatchEditFormLauncherForeignMutationsExpected<T>
  >((props) => {
    const entity = props.context.entity.sync;
    const config = props.context.config.sync;

    if (
      !AsyncState.Operations.hasValue(entity) ||
      !AsyncState.Operations.hasValue(config) ||
      props.context.status.kind == "not initialized"
    ) {
      return <></>;
    }

    if (props.context.status.kind == "error") {
      console.error(
        props.context.status.errors.map((error) => error).join("\n"),
      );
      return (
        props.context.errorComponent ?? <>Error: Check console for details</>
      );
    }

    if (config.kind == "failed-reload") {
      console.error(config.error);
      return (
        props.context.errorComponent ?? <>Error: Check console for details</>
      );
    }

    if (props.context.status.kind == "loading" || config.kind == "reloading") {
      return props.context.loadingComponent ?? <></>;
    }

    const bindings: Bindings = Map([
      ["global", config.value],
      ["root", entity.value],
      ["local", entity.value],
    ]);

    return (
      <props.context.status.Form
        context={{
          ...props.context.formState,
          value: entity.value,
          locked: false,
          disabled: props.context.globallyDisabled,
          globallyDisabled: props.context.globallyDisabled,
          readOnly: props.context.globallyReadOnly,
          globallyReadOnly: props.context.globallyReadOnly,
          type: DispatchParsedType.Default.primitive("unit"), // currently unused here
          bindings,
          extraContext: props.context.extraContext,
          remoteEntityVersionIdentifier:
            props.context.remoteEntityVersionIdentifier,
          domNodeAncestorPath: "",
          lookupTypeAncestorNames: [],
          customPresentationContext: undefined,
          typeAncestors: [],
        }}
        setState={(stateUpdater) =>
          props.setState(
            DispatchEditFormLauncherState<
              T,
              Flags,
              CustomPresentationContext,
              ExtraContext
            >().Updaters.Core.formState(stateUpdater),
          )
        }
        view={unit}
        foreignMutations={{
          ...props.foreignMutations,
          onChange: (pvUpdater, delta) => {
            if (pvUpdater.kind == "l") return;

            props.setState(
              DispatchEditFormLauncherState<
                T,
                Flags,
                CustomPresentationContext,
                ExtraContext
              >().Updaters.Template.entity(pvUpdater.value),
            );
          },
        }}
      />
    );
  }).any([
    DispatchEditFormRunner<T, Flags, CustomPresentationContext, ExtraContext>(),
  ]);
