import {
  DispatchCreateFormLauncherContext,
  DispatchCreateFormLauncherState,
  DispatchCreateFormLauncherForeignMutationsExpected,
} from "./state";
import {
  DispatchInjectablesTypes,
  Template,
  Unit,
  AsyncState,
  unit,
  Bindings,
  PredicateValue,
  Synchronized,
  DispatchParsedType,
} from "../../../../../../../../main";
import { DispatchCreateFormRunner } from "./coroutines/runner";
import { Map } from "immutable";

export const DispatchCreateFormLauncherTemplate = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
>() =>
  Template.Default<
    DispatchCreateFormLauncherContext<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    DispatchCreateFormLauncherState<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    DispatchCreateFormLauncherForeignMutationsExpected<T>
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
          domNodeAncestorPath: `[${props.context.formName}]`,
          labelContext: props.context.formName,
          lookupTypeAncestorNames: [],
          customPresentationContext: undefined,
          typeAncestors: [],
        }}
        setState={(stateUpdater) =>
          props.setState(
            DispatchCreateFormLauncherState<
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
              DispatchCreateFormLauncherState<
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
    DispatchCreateFormRunner<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >(),
  ]);
