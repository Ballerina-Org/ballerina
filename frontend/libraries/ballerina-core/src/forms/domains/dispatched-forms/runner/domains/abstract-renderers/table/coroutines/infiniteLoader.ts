import {
  TableAbstractRendererState,
  Unit,
} from "../../../../../../../../../main";
import { AsyncState } from "../../../../../../../../async/state";
import { replaceWith } from "../../../../../../../../fun/domains/updater/domains/replaceWith/state";
import { Co } from "./builder";
import { Map } from "immutable";

export const TableInfiniteLoader = (maxRetries = 3) => {
  const attemptLoad = <CustomPresentationContext = Unit, ExtraContext = Unit>(
    retryCount = 0,
  ) =>
    Co<CustomPresentationContext, ExtraContext>()
      .GetState()
      .then((current) =>
        current.customFormState.isLoadingMore
          ? Co<CustomPresentationContext, ExtraContext>().Return(true)
          : Co<CustomPresentationContext, ExtraContext>()
              .Await(
                () =>
                  current.customFormState.getChunkWithParams(
                    current.customFormState.filterAndSortParam == ""
                      ? Map<string, string>()
                      : Map<string, string>([
                          [
                            "filtersAndSorting",
                            current.customFormState.filterAndSortParam,
                          ],
                        ]),
                  ),
                () => "error" as const,
              )
              .then((apiResult) => {
                return apiResult.kind == "l"
                  ? Co<CustomPresentationContext, ExtraContext>().SetState(
                      TableAbstractRendererState.Updaters.Core.customFormState.children.isLoadingMore(
                        replaceWith(false),
                      ),
                    )
                  : retryCount < maxRetries
                    ? Co<CustomPresentationContext, ExtraContext>()
                        .Wait(500)
                        .then(() => attemptLoad(retryCount + 1))
                    : Co<CustomPresentationContext, ExtraContext>().Return(
                        false,
                      );
              }),
      );

  return Co().Seq([
    Co().SetState(
      updaters.Core.loadingMore(replaceWith(AsyncState.Default.loading())),
    ),
    attemptLoad(),
    Co()
      .GetState()
      .then((current) =>
        current.loadingMore.kind !== "loaded"
          ? Co().SetState(
              updaters.Core.loadingMore(
                replaceWith(AsyncState.Default.error("max retries reached")),
              ),
            )
          : Co().Wait(0),
      ),
  ]);
};
