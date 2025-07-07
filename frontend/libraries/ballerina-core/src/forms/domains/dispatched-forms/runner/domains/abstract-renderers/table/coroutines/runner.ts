import { Map } from "immutable";
import {
  ValueInfiniteStreamState,
  ValueStreamPosition,
} from "../../../../../../../../value-infinite-data-stream/state";
import {
  replaceWith,
  TableAbstractRendererState,
  Unit,
} from "../../../../../../../../../main";
import { TableAbstractRendererReadonlyContext } from "../../../../../../../../../main";
import { CoTypedFactory } from "../../../../../../../../../main";

const Co = <CustomPresentationContext = Unit, ExtraContext = Unit>() =>
  CoTypedFactory<
    TableAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    >,
    TableAbstractRendererState
  >();

// TODO -- very unsafe, needs work, checking undefined etc,,,
const DEFAULT_CHUNK_SIZE = 20;
// if value exists in entity, use that, otherwise load first chunk from infinite stream
const intialiseTable = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>()
    .GetState()
    .then((current) => {
      if (current.value == undefined) {
        return Co<CustomPresentationContext, ExtraContext>().Wait(0);
      }
      const initialData = current.value.data;
      const hasMoreValues = current.value.hasMoreValues;
      const from = current.value.from;
      const to = current.value.to;
      const getChunkWithParams = current.tableApiSource.getMany(
        current.fromTableApiParser,
      );

      return Co<CustomPresentationContext, ExtraContext>().SetState(
        replaceWith(TableAbstractRendererState.Default()).then(
          TableAbstractRendererState.Updaters.Core.customFormState.children
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
              TableAbstractRendererState.Updaters.Core.customFormState.children.getChunkWithParams(
                replaceWith(getChunkWithParams),
              ),
              TableAbstractRendererState.Updaters.Template.shouldReinitialize(
                false,
              ),
              TableAbstractRendererState.Updaters.Core.customFormState.children.previousRemoteEntityVersionIdentifier(
                replaceWith(current.remoteEntityVersionIdentifier),
              ),
              TableAbstractRendererState.Updaters.Core.customFormState.children.initializationStatus(
                replaceWith<
                  TableAbstractRendererState["customFormState"]["initializationStatus"]
                >("initialized"),
              ),
            ]),
        ),
      );
    });

const reinitialise = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>()
    .GetState()
    .then((_) => {
      return Co<CustomPresentationContext, ExtraContext>().SetState(
        TableAbstractRendererState.Updaters.Core.customFormState.children.initializationStatus(
          replaceWith<
            TableAbstractRendererState["customFormState"]["initializationStatus"]
          >("reinitializing"),
        ),
      );
    });

export const TableReinitialiseRunner = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>().Template<any>(
    reinitialise<CustomPresentationContext, ExtraContext>(),
    {
      interval: 15,
      runFilter: (props) =>
        props.context.customFormState.initializationStatus === "initialized" &&
        props.context.customFormState.shouldReinitialize,
    },
  );

export const TableRunner = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>().Template<any>(
    intialiseTable<CustomPresentationContext, ExtraContext>(),
    {
      interval: 15,
      runFilter: (props) => {
        return (
          props.context.customFormState.initializationStatus ===
            "not initialized" ||
          props.context.customFormState.initializationStatus ===
            "reinitializing"
        );
      },
    },
  );
