import { ValueInfiniteStreamLoader } from "../../../../../../../../value-infinite-data-stream/coroutines/infiniteLoader";
import { ValueInfiniteStreamState } from "../../../../../../../../value-infinite-data-stream/state";
import {
  OneAbstractRendererForeignMutationsExpected,
  OneAbstractRendererState,
} from "../state";
import {
  SimpleCallback,
  Value,
  Debounced,
  Unit,
  ValueOrErrors,
  ValueRecord,
  BaseFlags,
  Sum,
} from "../../../../../../../../../main";
import { Map } from "immutable";
import { Co, DebouncerCo, InitializeCo } from "./builder";
import { initializeOne } from "./_initializeOne";
import { initializeStream } from "./_initializeStream";
import { debouncer } from "./_debouncer";

export const initializeOneRunner = <
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
>() =>
  InitializeCo<CustomPresentationContext, ExtraContext>().Template<
    OneAbstractRendererForeignMutationsExpected<Flags>
  >(initializeOne<CustomPresentationContext, ExtraContext>(), {
    interval: 15,
    runFilter: (props) =>
      // if the value is some, we already have something to pass to the renderers
      // -> we don't have to run the initialization coroutine
      // if the inner value is unit, we are rendering a partial one
      props.context.value.kind === "option" && !props.context.value.isSome,
  });

export const initializeStreamRunner = <
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>().Template<
    OneAbstractRendererForeignMutationsExpected<Flags>
  >(initializeStream<CustomPresentationContext, ExtraContext>(), {
    interval: 15,
    runFilter: (props) => props.context.customFormState.stream.kind === "r",
  });

export const oneTableDebouncerRunner = <
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
>() =>
  DebouncerCo<CustomPresentationContext, ExtraContext>().Template<
    OneAbstractRendererForeignMutationsExpected<Flags>
  >(debouncer<CustomPresentationContext, ExtraContext>(), {
    interval: 15,
    runFilter: (props) =>
      Debounced.Operations.shouldCoroutineRun(
        props.context.customFormState.streamParams,
      ),
  });

export const oneTableLoaderRunner = <
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>().Template<
    OneAbstractRendererForeignMutationsExpected<Flags>
  >(
    ValueInfiniteStreamLoader().embed(
      (_) =>
        _.customFormState.stream.kind === "l"
          ? _.customFormState.stream.value
          : undefined,
      (upd) =>
        OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
          Sum.Updaters.left(upd),
        ),
    ),
    {
      interval: 15,
      runFilter: (props) =>
        props.context.customFormState.stream.kind === "l" &&
        ValueInfiniteStreamState.Operations.shouldCoroutineRun(
          props.context.customFormState.stream.value,
        ),
    },
  );
