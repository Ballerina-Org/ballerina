import {
  OneAbstractRendererState,
  replaceWith,
  Sum,
  Unit,
  ValueInfiniteStreamState,
  ValueOption,
  ValueOrErrors,
  ValueUnit,
  Option,
  InitializationStatus,
} from "../../../../../../../../../main";
import { InitializeCo } from "./builder";
import { DispatchDelta } from "../../../deltas/dispatch-delta/state";
import { BaseFlags } from "../../../deltas/delta-to-dto/state";

export const initializeOne = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  InitializeCo<CustomPresentationContext, ExtraContext>()
    .GetState()
    .then((current) => {
      const maybeId = OneAbstractRendererState.Operations.GetIdFromContext(
        current,
      ).MapErrors((_) =>
        _.concat(
          `\n... in couroutine for\n...${current.domNodeAncestorPath + "[one]"}`,
        ),
      );

      if (maybeId.kind === "errors") {
        console.error(maybeId.errors.join("\n"));
        return InitializeCo<CustomPresentationContext, ExtraContext>().Wait(0);
      }

      return InitializeCo<CustomPresentationContext, ExtraContext>()
        .Await(
          // get Api being defined is in the run condition and is a sign that this could be lazy loaded
          () => current.getApi!(maybeId.value),
          (_) => console.error("error while getting api value for the one", _),
        )
        .then((value) =>
          InitializeCo<CustomPresentationContext, ExtraContext>()
            .Do(() => {
              current
                .fromApiParser(value.value)
                .Then((result) => {
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
                    sourceAncestorLookupTypeNames:
                      current.lookupTypeAncestorNames,
                  };
                  current.onChange(Option.Default.some(updater), delta);

                  return ValueOrErrors.Default.return(result);
                })
                .MapErrors((_) => {
                  console.error("error while parsing api value for the one", _);
                  return _;
                });
            })
            .then(() =>
              InitializeCo<CustomPresentationContext, ExtraContext>().Do(() => {
                if (
                  current.customFormState.initializationStatus.kind ===
                  "reinitializing"
                ) {
                  // always true when running this coroutine
                  current.customFormState.initializationStatus.afterReinitializationAction();
                }
              }),
            ),
        )
        .then(() =>
          InitializeCo<CustomPresentationContext, ExtraContext>().SetState(
            OneAbstractRendererState.Updaters.Core.customFormState.children.initializationStatus(
              replaceWith<InitializationStatus>({
                kind: "initialized",
              }),
            ),
          ),
        );
    });
