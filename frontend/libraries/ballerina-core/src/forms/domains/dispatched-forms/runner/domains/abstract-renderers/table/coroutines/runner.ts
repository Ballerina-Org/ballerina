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

const Co = <CustomContext = Unit>() =>
  CoTypedFactory<
    AbstractTableRendererReadonlyContext<CustomContext>,
    AbstractTableRendererState
  >();

// TODO -- very unsafe, needs work, checking undefined etc,,,
const DEFAULT_CHUNK_SIZE = 20;
// if value exists in entity, use that, otherwise load first chunk from infinite stream
const intialiseTable = <CustomContext = Unit>() =>
  Co<CustomContext>()
    .GetState()
    .then((current) => {
      if (current.value == undefined) {
        return Co<CustomContext>().Wait(0);
      }
      const initialData = current.value.data;
      const hasMoreValues = current.value.hasMoreValues;
      const from = current.value.from;
      const to = current.value.to;
      const getChunkWithParams = current.tableApiSource.getMany(
        current.fromTableApiParser,
      );

      return Co<CustomContext>().SetState(
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
                  ValueInfiniteStreamState.Updaters.Coroutine.addLoadedChunk(
                    0,
                    {
                      data: initialData,
                      hasMoreValues: hasMoreValues,
                      from,
                      to,
                    },
                  ),
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

const reinitialise = <CustomContext = Unit>() =>
  Co<CustomContext>()
    .GetState()
    .then((_) => {
      return Co<CustomContext>().SetState(
        AbstractTableRendererState.Updaters.Core.customFormState.children.initializationStatus(
          replaceWith<
            AbstractTableRendererState["customFormState"]["initializationStatus"]
          >("reinitializing"),
        ),
      );
    });

export const TableReinitialiseRunner = <CustomContext = Unit>() =>
  Co<CustomContext>().Template<any>(reinitialise<CustomContext>(), {
    interval: 15,
    runFilter: (props) =>
      props.context.customFormState.initializationStatus === "initialized" &&
      props.context.customFormState.shouldReinitialize &&
      props.context.remoteEntityVersionIdentifier !==
        props.context.customFormState.previousRemoteEntityVersionIdentifier,
  });

export const TableRunner = <CustomContext = Unit>() =>
  Co<CustomContext>().Template<any>(intialiseTable<CustomContext>(), {
    interval: 15,
    runFilter: (props) => {
      return (
        props.context.customFormState.initializationStatus ===
          "not initialized" ||
        props.context.customFormState.initializationStatus === "reinitializing"
      );
    },
  });
