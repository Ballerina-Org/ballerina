import { Map } from "immutable";

import {
  DispatchFormRunnerContext,
  DispatchFormRunnerForeignMutationsExpected,
  DispatchFormRunnerState,
} from "./state";

import { DispatchFormRunner } from "./coroutines/runner";
import {
  Bindings,
  Template,
  unit,
  BasicUpdater,
  DispatchDelta,
  PredicateValue,
  Updater,
} from "../../../../../main";

export const DispatchFormRunnerTemplate = <
  T extends { [key in keyof T]: { type: any; state: any } },
>() =>
  Template.Default<
    DispatchFormRunnerContext<T> & DispatchFormRunnerState<T>,
    DispatchFormRunnerState<T>,
    DispatchFormRunnerForeignMutationsExpected
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
          ...props.context.formState,
          value: entity.value.value,
          bindings,
          extraContext: props.context.extraContext,
          remoteEntityVersionIdentifier:
            props.context.remoteEntityVersionIdentifier,
        }}
        setState={(_: BasicUpdater<any>) =>
          props.setState(DispatchFormRunnerState().Updaters.formState(_))
        }
        view={unit}
        foreignMutations={{
          onChange: (
            updater: Updater<PredicateValue>,
            delta: DispatchDelta,
          ) => {
            if (props.context.launcherRef.entity.kind == "r") return;
            props.context.launcherRef.onEntityChange(updater, delta);
          },
        }}
      />
    ) : props.context.status.kind == "loading" ||
      props.context.status.kind == "not initialized" ? (
      (props.context.loadingComponent ?? <>Loading...</>)
    ) : (
      (props.context.errorComponent ?? <>Error: Check console for details</>)
    );
  }).any([DispatchFormRunner()]);
