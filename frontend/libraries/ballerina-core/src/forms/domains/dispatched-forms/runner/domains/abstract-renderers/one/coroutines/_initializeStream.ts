import { Unit, replaceWith, Sum } from "../../../../../../../../../main";
import { ValueInfiniteStreamState } from "../../../../../../../../value-infinite-data-stream/state";
import { OneAbstractRendererState } from "../state";
import { Co } from "./builder";

export const initializeStream = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>()
    .GetState()
    .then((current) => {
      const InstantiatedCo = Co<CustomPresentationContext, ExtraContext>();
      const maybeId = OneAbstractRendererState.Operations.GetIdFromContext(
        current,
      ).MapErrors((_) =>
        _.concat(
          `\n... in couroutine for\n...${current.domNodeAncestorPath + "[one]"}`,
        ),
      );

      if (maybeId.kind === "errors") {
        console.error(maybeId.errors.join("\n"));
        return InstantiatedCo.Wait(0);
      }

      return InstantiatedCo.SetState(
        OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
          replaceWith(
            Sum.Default.left(
              ValueInfiniteStreamState.Default(
                100,
                current.customFormState.getChunkWithParams(maybeId.value)(
                  current.customFormState.streamParams.value,
                ),
              ),
            ),
          ),
        ),
      );
    });
