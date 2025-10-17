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
          InitializeCo<CustomPresentationContext, ExtraContext>().Do(() => {
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
          }),
        );
    });
