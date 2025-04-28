import { AsyncState } from "../../async/state";
import { Coroutine } from "../../coroutines/state";
import { Unit } from "../../fun/domains/unit/state";
import { replaceWith } from "../../fun/domains/updater/domains/replaceWith/state";
import {
  InfiniteStreamState,
  InfiniteStreamWritableState,
  StreamPosition,
} from "../state";
import { StreamCo } from "./builder";

export const InfiniteStreamLoader = <Element extends { Id: string }>(
  maxRetries: number = 3,
) => {
  const Co = StreamCo<Element>();
  const updaters = InfiniteStreamState<Element>().Updaters;

  const attemptLoad = (
    retryCount = 0,
  ): Coroutine<
    InfiniteStreamWritableState<Element>,
    InfiniteStreamWritableState<Element>,
    Unit
  > =>
    Co.GetState().then((current) => {
      if (current.loadingMore.kind === "loaded") {
        return Co.Return(true);
      }

      return Co.Await(
        () => current.getChunk([current.position]),
        () => "error" as const,
      ).then((apiResult) =>
        apiResult.kind === "l"
          ? Co.SetState(
              updaters.Core.loadingMore(
                replaceWith(AsyncState.Default.loaded({})),
              ).then(
                updaters.Coroutine.addLoadedChunk(
                  current.position.chunkIndex,
                  apiResult.value,
                ).then(
                  updaters.Core.position(
                    StreamPosition.Updaters.Core.shouldLoad(
                      replaceWith<StreamPosition["shouldLoad"]>(false),
                    ),
                  ),
                ),
              ),
            )
          : retryCount < maxRetries
            ? Co.Wait(500).then(() => attemptLoad(retryCount + 1))
            : Co.Return(false),
      );
    });

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

export const Loader = InfiniteStreamLoader;
