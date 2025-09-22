import {
  DispatchPassthroughFormLauncherContext,
  DispatchPassthroughFormLauncherState,
  DispatchPassthroughFormLauncherForeignMutationsExpected,
} from "./state";
import {
  Bindings,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  Template,
  unit,
  Unit,
} from "../../../../../../../../main";
import { DispatchPassthroughFormRunner } from "./coroutines/runner";
import { Map } from "immutable";

export const DispatchPassthroughFormLauncherTemplate = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
>() =>
  Template.Default<
    DispatchPassthroughFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    > &
      DispatchFormRunnerState<T, Flags>,
    DispatchPassthroughFormLauncherState<T>,
    DispatchPassthroughFormLauncherForeignMutationsExpected<T>
  >((props) => {
    const entity = props.context.launcherRef.entity;
    const config = props.context.launcherRef.config;

    if (entity.kind == "r" || config.kind == "r") {
      return <></>;
    }

    if (entity.value.kind == "errors") {
      console.error(entity.value.errors.map((error) => error).join("\n"));
      return (
        props.context.errorComponent ?? <>Error: Check console for details</>
      );
    }

    if (config.value.kind == "errors") {
      console.error(config.value.errors.map((error) => error).join("\n"));
      return (
        props.context.errorComponent ?? <>Error: Check console for details</>
      );
    }

    const bindings: Bindings = Map([
      ["global", config.value.value],
      ["root", entity.value.value],
      ["local", entity.value.value],
    ]);

    return props.context.status.kind == "loaded" ? (
      <props.context.status.Form
        context={{
          ...props.context,
          ...props.context.formState,
          value: entity.value.value,
          locked: false,
          disabled: false,
          bindings,
          extraContext: props.context.extraContext,
          remoteEntityVersionIdentifier:
            props.context.remoteEntityVersionIdentifier,
          domNodeAncestorPath: "",
          lookupTypeAncestorNames: [],
          // domNodeAncestorPath: `[${props.context.launcherRef.name}]`,
        }}
        setState={props.setState}
        view={unit}
        foreignMutations={{
          ...props.foreignMutations,
          onChange: (updater, delta) => {
            if (props.context.launcherRef.entity.kind == "r") return;
            props.context.launcherRef.onEntityChange(updater, delta);
          },
        }}
      />
    ) : (
      <></>
    );
  }).any([
    DispatchPassthroughFormRunner<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >().mapContext((context) => context),
  ]);
