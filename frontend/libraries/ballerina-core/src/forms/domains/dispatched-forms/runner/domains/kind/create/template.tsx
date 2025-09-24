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
  id,
} from "../../../../../../../../main";
import { DispatchCreateFormRunner } from "./coroutines/runner";
import { Map } from "immutable";

export const DispatchCreateFormLauncherTemplate = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>() =>
  Template.Default<
    DispatchCreateFormLauncherContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchCreateFormLauncherState<T, Flags>,
    DispatchCreateFormLauncherForeignMutationsExpected<T>
  >((props) => {
    const entity = props.context.entity.sync;

    // TODO: add global configuration

    if (
      !AsyncState.Operations.hasValue(entity) ||
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

    if (props.context.status.kind == "loading") {
      return props.context.loadingComponent ?? <>Loading...</>;
    }

    const bindings: Bindings = Map([
      [
        "global",
        PredicateValue.Default.record(
          Map([
            [
              "ERP",
              PredicateValue.Default.unionCase(
                "ERPSAP",
                PredicateValue.Default.record(
                  Map([
                    [
                      "Value",
                      PredicateValue.Default.unionCase(
                        "SAPS2",
                        PredicateValue.Default.record(
                          Map([
                            ["S2OnlyField", PredicateValue.Default.boolean()],
                          ]),
                        ),
                      ),
                    ],
                  ]),
                ),
              ),
            ],
          ]),
        ),
      ],
      ["root", entity.value],
      ["local", entity.value],
    ]);

    // TODO: add submit button support

    return (
      <props.context.status.Form
        context={{
          ...props.context.formState,
          value: entity.value,
          locked: false,
          disabled: false,
          bindings,
          extraContext: props.context.extraContext,
          remoteEntityVersionIdentifier:
            props.context.remoteEntityVersionIdentifier,
          domNodeAncestorPath: "",
          lookupTypeAncestorNames: [],
        }}
        setState={(stateUpdater) =>
          props.setState(
            DispatchCreateFormLauncherState<T, Flags>().Updaters.Core.formState(
              stateUpdater,
            ),
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
                Flags
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
      CustomPresentationContexts,
      ExtraContext
    >(),
  ]);
