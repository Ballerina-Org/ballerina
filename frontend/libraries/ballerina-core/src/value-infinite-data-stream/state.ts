import { OrderedMap, Map } from "immutable";
import { BasicUpdater, Updater } from "../fun/domains/updater/state";
import { replaceWith } from "../fun/domains/updater/domains/replaceWith/state";
import { simpleUpdater } from "../fun/domains/updater/domains/simpleUpdater/state";
import { AsyncState } from "../async/state";
import { Unit } from "../fun/domains/unit/state";
import { BasicFun } from "../fun/state";
import {
  Guid,
  MapRepo,
  PredicateValue,
  Sum,
  ValueOrErrors,
  ValueRecord,
} from "../../main";

export type ValueStreamingStatus = "reload" | "loadMore" | false;

export type ValueStreamPosition = {
  nextStart: number;
  chunkSize: number;
  chunkIndex: ValueChunkIndex;
  shouldLoad: ValueStreamingStatus;
  lastModifiedTime: number;
};

export const ValueStreamPosition = {
  Default: (
    initialChunkSize: number,
    shouldLoad: ValueStreamingStatus = false,
  ): ValueStreamPosition => ({
    nextStart: 0,
    chunkSize: initialChunkSize,
    chunkIndex: 0,
    shouldLoad,
    lastModifiedTime: Date.now(),
  }),
  Updaters: {
    Template: {
      changeChunkSize: (chunkSize: number): Updater<ValueStreamPosition> =>
        ValueStreamPosition.Updaters.Core.chunkSize(
          replaceWith(chunkSize),
        ).then(ValueStreamPosition.Updaters.Template.reload()),
      reload: (): Updater<ValueStreamPosition> =>
        ValueStreamPosition.Updaters.Core.lastModifiedTime(
          replaceWith(Date.now()),
        )
          .then(
            ValueStreamPosition.Updaters.Core.shouldLoad(
              replaceWith<ValueStreamPosition["shouldLoad"]>("reload"),
            ),
          )
          .then(
            ValueStreamPosition.Updaters.Core.chunkIndex(
              replaceWith<ValueStreamPosition["chunkIndex"]>(0),
            ),
          ),
      loadMore: (): Updater<ValueStreamPosition> =>
        ValueStreamPosition.Updaters.Core.chunkIndex((_) => _ + 1).then(
          ValueStreamPosition.Updaters.Core.lastModifiedTime(
            replaceWith(Date.now()),
          ).then(
            ValueStreamPosition.Updaters.Core.shouldLoad(
              replaceWith<ValueStreamPosition["shouldLoad"]>("loadMore"),
            ),
          ),
        ),
    },
    Core: {
      ...simpleUpdater<ValueStreamPosition>()("lastModifiedTime"),
      ...simpleUpdater<ValueStreamPosition>()("shouldLoad"),
      ...simpleUpdater<ValueStreamPosition>()("chunkIndex"),
      ...simpleUpdater<ValueStreamPosition>()("chunkSize"),
      ...simpleUpdater<ValueStreamPosition>()("nextStart"),
    },
    Coroutine: {},
  },
};

export type ValueChunk = {
  hasMoreValues: boolean;
  data: OrderedMap<Guid, ValueRecord>;
  from: number;
  to: number;
};
export const ValueChunk = {
  Default: (
    hasMoreValues: boolean,
    data: OrderedMap<Guid, ValueRecord>,
    from: number,
    to: number,
  ): ValueChunk => ({
    hasMoreValues,
    data,
    from,
    to,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<ValueChunk>()("hasMoreValues"),
      ...simpleUpdater<ValueChunk>()("data"),
      ...simpleUpdater<ValueChunk>()("from"),
      ...simpleUpdater<ValueChunk>()("to"),
    },
    Template: {
      updateValue: (
        valueId: Guid,
        valueUpdater: BasicUpdater<ValueRecord>,
      ): Updater<ValueChunk> =>
        ValueChunk.Updaters.Core.data(
          MapRepo.Updaters.update(valueId, valueUpdater),
        ),
      updateValueItem: (
        valueId: Guid,
        itemId: Guid,
        valueUpdater: BasicUpdater<PredicateValue>,
      ): Updater<ValueChunk> =>
        ValueChunk.Updaters.Core.data(
          MapRepo.Updaters.update(
            valueId,
            ValueRecord.Updaters.update(itemId, valueUpdater),
          ),
        ),
    },
  },
};

export type ValueChunkIndex = number;
// TODO -- better state typing
export type StateChunkValue = Map<string, any>;

export const StateChunkItem = {
  Default: (state: Record<string, any>): StateChunkValue => Map(state),
};

export type StateChunk = Map<string, StateChunkValue>;

export const StateChunk = {
  Default: (state: Record<string, any>): StateChunk => Map(state),
};

export type ValueInfiniteStreamState = {
  loadingMore: AsyncState<Unit>;
  loadedElements: OrderedMap<ValueChunkIndex, ValueChunk>;
  position: ValueStreamPosition;
  getChunk: BasicFun<[ValueStreamPosition], Promise<ValueChunk>>;
  chunkStates: Map<ValueChunkIndex, StateChunk>;
};

export const ValueInfiniteStreamState = {
  Default: (
    initialChunkSize: number,
    getChunk: ValueInfiniteStreamState["getChunk"],
    shouldLoad?: ValueStreamingStatus,
  ): ValueInfiniteStreamState => ({
    loadingMore: AsyncState.Default.unloaded(),
    loadedElements: OrderedMap(),
    position: ValueStreamPosition.Default(initialChunkSize, shouldLoad),
    getChunk,
    chunkStates: Map(),
  }),
  Operations: {
    shouldCoroutineRun: (current: ValueInfiniteStreamState): boolean =>
      current.position.shouldLoad != false,
    loadNextPage: (current: ValueInfiniteStreamState): boolean =>
      current.position.shouldLoad !== false &&
      current.loadedElements.last()?.hasMoreValues !== false,
    getChunkIndexForValue: (
      current: ValueInfiniteStreamState,
      valueId: Guid,
    ): ValueOrErrors<number, string> =>
      ValueOrErrors.Default.return<number | undefined, string>(
        current.loadedElements.findLastKey((chunk) => chunk.data.has(valueId)),
      ).Then((chunkIndex) =>
        chunkIndex == undefined
          ? ValueOrErrors.Default.throwOne<number, string>(
              "value not found in any chunks",
            )
          : ValueOrErrors.Default.return<number, string>(chunkIndex),
      ),
  },
  Updaters: {
    Coroutine: {
      addLoadedChunk: (
        chunkIndex: number,
        newChunk: ValueChunk,
      ): Updater<ValueInfiniteStreamState> =>
        ValueInfiniteStreamState.Updaters.Core.loadedElements((_) =>
          _.set(chunkIndex, newChunk),
        ),
    },
    Core: {
      ...simpleUpdater<ValueInfiniteStreamState>()("getChunk"),
      ...simpleUpdater<ValueInfiniteStreamState>()("loadingMore"),
      ...simpleUpdater<ValueInfiniteStreamState>()("loadedElements"),
      ...simpleUpdater<ValueInfiniteStreamState>()("chunkStates"),
      whenNotAlreadyLoading: (
        _: BasicUpdater<ValueInfiniteStreamState>,
      ): Updater<ValueInfiniteStreamState> => {
        return Updater((current) => {
          if (ValueInfiniteStreamState.Operations.loadNextPage(current)) {
            return current;
          }
          return _(current);
        });
      },
      position: (
        positionUpdater: Updater<ValueInfiniteStreamState["position"]>,
      ): Updater<ValueInfiniteStreamState> =>
        Updater((current) => {
          const newPosition = positionUpdater(current.position);
          let newState = current;
          if (newPosition.chunkSize != current.position.chunkSize)
            newState =
              ValueInfiniteStreamState.Updaters.Core.clearLoadedElements()(
                newState,
              );
          return { ...newState, position: newPosition };
        }),
      clearLoadedElements: (): Updater<ValueInfiniteStreamState> =>
        ValueInfiniteStreamState.Updaters.Core.loadedElements((_) =>
          OrderedMap(),
        ),
    },
    Template: {
      reload: (
        getChunk: ValueInfiniteStreamState["getChunk"],
      ): Updater<ValueInfiniteStreamState> =>
        ValueInfiniteStreamState.Updaters.Core.position(
          ValueStreamPosition.Updaters.Template.reload(),
        )
          .then(ValueInfiniteStreamState.Updaters.Core.clearLoadedElements())
          .then(
            ValueInfiniteStreamState.Updaters.Core.getChunk(
              replaceWith(getChunk),
            ),
          ),
      loadMore: (): Updater<ValueInfiniteStreamState> =>
        ValueInfiniteStreamState.Updaters.Core.whenNotAlreadyLoading(
          ValueInfiniteStreamState.Updaters.Core.position(
            ValueStreamPosition.Updaters.Template.loadMore(),
          ),
        ),
      updateChunkValue:
        (chunkIndex: number, chunkValueKey: Guid) =>
        (
          valueUpdater: BasicUpdater<ValueRecord>,
        ): Updater<ValueInfiniteStreamState> =>
          ValueInfiniteStreamState.Updaters.Core.loadedElements(
            MapRepo.Updaters.update(
              chunkIndex,
              ValueChunk.Updaters.Template.updateValue(
                chunkValueKey,
                valueUpdater,
              ),
            ),
          ),
      updateChunkValueItem:
        (chunkIndex: number, chunkValueKey: Guid, chunkValueItemKey: Guid) =>
        (
          valueUpdater: BasicUpdater<PredicateValue>,
        ): Updater<ValueInfiniteStreamState> =>
          ValueInfiniteStreamState.Updaters.Core.loadedElements(
            MapRepo.Updaters.update(
              chunkIndex,
              ValueChunk.Updaters.Template.updateValueItem(
                chunkValueKey,
                chunkValueItemKey,
                valueUpdater,
              ),
            ),
          ),
      updateChunkStateValue:
        (chunkIndex: number, chunkStateValueKey: string) =>
        (
          stateUpdater: BasicUpdater<StateChunkValue>,
        ): Updater<ValueInfiniteStreamState> =>
          ValueInfiniteStreamState.Updaters.Core.chunkStates(
            MapRepo.Updaters.upsert(
              chunkIndex,
              () => StateChunk.Default({ [chunkStateValueKey]: Map() }),
              MapRepo.Updaters.update(chunkStateValueKey, stateUpdater),
            ),
          ),
      updateChunkStateValueItem:
        (
          chunkIndex: number,
          chunkStateValueKey: string,
          chunkStateValueItemKey: string,
          defaultStateValueItem: () => any,
        ) =>
        (stateUpdater: BasicUpdater<any>): Updater<ValueInfiniteStreamState> =>
          ValueInfiniteStreamState.Updaters.Core.chunkStates(
            MapRepo.Updaters.upsert(
              chunkIndex,
              () => StateChunk.Default(Map()),
              MapRepo.Updaters.upsert(
                chunkStateValueKey,
                () => StateChunk.Default(Map()),
                MapRepo.Updaters.upsert<string, any>(
                  chunkStateValueItemKey,
                  defaultStateValueItem,
                  stateUpdater,
                ),
              ),
            ),
          ),
    },
  },
};

export type ValueInfiniteStreamReadonlyContext = Unit;
export type ValueInfiniteStreamWritableState = ValueInfiniteStreamState;
