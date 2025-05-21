import { Map } from "immutable";
import {
  ValueInfiniteStreamState,
  ValueStreamPosition,
} from "../../../../../../../../value-infinite-data-stream/state";
import {
  replaceWith,
  AbstractTableRendererState,
  Unit,
  Value,
  Debounced,
  Debounce,
  BasicFun,
  id,
} from "../../../../../../../../../main";
import { AbstractTableRendererReadonlyContext } from "../../../../../../../../../main";
import { ValueRecord } from "../../../../../../../../../main";
import { CoTypedFactory } from "../../../../../../../../../main";

const DebouncedCo = CoTypedFactory<
  Value<Map<string, string>>,
  ValueInfiniteStreamState
>();

// TODO -- very unsafe, needs work, checking undefined etc,,,
const DEFAULT_CHUNK_SIZE = 20;
// if value exists in entity, use that, otherwise load first chunk from infinite stream
const intialiseTable = <FilteringAndSortingState>() => {
  const Co = CoTypedFactory<
    AbstractTableRendererReadonlyContext<FilteringAndSortingState>,
    AbstractTableRendererState<FilteringAndSortingState>
  >();
  return Co.GetState().then((current) => {
    if (current.value == undefined) {
      return Co.Wait(0);
    }
    const initialData = current.value.data;
    const hasMoreValues = current.value.hasMoreValues;
    const from = current.value.from;
    const to = current.value.to;
    const getChunkWithParams = current.tableApiSource.getMany(
      current.fromTableApiParser,
    );

    return Co.SetState(
      replaceWith(
        AbstractTableRendererState<FilteringAndSortingState>().Default(),
      ).then(
        AbstractTableRendererState<FilteringAndSortingState>()
          .Updaters.Core.customFormState.children.stream(
            replaceWith(
              ValueInfiniteStreamState.Default(
                DEFAULT_CHUNK_SIZE,
                getChunkWithParams(Map<string, string>()),
                initialData.size == 0 && hasMoreValues ? "loadMore" : false,
              ),
            )
              .then(
                ValueInfiniteStreamState.Updaters.Coroutine.addLoadedChunk(0, {
                  data: initialData,
                  hasMoreValues: hasMoreValues,
                  from,
                  to,
                }),
              )
              .then(
                ValueInfiniteStreamState.Updaters.Core.position(
                  ValueStreamPosition.Updaters.Core.nextStart(
                    replaceWith(to + 1),
                  ),
                ),
              ),
          )
          .then(
            AbstractTableRendererState<FilteringAndSortingState>().Updaters.Core.customFormState.children.getChunkWithParams(
              replaceWith(getChunkWithParams),
            ),
          )
          .then(
            AbstractTableRendererState<FilteringAndSortingState>().Updaters.Core.customFormState.children.isInitialized(
              replaceWith(true),
            ),
          ),
      ),
    );
  });
};

const reloadTable = <FilteringAndSortingState>() => {
  const Co = CoTypedFactory<
    AbstractTableRendererReadonlyContext<FilteringAndSortingState>,
    AbstractTableRendererState<FilteringAndSortingState>
  >();

  return Co.Repeat(
    Co.Seq([
      Debounce<Value<Map<string, string>>, ValueInfiniteStreamState>(
        DebouncedCo.GetState()
          .then((current) => {
            return DebouncedCo.SetState(
              ValueInfiniteStreamState.Updaters.Template.reload(
                current.getChunkWithParams(current.streamParams.value),
              ),
            );
          })
          .then((_) => DebouncedCo.Return("success")),
        250,
      ).embed(
        (_) => ({
          ..._.customFormState,
          ..._.customFormState.stream,
          ..._.customFormState.streamParams,
        }),
        AbstractTableRendererState.Updaters.Core.customFormState.children
          .streamParams,
      ),
      Co.Wait(0),
    ]),
  );
};

export const TableRunner = Co.Template<Unit>(intialiseTable, {
  interval: 15,
  runFilter: (props) => {
    return !props.context.customFormState.isInitialized;
  },
});

export const TableReloadRunner = Co.Template<Unit>(reloadTable, {
  interval: 15,
  runFilter: (props) => {
    return Debounced.Operations.shouldCoroutineRun(
      props.context.customFormState.streamParams,
    );
  },
});
