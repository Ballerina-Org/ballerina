import { Map, Set } from "immutable";
import {
  ValueInfiniteStreamState,
  ValueStreamPosition,
} from "../../../../../../../../value-infinite-data-stream/state";
import {
  replaceWith,
  SumNType,
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
const initialiseFiltersAndSorting = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>(
  filterTypes: Map<string, SumNType<any>>,
) => {
  return Co<CustomPresentationContext, ExtraContext>().Seq([
    Co<CustomPresentationContext, ExtraContext>()
      .GetState()
      .then((current) => {
        const getDefaultFiltersAndSorting =
          current.tableApiSource.getDefaultFiltersAndSorting(filterTypes);
        return Co<CustomPresentationContext, ExtraContext>()
          .Await(getDefaultFiltersAndSorting(current.parseFromApiByType), () =>
            console.error("error getting default filters and sorting"),
          )
          .then((filtersAndSorting) => {
            return filtersAndSorting.kind == "l"
              ? Co<CustomPresentationContext, ExtraContext>().SetState(
                  TableAbstractRendererState.Updaters.Core.customFormState.children
                    .filters(replaceWith(filtersAndSorting.value.filters))
                    .then(
                      TableAbstractRendererState.Updaters.Core.customFormState.children.sorting(
                        replaceWith(filtersAndSorting.value.sorting),
                      ),
                    ),
                )
              : Co<CustomPresentationContext, ExtraContext>().Wait(0);
          });
      }),
    Co<CustomPresentationContext, ExtraContext>().SetState(
      TableAbstractRendererState.Updaters.Core.customFormState.children.isFilteringInitialized(
        // always set to true even if the first call fails so we don't block the flow
        replaceWith(true),
      ),
    ),
  ]);
};

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

      console.debug("params", current.customFormState.filterAndSortParam);

      const params =
        current.customFormState.filterAndSortParam == ""
          ? Map<string, string>()
          : Map([
              ["filtersAndSorting", current.customFormState.filterAndSortParam],
            ]);

      return Co<CustomPresentationContext, ExtraContext>().SetState(
        TableAbstractRendererState.Updaters.Core.customFormState.children
          .stream(
            replaceWith(
              ValueInfiniteStreamState.Default(
                DEFAULT_CHUNK_SIZE,
                getChunkWithParams(params),
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
                  ValueStreamPosition.Updaters.Core.nextStart(replaceWith(to)),
                ),
              ),
          )
          .thenMany([
            TableAbstractRendererState.Updaters.Core.customFormState.children.rowStates(
              replaceWith(Map()),
            ),
            TableAbstractRendererState.Updaters.Core.customFormState.children.selectedRows(
              replaceWith(Set()),
            ),
            TableAbstractRendererState.Updaters.Core.customFormState.children.selectedDetailRow(
              replaceWith(undefined as any),
            ),
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

export const TableInitialiseFiltersAndSortingRunner = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>(
  filterTypes: Map<string, SumNType<any>>,
) =>
  Co<CustomPresentationContext, ExtraContext>().Template<any>(
    initialiseFiltersAndSorting<CustomPresentationContext, ExtraContext>(
      filterTypes,
    ),
    {
      interval: 15,
      runFilter: (props) =>
        props.context.customFormState.isFilteringInitialized == false,
    },
  );
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
