import { ValueInfiniteStreamLoader } from "../../../../../../../../value-infinite-data-stream/coroutines/infiniteLoader";
import { ValueInfiniteStreamState } from "../../../../../../../../value-infinite-data-stream/state";
import {
  ReferenceOneAbstractRendererForeignMutationsExpected,
  ReferenceOneAbstractRendererState,
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
import { initializeReferenceOne } from "./_initializeReferenceOne";
import { initializeStream } from "./_initializeStream";
import { debouncer } from "./_debouncer";

export const initializeReferenceOneRunner = <
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
>() =>
  InitializeCo<CustomPresentationContext, ExtraContext>().Template<
    ReferenceOneAbstractRendererForeignMutationsExpected<Flags>
  >(initializeReferenceOne<CustomPresentationContext, ExtraContext>(), {
    interval: 15,
    runFilter: (props) =>
      // if the value is some, we already have something to pass to the renderers
      // -> we don't have to run the initialization coroutine
      // if the inner value is unit, we are rendering a partial referenceOne
      props.context.value.kind === "option" &&
      !props.context.value.isSome &&
      props.context.getApi != undefined,
  });

export const initializeStreamRunnerReferenceOne = <
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>().Template<
    ReferenceOneAbstractRendererForeignMutationsExpected<Flags>
  >(initializeStream<CustomPresentationContext, ExtraContext>(), {
    interval: 15,
    runFilter: (props) =>
      props.context.customFormState.stream.kind === "r" &&
      props.context.customFormState.getChunkWithParams !== undefined,
  });

export const referenceOneTableDebouncerRunner = <
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
>() =>
  DebouncerCo<CustomPresentationContext, ExtraContext>().Template<
    ReferenceOneAbstractRendererForeignMutationsExpected<Flags>
  >(debouncer<CustomPresentationContext, ExtraContext>(), {
    interval: 15,
    runFilter: (props) =>
      Debounced.Operations.shouldCoroutineRun(
        props.context.customFormState.streamParams,
      ) && props.context.customFormState.getChunkWithParams !== undefined,
  });

export const referenceOneTableLoaderRunner = <
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>().Template<
    ReferenceOneAbstractRendererForeignMutationsExpected<Flags>
  >(
    ValueInfiniteStreamLoader().embed(
      (_) =>
        _.customFormState.stream.kind === "l"
          ? _.customFormState.stream.value
          : undefined,
      (upd) =>
        ReferenceOneAbstractRendererState.Updaters.Core.customFormState.children.stream(
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
