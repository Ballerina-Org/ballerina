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
import { v4 } from "uuid";

const Co = CoTypedFactory<
  AbstractTableRendererReadonlyContext,
  AbstractTableRendererState
>();

// TODO -- very unsafe, needs work, checking undefined etc,,,
const DEFAULT_CHUNK_SIZE = 20;
// if value exists in entity, use that, otherwise load first chunk from infinite stream
const intialiseTable = Co.GetState().then((current) => {
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
    replaceWith(AbstractTableRendererState.Default()).then(
      AbstractTableRendererState.Updaters.Core.customFormState.children
        .stream(
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
        .thenMany([
          AbstractTableRendererState.Updaters.Core.customFormState.children.getChunkWithParams(
            replaceWith(getChunkWithParams),
          ),
          AbstractTableRendererState.Updaters.Template.shouldReinitialize(
            false,
          ),
          AbstractTableRendererState.Updaters.Core.customFormState.children.previousRemoteEntityVersionIdentifier(
            replaceWith(current.remoteEntityVersionIdentifier),
          ),
          AbstractTableRendererState.Updaters.Core.customFormState.children.initializationStatus(
            replaceWith<
              AbstractTableRendererState["customFormState"]["initializationStatus"]
            >("initialized"),
          ),
        ]),
    ),
  );
});

const reinitialise = Co.GetState().then((_) => {
  return Co.SetState(
    AbstractTableRendererState.Updaters.Core.customFormState.children.initializationStatus(
      replaceWith<
        AbstractTableRendererState["customFormState"]["initializationStatus"]
      >("reinitializing"),
    ),
  );
});

export const TableReinitialiseRunner = Co.Template<any>(reinitialise, {
  interval: 15,
  runFilter: (props) =>
    props.context.customFormState.initializationStatus === "initialized" &&
    props.context.customFormState.shouldReinitialize &&
    props.context.remoteEntityVersionIdentifier !==
      props.context.customFormState.previousRemoteEntityVersionIdentifier,
});

export const TableRunner = Co.Template<any>(intialiseTable, {
  interval: 15,
  runFilter: (props) => {
    return (
      props.context.customFormState.initializationStatus ===
        "not initialized" ||
      props.context.customFormState.initializationStatus === "reinitializing"
    );
  },
});
