import { Unit } from "../../../main";
import { AsyncState } from "../../async/state";
import { Coroutine } from "../../coroutines/state";
import { replaceWith } from "../../fun/domains/updater/domains/replaceWith/state";
import {
  ValueInfiniteStreamState,
  ValueInfiniteStreamWritableState,
  ValueStreamPosition,
} from "../state";
import { ValueStreamCo } from "./builder";

export const ValueInfiniteStreamLoader = (maxRetries = 3) => {
  const Co = ValueStreamCo();
  const updaters = ValueInfiniteStreamState.Updaters;

  const attemptLoad = (
    retryCount = 0,
  ): Coroutine<
    ValueInfiniteStreamWritableState,
    ValueInfiniteStreamWritableState,
    Unit
  > =>
    Co.GetState().then((current) =>
      current.loadingMore.kind === "loaded"
        ? Co.Return(true)
        : Co.Await(
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
                            replaceWith<ValueStreamPosition["shouldLoad"]>(
                              false,
                            ),
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
              : retryCount < maxRetries
                ? Co.Wait(500).then(() => attemptLoad(retryCount + 1))
                : Co.Return(false);
          }),
    );

  return Co.Seq([
    Co.SetState(
      updaters.Core.loadingMore(replaceWith(AsyncState.Default.loading())),
    ),
    attemptLoad(),
    Co.GetState().then((current) =>
      current.loadingMore.kind !== "loaded"
        ? Co.SetState(
            updaters.Core.loadingMore(
              replaceWith(AsyncState.Default.error("max retries reached")),
            ),
          )
        : Co.Wait(0),
    ),
  ]);
};
