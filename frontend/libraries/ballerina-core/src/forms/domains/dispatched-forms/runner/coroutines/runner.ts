import { AsyncState } from "../../../../../async/state";
import { CoTypedFactory } from "../../../../../coroutines/builder";
import {
  ApiSources,
  DispatchFormRunnerContext,
  DispatchFormRunnerForeignMutationsExpected,
  DispatchFormRunnerStatus,
} from "../state";
import { id } from "../../../../../fun/domains/id/state";
import { DispatchFormRunnerState } from "../state";
import { replaceWith } from "../../../../../fun/domains/updater/domains/replaceWith/state";
import { List } from "immutable";
import {
  DispatchInjectablesTypes,
  Dispatcher,
  DispatcherContext,
  DispatchParsedType,
  Renderer,
  ValueOrErrors,
  unit,
} from "../../../../../../main";

export type DispatcherContextWithApiSources<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
> = Omit<
  DispatcherContext<T, Flags, CustomPresentationContexts, ExtraContext>,
  "defaultState"
> &
  ApiSources & {
    defaultState: (
      t: DispatchParsedType<T>,
      renderer: Renderer<T>,
    ) => ValueOrErrors<any, string>;
  };

export const DispatchFormRunner = <
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
>() => {
  const Co = CoTypedFactory<
    DispatchFormRunnerContext<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    DispatchFormRunnerState<T, Flags>
  >();

  return Co.Template<DispatchFormRunnerForeignMutationsExpected>(
    Co.Return(unit),
    {
      interval: 15,
      runFilter: (_) => false,
    },
  );
};
