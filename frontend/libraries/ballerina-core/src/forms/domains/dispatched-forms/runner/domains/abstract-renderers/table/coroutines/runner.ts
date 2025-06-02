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
} from "../../../../../../../../../main";
import { AbstractTableRendererReadonlyContext } from "../../../../../../../../../main";
import { CoTypedFactory } from "../../../../../../../../../main";

const Co = <TableParams>() => CoTypedFactory<
  AbstractTableRendererReadonlyContext<TableParams>,
  AbstractTableRendererState<TableParams>
>();

// TODO -- very unsafe, needs work, checking undefined etc,,,
const DEFAULT_CHUNK_SIZE = 20;
// if value exists in entity, use that, otherwise load first chunk from infinite stream
const initializeTable = <TableParams>() => Co<TableParams>().GetState().then((current) => {
  if (current.value == undefined) {
    return Co<TableParams>().Wait(0);
  }
  const initialData = current.value.data;
  const hasMoreValues = current.value.hasMoreValues;
  const from = current.value.from;
  const to = current.value.to;
  const getChunkWithParams = current.tableApiSource.getMany(
    current.fromTableApiParser,
  );

  return Co<TableParams>().SetState(
    replaceWith(AbstractTableRendererState<TableParams>().Default()).then(
      AbstractTableRendererState<TableParams>().Updaters.Core.customFormState.children
        .stream(
          replaceWith(
            ValueInfiniteStreamState.Default(
              DEFAULT_CHUNK_SIZE,
              getChunkWithParams(tableParams, Map<string, string>()), // FIXME: Nick
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
          AbstractTableRendererState<TableParams>().Updaters.Core.customFormState.children.getChunkWithParams(
            replaceWith(getChunkWithParams),
          ),
        )
        .then(
          AbstractTableRendererState<TableParams>().Updaters.Core.customFormState.children.isInitialized(
            replaceWith(true),
          ),
        ),
    ),
  );
});

export const TableRunner = <TableParams>() => Co<TableParams>().Template<any>(initializeTable<TableParams>(), {
  interval: 15,
  runFilter: (props) => {
    return !props.context.customFormState.isInitialized;
  },
});
