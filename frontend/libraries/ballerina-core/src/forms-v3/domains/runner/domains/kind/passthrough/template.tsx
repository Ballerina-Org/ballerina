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
import React from "react";
import {
  RegistryStoreProvider,
  createRegistryStore,
} from "../../abstract-renderers/registry-store";

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
    const config = props.context.launcherRef.config;
    const registry = props.context.launcherRef.valueRegistry;

    // Initialize local store once and update imperatively on registry changes without relying on props for children
    const store = React.useMemo(() => createRegistryStore(registry), []);
    React.useEffect(() => {
      store.set(registry);
    }, [registry, store]);

    if (config.kind == "r" || props.context.status.kind == "not initialized") {
      return <></>;
    }

    if (config.value.kind == "errors") {
      console.error(config.value.errors.map((error) => error).join("\n"));
      return (
        props.context.errorComponent ?? <>Error: Check console for details</>
      );
    }

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
      <RegistryStoreProvider store={store}>
        <props.context.status.Form
          context={{
            ...props.context.formState,
            locked: false,
            disabled: props.context.globallyDisabled,
            globallyDisabled: props.context.globallyDisabled,
            readOnly: props.context.globallyReadOnly,
            globallyReadOnly: props.context.globallyReadOnly,
            type: DispatchParsedType.Default.primitive("unit"), // currently unused here
            localBindingsPath: `[${props.context.formName}]`,
            globalBindings: config.value.value,
            extraContext: props.context.extraContext,
            remoteEntityVersionIdentifier:
              props.context.remoteEntityVersionIdentifier,
            domNodeAncestorPath: "",
            labelContext: props.context.formName,
            lookupTypeAncestorNames: [],
            customPresentationContext: undefined,
            typeAncestors: [],
            path: `[root]`,
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
              props.context.launcherRef.onEntityChange(updater, delta);
            },
          }}
        />
      </RegistryStoreProvider>
    );
  }).any([
    DispatchPassthroughFormRunner<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >().mapContext((context) => context),
  ]);
