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
  OneReinitilizationState,
} from "../../../../../../../../../main";
import { InitializeCo } from "./builder";

export const initializeOne = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() => {
  console.debug("initializeOne");

  return InitializeCo<CustomPresentationContext, ExtraContext>()
    .GetState()
    .then((current) => {
      console.debug("initializing one", current.domNodeAncestorPath);

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
          InitializeCo<CustomPresentationContext, ExtraContext>().Do(() => {
            current
              .fromApiParser(value.value)
              .Then((result) => {
                if (
                  current.domNodeAncestorPath.includes(
                    "InvoicePositionAccountingRows",
                  )
                ) {
                  console.group("InvoicePositionAccountingRows");
                  console.debug("one content", result);
                  console.groupEnd();
                }
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

                if (current.customFormState.reinitializing.status === "reinitializing") {
                  // always true when running this coroutine
                  current.customFormState.reinitializing.postprocessAction();
                }

                return ValueOrErrors.Default.return(result);
              })
              .MapErrors((_) => {
                console.error("error while parsing api value for the one", _);
                return _;
              });
          }),
        )
        .then(() =>
          InitializeCo<CustomPresentationContext, ExtraContext>().SetState(
            OneAbstractRendererState.Updaters.Core.customFormState.children.reinitializing(
              replaceWith<OneReinitilizationState>({
                status: "idle",
              }),
            ),
          ),
        );
    });
};
