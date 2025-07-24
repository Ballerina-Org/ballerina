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
import { getIdFromContext } from "./utils";

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

      const maybeId = getIdFromContext(current).MapErrors((_) =>
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
                    Sum.Updaters.left(
                      ValueInfiniteStreamState.Updaters.Template.loadMore(),
                    ),
                  )
                : IdUpdater,
            ),
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
        initializeValueCo,
        initializationCompletedCo,
      ]);
    });
