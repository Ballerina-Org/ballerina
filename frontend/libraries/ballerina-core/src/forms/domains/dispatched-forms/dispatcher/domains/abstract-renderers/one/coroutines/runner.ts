import { DispatchOnChange } from "../../../../state";

import { ValueInfiniteStreamLoader } from "../../../../../../../../value-infinite-data-stream/coroutines/infiniteLoader";
import { ValueInfiniteStreamState } from "../../../../../../../../value-infinite-data-stream/state";
import {
  OneAbstractRendererReadonlyContext,
  OneAbstractRendererState,
} from "../state";
import {
  Debounce,
  SimpleCallback,
  DispatchParsedType,
  Value,
  CoTypedFactory,
  ValueOption,
  Debounced,
} from "../../../../../../../../../main";

const Co = CoTypedFactory<
  OneAbstractRendererReadonlyContext,
  OneAbstractRendererState
>();

const DebouncerCo = CoTypedFactory<
  OneAbstractRendererReadonlyContext & { onDebounce: SimpleCallback<void> },
  OneAbstractRendererState
>();

const DebouncedCo = CoTypedFactory<
  { onDebounce: SimpleCallback<void> },
  Value<string>
>();


const debouncer = DebouncerCo.Repeat(
  DebouncerCo.Seq([
    Debounce<Value<string>, { onDebounce: SimpleCallback<void> }>(
      DebouncedCo.GetState()
        .then((current) => DebouncedCo.Do(() => current.onDebounce()))
        //.SetState(SearchNow.Updaters.reloadsRequested(_ => _ + 1))
        .then((_) => DebouncedCo.Return("success")),
      250,
    ).embed(
      (_) => ({ ..._, ..._.customFormState.searchText }),
      OneAbstractRendererState.Updaters.Core.customFormState.children
        .searchText,
    ),
    DebouncerCo.Wait(0),
  ]),
);

export const oneDebouncerRunner = DebouncerCo.Template<{
  onChange: DispatchOnChange<ValueOption>;
}>(debouncer, {
  interval: 15,
  runFilter: (props) =>
    Debounced.Operations.shouldCoroutineRun(
      props.context.customFormState.searchText,
    ),
});

export const oneLoaderRunner = Co.Template<{
  onChange: DispatchOnChange<ValueOption>;
}>(
  ValueInfiniteStreamLoader.embed(
    (_) => _.customFormState.stream,
    OneAbstractRendererState.Updaters.Core.customFormState.children.stream,
  ),
  {
    interval: 15,
    runFilter: (props) =>
      ValueInfiniteStreamState.Operations.shouldCoroutineRun(
        props.context.customFormState.stream,
      ),
  },
);
