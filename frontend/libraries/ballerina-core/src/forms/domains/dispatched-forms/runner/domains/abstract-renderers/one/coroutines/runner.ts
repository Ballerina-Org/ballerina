import { ValueInfiniteStreamLoader } from "../../../../../../../../value-infinite-data-stream/coroutines/infiniteLoader";
import { ValueInfiniteStreamState } from "../../../../../../../../value-infinite-data-stream/state";
import {
  OneAbstractRendererReadonlyContext,
  OneAbstractRendererState,
} from "../state";
import {
  Debounce,
  SimpleCallback,
  Value,
  CoTypedFactory,
  ValueOption,
  Debounced,
  AsyncState,
  Synchronize,
  PredicateValue,
  Unit,
  Synchronized,
  ValueOrErrors,
  ValueRecord,
  ValueUnit,
  DispatchOnChange,
  replaceWith,
} from "../../../../../../../../../main";
import { Map } from "immutable";

const Co = CoTypedFactory<
  OneAbstractRendererReadonlyContext,
  OneAbstractRendererState
>();

const DebouncerCo = CoTypedFactory<
  OneAbstractRendererReadonlyContext & {
    onDebounce: SimpleCallback<void>;
  },
  OneAbstractRendererState
>();

const DebouncedCo = CoTypedFactory<
  { onDebounce: SimpleCallback<void> },
  Value<string>
>();

const intializeOne = Co.GetState().then((current) => {
  if (current.value == undefined) {
    return Co.Wait(0);
  }

  /// When initailising, in both stages, inject the id to the get chunk

  const local = current.bindings.get("local");
  if (local == undefined) {
    console.error(
      `local binding is undefined when intialising one\n...${current.identifiers.withLauncher}`,
    );
    return Co.Wait(0);
  }

  if (!PredicateValue.Operations.IsRecord(local)) {
    console.error(
      `local binding is not a record when intialising one\n...${current.identifiers.withLauncher}`,
    );
    return Co.Wait(0);
  }

  if (!local.fields.has("Id")) {
    console.error(
      `local binding is missing Id (check casing) when intialising one\n...${current.identifiers.withLauncher}`,
    );
    return Co.Wait(0);
  }

  const id = local.fields.get("Id")!; // safe because of above check;
  if (!PredicateValue.Operations.IsString(id)) {
    console.error(
      `local Id is not a string when intialising one\n...${current.identifiers.withLauncher}`,
    );
    return Co.Wait(0);
  }

  const hasInitialValue =
    (PredicateValue.Operations.IsOption(current.value) &&
      current.value.isSome) ||
    PredicateValue.Operations.IsUnit(current.value);
  if (hasInitialValue) {
    const initialValue =
      PredicateValue.Operations.IsOption(current.value) && current.value.isSome
        ? current.value.value
        : PredicateValue.Default.unit();

    return Co.SetState(
      OneAbstractRendererState.Updaters.Core.customFormState.children
        .selectedValue(
          Synchronized.Updaters.sync(
            AsyncState.Updaters.toLoaded(
              ValueOrErrors.Default.return(initialValue),
            ),
          ),
        )
        .then(
          OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
            replaceWith(
              ValueInfiniteStreamState.Default(
                10,
                current.customFormState.getChunkWithParams(id)(Map()),
              ),
            ),
          ),
        ),
    );
  }

  return Co.Seq([
    Co.SetState(
      OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
        replaceWith(
          ValueInfiniteStreamState.Default(
            10,
            current.customFormState.getChunkWithParams(id)(Map()),
          ),
        ),
      ),
    ),
    Synchronize<Unit, ValueOrErrors<ValueRecord | ValueUnit, string>>(
      (_) =>
        current.getApi(id).then((value) => {
          return current.fromApiParser(value).Then((result) => {
            return ValueOrErrors.Default.return(result);
          });
        }),
      () => "transient failure",
      5,
      150,
    ).embed(
      (_) => _.customFormState.selectedValue,
      (_) =>
        OneAbstractRendererState.Updaters.Core.customFormState.children.selectedValue(
          _,
        ),
    ),
  ]);
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
      OneAbstractRendererState.Updaters.Core.customFormState.children
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
