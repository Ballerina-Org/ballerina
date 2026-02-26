import { Unit, replaceWith, Sum } from "../../../../../../../../../main";
import { ValueInfiniteStreamState } from "../../../../../../../../value-infinite-data-stream/state";
import { ReferenceAbstractRendererState } from "../state";
import { Co } from "./builder";

export const initializeStream = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>()
    .GetState()
    .then((current) => {
      const InstantiatedCo = Co<CustomPresentationContext, ExtraContext>();
      const maybeId = ReferenceAbstractRendererState.Operations.GetIdFromContext(
        current,
      ).MapErrors((_) =>
        _.concat(
          `\n... in couroutine for\n...${current.domNodeAncestorPath + "[reference]"}`,
        ),
      );

      if (maybeId.kind === "errors") {
        console.error(maybeId.errors.join("\n"));
        return InstantiatedCo.Wait(0);
      }

      return InstantiatedCo.SetState(
        ReferenceAbstractRendererState.Updaters.Core.customFormState.children.stream(
          replaceWith(
            Sum.Default.left(
              ValueInfiniteStreamState.Default(
                100,
                // safe because we check for undefined in the runFilter
                current.customFormState.getChunkWithParams!(maybeId.value)( //TODO Suzan: injecting is not needed for the refs
                  current.customFormState.streamParams.value[0],
                ),
              ),
            ),
          ),
        ),
      );
    });
