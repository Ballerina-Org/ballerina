import { AsyncState } from "../../async/state";
import { replaceWith } from "../../fun/domains/updater/domains/replaceWith/state";
import { ValueInfiniteStreamState, ValueStreamPosition } from "../state";
import { ValueStreamCo } from "./builder";

const Co = ValueStreamCo();
const updaters = ValueInfiniteStreamState.Updaters;

export const ValueInfiniteStreamLoader = Co.Seq([
  Co.SetState(
    updaters.Core.loadingMore(replaceWith(AsyncState.Default.loading())),
  ),
  Co.While(
    ([current]) => current.loadingMore.kind != "loaded",
    Co.GetState().then((current) =>
      Co.Await(
        () => current.getChunk([current.position]),
        () => "error" as const,
      ).then((apiResult) => {
        return apiResult.kind == "l"
          ? Co.SetState(
              updaters.Core.loadingMore(
                replaceWith(AsyncState.Default.loaded({})),
              )
                .then(
                  updaters.Coroutine.addLoadedChunk(
                    current.position.chunkIndex,
                    apiResult.value,
                  ).then(
                    updaters.Core.position(
                      ValueStreamPosition.Updaters.Core.shouldLoad(
                        replaceWith<ValueStreamPosition["shouldLoad"]>(false),
                      ),
                    ),
                  ),
                )
                .then(
                  updaters.Core.position(
                    ValueStreamPosition.Updaters.Core.nextStart(
                      replaceWith(apiResult.value.to + 1),
                    ),
                  ),
                ),
            )
          : Co.Wait(500);
      }),
    ),
  ),
]);
