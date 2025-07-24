import { ValueInfiniteStreamLoader } from "../../../../../../../../value-infinite-data-stream/coroutines/infiniteLoader";
import { ValueInfiniteStreamState } from "../../../../../../../../value-infinite-data-stream/state";
import {
  OneAbstractRendererForeignMutationsExpected,
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
  DispatchDelta,
  BaseFlags,
  Option,
} from "../../../../../../../../../main";
import { Map } from "immutable";

const Co = <CustomPresentationContext = Unit, ExtraContext = Unit>() =>
  CoTypedFactory<
    OneAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext>,
    OneAbstractRendererState
  >();
const InitializeCo = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  CoTypedFactory<
    OneAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      Pick<OneAbstractRendererForeignMutationsExpected, "onChange">,
    OneAbstractRendererState
  >();

const DebouncerCo = <CustomPresentationContext = Unit, ExtraContext = Unit>() =>
  CoTypedFactory<
    OneAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > & {
      onDebounce: SimpleCallback<void>;
    },
    OneAbstractRendererState
  >();

const DebouncedCo = CoTypedFactory<
  { onDebounce: SimpleCallback<void> },
  Value<Map<string, string>>
>();

const intializeOne = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  InitializeCo<CustomPresentationContext, ExtraContext>()
    .GetState()
    .then((current) => {
      const InstantiatedInitializeCo = InitializeCo<
        CustomPresentationContext,
        ExtraContext
      >();
      if (current.value == undefined) {
        return InstantiatedInitializeCo.Wait(0);
      }

      /// When initailising, in both stages, inject the id to the get chunk

      const local = current.bindings.get("local");
      if (local == undefined) {
        console.error(
          `local binding is undefined when intialising one\n... in couroutine for\n...${current.domNodeAncestorPath + "[one]"}`,
        );
        return InstantiatedInitializeCo.Wait(0);
      }

      if (!PredicateValue.Operations.IsRecord(local)) {
        console.error(
          `local binding is not a record when intialising one\n... in couroutine for\n...${current.domNodeAncestorPath + "[one]"}`,
        );
        return InstantiatedInitializeCo.Wait(0);
      }

      if (!local.fields.has("Id")) {
        console.error(
          `local binding is missing Id (check casing) when intialising one\n... in couroutine for\n...${current.domNodeAncestorPath + "[one]"}`,
        );
        return InstantiatedInitializeCo.Wait(0);
      }

      const id = local.fields.get("Id")!; // safe because of above check;
      if (!PredicateValue.Operations.IsString(id)) {
        console.error(
          `local Id is not a string when intialising one\n... in couroutine for\n...${current.domNodeAncestorPath + "[one]"}`,
        );
        return InstantiatedInitializeCo.Wait(0);
      }

      const initializationCompletedCo = InitializeCo<
        CustomPresentationContext,
        ExtraContext
      >().Seq([
        InstantiatedInitializeCo.Do(() => {
          console.debug("initializationCompletedCo");
        }),
        InstantiatedInitializeCo.SetState(
          OneAbstractRendererState.Updaters.Core.customFormState.children
            .previousRemoteEntityVersionIdentifier(
              replaceWith(current.remoteEntityVersionIdentifier),
            )
            .then(
              OneAbstractRendererState.Updaters.Core.customFormState.children.shouldReinitialize(
                replaceWith(false),
              ),
            )
            .then(
              current.customFormState.status == "open"
                ? OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
                    ValueInfiniteStreamState.Updaters.Template.loadMore(),
                  )
                : idUpdater,
            ),
        ),
      ]);

      const hasInitialValue =
        (PredicateValue.Operations.IsOption(current.value) &&
          current.value.isSome) ||
        PredicateValue.Operations.IsUnit(current.value);

      const initializeStreamCo = InitializeCo<
        CustomPresentationContext,
        ExtraContext
      >().SetState(
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
      );

      if (hasInitialValue) {
        return InstantiatedInitializeCo.Seq([
          initializeStreamCo,
          initializationCompletedCo,
        ]);
      }

      const initializeValueCo = InitializeCo<
        CustomPresentationContext,
        ExtraContext
      >()
        .Await(
          () => current.getApi(id),
          (_) => console.error("err"),
        )
        .then((value) =>
          InstantiatedInitializeCo.Do(() => {
            return current.fromApiParser(value.value).Then((result) => {
              const updater = replaceWith<ValueOption | ValueUnit>(
                ValueOption.Default.some(result),
              );
              const delta: DispatchDelta<BaseFlags> = {
                kind: "OneReplace",
                replace: result,
                flags: {
                  kind: "localOnly",
                },
                type: current.type,
                sourceAncestorLookupTypeNames: current.lookupTypeAncestorNames,
              };
              console.debug("initializeOne delta", delta);
              current.onChange(Option.Default.some(updater), delta);

              return ValueOrErrors.Default.return(result);
            });
          }),
        );

      return InstantiatedInitializeCo.Seq([
        initializeStreamCo,
        initializeValueCo,
        initializationCompletedCo,
      ]);
    });

const debouncer = <CustomPresentationContext = Unit, ExtraContext = Unit>() =>
  DebouncerCo<CustomPresentationContext, ExtraContext>().Repeat(
    DebouncerCo<CustomPresentationContext, ExtraContext>().Seq([
      Debounce<
        Value<Map<string, string>>,
        { onDebounce: SimpleCallback<void> }
      >(
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
      DebouncerCo<CustomPresentationContext, ExtraContext>().Wait(0),
    ]),
  );

export const initializeOneRunner = <
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
>() =>
  InitializeCo<CustomPresentationContext, ExtraContext>().Template<
    OneAbstractRendererForeignMutationsExpected<Flags>
  >(intializeOne<CustomPresentationContext, ExtraContext>(), {
    interval: 15,
    runFilter: (props) =>
      // if the value is some, we already have something to pass to the renderers
      // -> we don't have to run the initialization coroutine
      // if the inner value is unit, we are rendering a partial one
      (props.context.value.kind === "option" && !props.context.value.isSome) ||
      (props.context.customFormState.shouldReinitialize &&
        props.context.remoteEntityVersionIdentifier !==
          props.context.customFormState.previousRemoteEntityVersionIdentifier),
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
