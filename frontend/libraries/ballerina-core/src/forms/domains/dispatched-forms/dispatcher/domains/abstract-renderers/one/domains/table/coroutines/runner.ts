import { DispatchOnChange } from "../../../../../../state";

import { ValueInfiniteStreamLoader } from "../../../../../../../../../../value-infinite-data-stream/coroutines/infiniteLoader";
import { ValueInfiniteStreamState } from "../../../../../../../../../../value-infinite-data-stream/state";
import {
  OneTableAbstractRendererReadonlyContext,
  OneTableAbstractRendererState,
} from "../state";
import {
  Debounce,
  SimpleCallback,
  DispatchParsedType,
  Value,
  CoTypedFactory,
  ValueOption,
  Debounced,
  replaceWith,
  AsyncState,
  Synchronize,
  PredicateValue,
  Unit,
  ApiErrors,
  Synchronized,
} from "../../../../../../../../../../../main";

const Co = CoTypedFactory<
  OneTableAbstractRendererReadonlyContext,
  OneTableAbstractRendererState
>();

const DebouncerCo = CoTypedFactory<
  OneTableAbstractRendererReadonlyContext & {
    onDebounce: SimpleCallback<void>;
  },
  OneTableAbstractRendererState
>();

const DebouncedCo = CoTypedFactory<
  { onDebounce: SimpleCallback<void> },
  Value<string>
>();

const intializeOne = Co.GetState().then((current) => {
  if (current.value == undefined) {
    return Co.Wait(0);
  }
  const hasInitialValue = current.value.isSome;
  if (hasInitialValue) {
    return Co.SetState(
      OneTableAbstractRendererState.Updaters.Core.customFormState.children.selectedValue(
        Synchronized.Updaters.sync(
          AsyncState.Updaters.toLoaded(current.value.value),
        ),
      ),
    );
  }

  return Synchronize<Unit, ValueOption>(
    (_) => current.api.get(current.id),
    () => "transient failure",
    5,
    150,
  ).embed(
    (_) => _.customFormState.selectedValue,
    OneTableAbstractRendererState.Updaters.Core.customFormState.children
      .selectedValue,
  );
});

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
      OneTableAbstractRendererState.Updaters.Core.customFormState.children
        .searchText,
    ),
    DebouncerCo.Wait(0),
  ]),
);

export const initializeOneRunner = Co.Template<{
  onChange: DispatchOnChange<ValueOption>;
}>(intializeOne, {
  interval: 15,
  runFilter: (props) =>
    !AsyncState.Operations.hasValue(
      props.context.customFormState.selectedValue.sync,
    ),
});
export const oneTableDebouncerRunner = DebouncerCo.Template<{
  onChange: DispatchOnChange<ValueOption>;
}>(debouncer, {
  interval: 15,
  runFilter: (props) =>
    Debounced.Operations.shouldCoroutineRun(
      props.context.customFormState.searchText,
    ),
});

export const oneTableLoaderRunner = Co.Template<{
  onChange: DispatchOnChange<ValueOption>;
}>(
  ValueInfiniteStreamLoader.embed(
    (_) => _.customFormState.stream,
    OneTableAbstractRendererState.Updaters.Core.customFormState.children.stream,
  ),
  {
    interval: 15,
    runFilter: (props) =>
      ValueInfiniteStreamState.Operations.shouldCoroutineRun(
        props.context.customFormState.stream,
      ),
  },
);
