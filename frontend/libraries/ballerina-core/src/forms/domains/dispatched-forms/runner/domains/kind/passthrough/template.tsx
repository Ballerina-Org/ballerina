import {
  DispatchPassthroughFormLauncherContext,
  DispatchPassthroughFormLauncherState,
  DispatchPassthroughFormLauncherForeignMutationsExpected,
} from "./state";
import {
  BasicUpdater,
  Bindings,
  CommonAbstractRendererState,
  DispatchDelta,
  DispatchFormRunnerState,
  DispatchInjectablesTypes,
  DispatchParsedType,
  Option,
  PassthroughLauncherRef,
  PredicateValue,
  Template,
  unit,
  Unit,
  Updater,
} from "../../../../../../../../main";
import { DispatchPassthroughFormRunner } from "./coroutines/runner";
import { Map, OrderedMap } from "immutable";
import { useEffect } from "react";

export const DispatchPassthroughFormLauncherTemplate = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() =>
  Template.Default<
    DispatchPassthroughFormLauncherContext<
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
    DispatchPassthroughFormLauncherState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    DispatchPassthroughFormLauncherForeignMutationsExpected<T>
  >((props) => {
    const entity = props.context.launcherRef.entity;
    const config = props.context.launcherRef.config;

    useEffect(() => {
      if (entity.kind == "r") return;
      if (entity.value.kind == "errors") return;
      const a = performance.now();
      const sig = PredicateValue.Operations.GenerateSignature(
        PredicateValue.Default.record(
          OrderedMap<string, PredicateValue>([
            ["Hello", "Hello"],
            ["World", "2"],
            ["3", false]
          ]),
        ),
      );
      const b = performance.now();
      console.debug(`Time taken: ${b - a} milliseconds`);
      console.debug(sig);
    }, [entity.value]);

    const a = performance.now();

    if (
      entity.kind == "r" ||
      config.kind == "r" ||
      props.context.status.kind == "not initialized"
    ) {
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

    if (props.context.status.kind == "error") {
      console.error(
        props.context.status.errors.map((error) => error).join("\n"),
      );
      return (
        props.context.errorComponent ?? <>Error: Check console for details</>
      );
    }

    if (props.context.status.kind == "loading") {
      return props.context.loadingComponent ?? <></>;
    }

    return (
      <props.context.status.Form
        context={{
          ...props.context.formState,
          value: entity.value.value,
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
          labelContext: props.context.formName,
          lookupTypeAncestorNames: [],
          customPresentationContext: undefined,
          typeAncestors: [],
        }}
        setState={(stateUpdater: BasicUpdater<CommonAbstractRendererState>) =>
          props.setState(
            DispatchPassthroughFormLauncherState<
              T,
              Flags,
              CustomPresentationContext,
              ExtraContext
            >().Updaters.formState(stateUpdater),
          )
        }
        view={unit}
        foreignMutations={{
          ...props.foreignMutations,
          onChange: (
            updater: Option<BasicUpdater<PredicateValue>>,
            delta: DispatchDelta<Flags>,
          ) => {
            if (props.context.launcherRef.entity.kind == "r") return;
            props.context.launcherRef.onEntityChange(updater, delta);
          },
        }}
      />
    );
  }).any([
    DispatchPassthroughFormRunner<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >().mapContext((context) => context),
  ]);
