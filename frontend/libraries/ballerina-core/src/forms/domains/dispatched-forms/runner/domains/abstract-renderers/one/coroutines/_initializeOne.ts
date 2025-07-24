import {
  BaseFlags,
  DispatchDelta,
  OneAbstractRendererState,
  PredicateValue,
  replaceWith,
  Sum,
  Unit,
  id as IdUpdater,
  ValueInfiniteStreamState,
  ValueOption,
  ValueOrErrors,
  ValueUnit,
  Option,
} from "../../../../../../../../../main";
import { InitializeCo } from "./builder";
import { initializeStream } from "./_initializeStream";

export const initializeOne = <
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

      const maybeId = OneAbstractRendererState.Operations.GetIdFromContext(
        current,
      ).MapErrors((_) =>
        _.concat(
          `\n... in couroutine for\n...${current.domNodeAncestorPath + "[one]"}`,
        ),
      );

      if (maybeId.kind === "errors") {
        console.error(maybeId.errors.join("\n"));
        return InstantiatedInitializeCo.Wait(0);
      }

      const initializationCompletedCo = InitializeCo<
        CustomPresentationContext,
        ExtraContext
      >().Seq([
        InstantiatedInitializeCo.SetState(
          current.customFormState.status == "open"
            ? OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
                Sum.Updaters.left(
                  ValueInfiniteStreamState.Updaters.Template.loadMore(),
                ),
              )
            : IdUpdater,
        ),
      ]);

      const hasInitialValue =
        (PredicateValue.Operations.IsOption(current.value) &&
          current.value.isSome) ||
        PredicateValue.Operations.IsUnit(current.value);

      if (hasInitialValue) {
        return InstantiatedInitializeCo.Seq([initializationCompletedCo]);
      }

      const initializeValueCo = InitializeCo<
        CustomPresentationContext,
        ExtraContext
      >()
        .Await(
          () => current.getApi(maybeId.value),
          (_) => console.error("error while getting api value for the one", _),
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
              current.onChange(Option.Default.some(updater), delta);

              return ValueOrErrors.Default.return(result);
            });
          }),
        );

      const initializeStreamCo = initializeStream<
        CustomPresentationContext,
        ExtraContext
      >();

      return InstantiatedInitializeCo.Seq([
        initializeValueCo,
        initializeStreamCo,
        initializationCompletedCo,
      ]);
    });
