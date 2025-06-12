import { OrderedMap } from "immutable";
import { BasicUpdater, Updater } from "../fun/domains/updater/state";
import { replaceWith } from "../fun/domains/updater/domains/replaceWith/state";
import { simpleUpdater } from "../fun/domains/updater/domains/simpleUpdater/state";
import { AsyncState } from "../async/state";
import { Unit } from "../fun/domains/unit/state";
import { BasicFun } from "../fun/state";

export type StreamingStatus = "reload" | "loadMore" | false;

type Identifiable = { Id: string };

export type StreamPosition = {
  chunkSize: number;
  chunkIndex: number;
  shouldLoad: StreamingStatus;
  lastModifiedTime: number;
};

export const StreamPosition = {
  Default: (
    initialChunkSize: number,
    shouldLoad: StreamingStatus = false,
  ): StreamPosition => ({
    chunkSize: initialChunkSize,
    chunkIndex: 0,
    shouldLoad,
    lastModifiedTime: Date.now(),
  }),
  Updaters: {
    Template: {
      loadInitial: (): Updater<StreamPosition> =>
        StreamPosition.Updaters.Core.lastModifiedTime(
          replaceWith(Date.now()),
        ).then(
          StreamPosition.Updaters.Core.shouldLoad(
            replaceWith<StreamPosition["shouldLoad"]>("loadMore"),
          ),
        ),
      changeChunkSize: (chunkSize: number): Updater<StreamPosition> =>
        StreamPosition.Updaters.Core.chunkSize(replaceWith(chunkSize)).then(
          StreamPosition.Updaters.Template.reload(),
        ),
      reload: (): Updater<StreamPosition> =>
        StreamPosition.Updaters.Core.lastModifiedTime(replaceWith(Date.now()))
          .then(
            StreamPosition.Updaters.Core.shouldLoad(
              replaceWith<StreamPosition["shouldLoad"]>("reload"),
            ),
          )
          .then(
            StreamPosition.Updaters.Core.chunkIndex(
              replaceWith<StreamPosition["chunkIndex"]>(0),
            ),
          ),
      loadMore: (): Updater<StreamPosition> =>
        StreamPosition.Updaters.Core.chunkIndex((_) => _ + 1).then(
          StreamPosition.Updaters.Core.lastModifiedTime(
            replaceWith(Date.now()),
          ).then(
            StreamPosition.Updaters.Core.shouldLoad(
              replaceWith<StreamPosition["shouldLoad"]>("loadMore"),
            ),
          ),
        ),
    },
    Core: {
      ...simpleUpdater<StreamPosition>()("lastModifiedTime"),
      ...simpleUpdater<StreamPosition>()("shouldLoad"),
      ...simpleUpdater<StreamPosition>()("chunkIndex"),
      ...simpleUpdater<StreamPosition>()("chunkSize"),
    },
    Coroutine: {},
  },
};

export type Chunk<Element extends Identifiable> = {
  hasMoreValues: boolean;
  data: OrderedMap<Element["Id"], Element>;
};
export const Chunk = <Element extends Identifiable>() => ({
  Default: (
    hasMoreValues: boolean,
    data: OrderedMap<Element["Id"], Element>,
  ): Chunk<Element> => ({
    hasMoreValues,
    data,
  }),
});

export type InfiniteStreamState<Element extends Identifiable> = {
  loadingMore: AsyncState<Unit>;
  loadedElements: OrderedMap<StreamPosition["chunkIndex"], Chunk<Element>>;
  position: StreamPosition;
  getChunk: BasicFun<[StreamPosition], Promise<Chunk<Element>>>;
};

export const InfiniteStreamState = <Element extends Identifiable>() => ({
  Default: (
    initialChunkSize: number,
    getChunk: InfiniteStreamState<Element>["getChunk"],
    shouldLoad?: StreamingStatus,
  ): InfiniteStreamState<Element> => ({
    loadingMore: AsyncState.Default.unloaded(),
    loadedElements: OrderedMap(),
    position: StreamPosition.Default(initialChunkSize, shouldLoad),
    getChunk,
  }),
  Operations: {
    shouldCoroutineRun: (current: InfiniteStreamState<Element>): boolean =>
      current.position.shouldLoad != false,
    loadNextPage: (current: InfiniteStreamState<Element>): boolean =>
      current.position.shouldLoad !== false &&
      current.loadedElements.last()?.hasMoreValues !== false,
  },
  Updaters: {
    Coroutine: {
      addLoadedChunk: (
        chunkIndex: number,
        newChunk: Chunk<Element>,
      ): Updater<InfiniteStreamState<Element>> =>
        InfiniteStreamState<Element>().Updaters.Core.loadedElements((_) =>
          _.set(chunkIndex, newChunk),
        ),
    },
    Core: {
      ...simpleUpdater<InfiniteStreamState<Element>>()("getChunk"),
      ...simpleUpdater<InfiniteStreamState<Element>>()("loadingMore"),
      ...simpleUpdater<InfiniteStreamState<Element>>()("loadedElements"),
      whenNotAlreadyLoading: (
        _: BasicUpdater<InfiniteStreamState<Element>>,
      ): Updater<InfiniteStreamState<Element>> => {
        return Updater((current) => {
          if (InfiniteStreamState<Element>().Operations.loadNextPage(current)) {
            return current;
          }
          return _(current);
        });
      },
      position: (
        positionUpdater: Updater<InfiniteStreamState<Element>["position"]>,
      ): Updater<InfiniteStreamState<Element>> =>
        Updater((current) => {
          const newPosition = positionUpdater(current.position);
          let newState = current;
          if (newPosition.chunkSize != current.position.chunkSize)
            newState =
              InfiniteStreamState<Element>().Updaters.Core.clearLoadedElements()(
                newState,
              );
          return { ...newState, position: newPosition };
        }),
      clearLoadedElements: (): Updater<InfiniteStreamState<Element>> =>
        InfiniteStreamState<Element>().Updaters.Core.loadedElements((_) =>
          OrderedMap(),
        ),
    },
    Template: {
      loadInitial: (): Updater<InfiniteStreamState<Element>> =>
        InfiniteStreamState<Element>().Updaters.Core.position(
          StreamPosition.Updaters.Template.loadInitial(),
        ),
      reload: (
        getChunk: InfiniteStreamState<Element>["getChunk"],
      ): Updater<InfiniteStreamState<Element>> =>
        InfiniteStreamState<Element>()
          .Updaters.Core.position(StreamPosition.Updaters.Template.reload())
          .then(
            InfiniteStreamState<Element>().Updaters.Core.clearLoadedElements(),
          )
          .then(
            InfiniteStreamState<Element>().Updaters.Core.getChunk(
              replaceWith(getChunk),
            ),
          ),
      loadMore: (): Updater<InfiniteStreamState<Element>> =>
        InfiniteStreamState<Element>().Updaters.Core.whenNotAlreadyLoading(
          InfiniteStreamState<Element>().Updaters.Core.position(
            StreamPosition.Updaters.Template.loadMore(),
          ),
        ),
    },
  },
});

export type InfiniteStreamReadonlyState = Unit;
export type InfiniteStreamWritableState<Element extends { Id: string }> =
  InfiniteStreamState<Element>;
