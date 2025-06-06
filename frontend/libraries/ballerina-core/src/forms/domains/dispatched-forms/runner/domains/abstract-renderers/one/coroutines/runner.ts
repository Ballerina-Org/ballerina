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
  id as idUpdater,
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
  Value<Map<string, string>>
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

  const initializationCompletedCo = Co.SetState(
    OneAbstractRendererState.Updaters.Core.customFormState.children
      .initializationStatus(
        replaceWith<
          OneAbstractRendererState["customFormState"]["initializationStatus"]
        >("initialized"),
      )
      .thenMany([
        OneAbstractRendererState.Updaters.Core.customFormState.children.previousRemoteEntityVersionIdentifier(
          replaceWith(current.remoteEntityVersionIdentifier),
        ),
        OneAbstractRendererState.Updaters.Core.customFormState.children.shouldReinitialize(
          replaceWith(false),
        ),
        current.customFormState.initializationStatus == "reinitializing" &&
        current.customFormState.status == "open"
          ? OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
              ValueInfiniteStreamState.Updaters.Template.loadMore(),
            )
          : idUpdater,
      ]),
  );

  const hasInitialValue =
    (PredicateValue.Operations.IsOption(current.value) &&
      current.value.isSome) ||
    PredicateValue.Operations.IsUnit(current.value);
  if (hasInitialValue) {
    const initialValue =
      PredicateValue.Operations.IsOption(current.value) && current.value.isSome
        ? current.value.value
        : PredicateValue.Default.unit();

    return Co.Seq([
      Co.SetState(
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
                  100,
                  current.customFormState.getChunkWithParams(id)(
                    current.customFormState.streamParams.value,
                  ),
                ),
              ),
            ),
          ),
      ),
      initializationCompletedCo,
    ]);
  }

  return Co.Seq([
    Co.SetState(
      OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
        replaceWith(
          ValueInfiniteStreamState.Default(
            100,
            current.customFormState.getChunkWithParams(id)(
              current.customFormState.streamParams.value,
            ),
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
    initializationCompletedCo,
  ]);
});

const debouncer = DebouncerCo.Repeat(
  DebouncerCo.Seq([
    Debounce<Value<Map<string, string>>, { onDebounce: SimpleCallback<void> }>(
      DebouncedCo.GetState()
        .then((current) => DebouncedCo.Do(() => current.onDebounce()))
        //.SetState(SearchNow.Updaters.reloadsRequested(_ => _ + 1))
        .then((_) => DebouncedCo.Return("success")),
      250,
    ).embed(
      (_) => ({
        ..._.customFormState.streamParams,
        onDebounce: _.onDebounce,
      }),
      OneAbstractRendererState.Updaters.Core.customFormState.children
        .streamParams,
    ),
    DebouncerCo.Wait(0),
  ]),
);

const reinitialize = Co.GetState().then((_) => {
  return Co.SetState(
    OneAbstractRendererState.Updaters.Core.customFormState.children.initializationStatus(
      replaceWith<
        OneAbstractRendererState["customFormState"]["initializationStatus"]
      >("reinitializing"),
    ),
  );
});

export const reinitializeOneRunner = Co.Template<any>(reinitialize, {
  interval: 15,
  runFilter: (props) =>
    props.context.customFormState.initializationStatus === "initialized" &&
    props.context.customFormState.shouldReinitialize &&
    props.context.remoteEntityVersionIdentifier !==
      props.context.customFormState.previousRemoteEntityVersionIdentifier,
});

export const initializeOneRunner = Co.Template<{
  onChange: DispatchOnChange<ValueOption>;
}>(intializeOne, {
  interval: 15,
  runFilter: (props) =>
    props.context.customFormState.initializationStatus === "not initialized" ||
    props.context.customFormState.initializationStatus === "reinitializing",
});

export const oneTableDebouncerRunner = DebouncerCo.Template<{
  onChange: DispatchOnChange<ValueOption>;
}>(debouncer, {
  interval: 15,
  runFilter: (props) =>
    Debounced.Operations.shouldCoroutineRun(
      props.context.customFormState.streamParams,
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
