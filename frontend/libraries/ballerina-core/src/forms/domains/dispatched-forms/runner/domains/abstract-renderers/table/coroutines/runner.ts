import { Map, Set } from "immutable";
import { ValueInfiniteStreamState } from "../../../../../../../../value-infinite-data-stream/state";
import {
  DispatchParsedType,
  DispatchTableApiSource,
  PredicateValue,
  SumNType,
  Unit,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { Co, InfiniteLoaderCo } from "./builder";
import {} from "./initialiseFiltersAndSorting";
import { InitialiseFiltersAndSorting } from "./initialiseFiltersAndSorting";
import { TableInfiniteLoader } from "./infiniteLoader";

// const intialiseTable = <
//   CustomPresentationContext = Unit,
//   ExtraContext = Unit,
// >() =>
//   Co<CustomPresentationContext, ExtraContext>()
//     .GetState()
//     .then((current) => {
//       const isLazyLoaded =
//         current.value.data.size == 0 && current.value.hasMoreValues;

//       const getChunkWithParams = current.tableApiSource.getMany(
//         current.fromTableApiParser,
//       );

//       const params =
//         current.customFormState.filterAndSortParam == ""
//           ? Map<string, string>()
//           : Map([
//               ["filtersAndSorting", current.customFormState.filterAndSortParam],
//             ]);

//       return Co<CustomPresentationContext, ExtraContext>().SetState(
//         TableAbstractRendererState.Updaters.Core.customFormState.children
//           .stream(
//             replaceWith(
//               ValueInfiniteStreamState.Default(
//                 DEFAULT_CHUNK_SIZE,
//                 getChunkWithParams(params),
//                 initialData.size == 0 && hasMoreValues ? "loadMore" : false,
//               ),
//             )
//               .then(
//                 ValueInfiniteStreamState.Updaters.Coroutine.addLoadedChunk(0, {
//                   data: initialData,
//                   hasMoreValues: hasMoreValues,
//                   from,
//                   to,
//                 }),
//               )
//               .then(
//                 ValueInfiniteStreamState.Updaters.Core.position(
//                   ValueStreamPosition.Updaters.Core.nextStart(replaceWith(to)),
//                 ),
//               ),
//           )
//           .thenMany([
//             TableAbstractRendererState.Updaters.Core.customFormState.children.rowStates(
//               replaceWith(Map()),
//             ),
//             TableAbstractRendererState.Updaters.Core.customFormState.children.selectedRows(
//               replaceWith(Set()),
//             ),
//             TableAbstractRendererState.Updaters.Core.customFormState.children.selectedDetailRow(
//               replaceWith(undefined as any),
//             ),
//             TableAbstractRendererState.Updaters.Core.customFormState.children.getChunkWithParams(
//               replaceWith(getChunkWithParams),
//             ),
//             TableAbstractRendererState.Updaters.Template.shouldReinitialize(
//               false,
//             ),
//             TableAbstractRendererState.Updaters.Core.customFormState.children.previousRemoteEntityVersionIdentifier(
//               replaceWith(current.remoteEntityVersionIdentifier),
//             ),
//             TableAbstractRendererState.Updaters.Core.customFormState.children.initializationStatus(
//               replaceWith<
//                 TableAbstractRendererState["customFormState"]["initializationStatus"]
//               >("initialized"),
//             ),
//           ]),
//       );
//     });

// const reinitialise = <
//   CustomPresentationContext = Unit,
//   ExtraContext = Unit,
// >() =>
//   Co<CustomPresentationContext, ExtraContext>()
//     .GetState()
//     .then((_) => {
//       return Co<CustomPresentationContext, ExtraContext>().SetState(
//         TableAbstractRendererState.Updaters.Core.customFormState.children.initializationStatus(
//           replaceWith<
//             TableAbstractRendererState["customFormState"]["initializationStatus"]
//           >("reinitializing"),
//         ),
//       );
//     });

export const TableInitialiseFiltersAndSortingRunner = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>(
  filterTypes: Map<string, SumNType<any>>,
  tableApiSource: DispatchTableApiSource,
  parseFromApiByType: (
    type: DispatchParsedType<any>,
  ) => (raw: any) => ValueOrErrors<PredicateValue, string>,
) =>
  Co<CustomPresentationContext, ExtraContext>().Template<any>(
    InitialiseFiltersAndSorting<CustomPresentationContext, ExtraContext>(
      tableApiSource,
      parseFromApiByType,
      filterTypes,
    ),
    {
      interval: 15,
      runFilter: (props) =>
        props.context.customFormState.isFilteringInitialized == false,
    },
  );

export const TableInfiniteLoaderRunner = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>(
  tableApiSource: DispatchTableApiSource,
  fromTableApiParser: (value: unknown) => ValueOrErrors<PredicateValue, string>,
) =>
  InfiniteLoaderCo<CustomPresentationContext, ExtraContext>().Template<any>(
    TableInfiniteLoader<CustomPresentationContext, ExtraContext>(
      tableApiSource,
      fromTableApiParser,
    )(),
    {
      interval: 15,
      runFilter: (props) =>
        (props.context.value.data.size == 0 &&
          props.context.value.hasMoreValues) ||
        props.context.customFormState.loadMore == "load more" ||
        props.context.customFormState.loadMore == "reload from 0",
    },
  );

// export const TableReinitialiseRunner = <
//   CustomPresentationContext = Unit,
//   ExtraContext = Unit,
// >() =>
//   Co<CustomPresentationContext, ExtraContext>().Template<any>(
//     reinitialise<CustomPresentationContext, ExtraContext>(),
//     {
//       interval: 15,
//       runFilter: (props) =>
//         props.context.customFormState.initializationStatus === "initialized" &&
//         props.context.customFormState.isFilteringInitialized &&
//         props.context.customFormState.shouldReinitialize,
//     },
//   );

// export const TableRunner = <
//   CustomPresentationContext = Unit,
//   ExtraContext = Unit,
// >() =>
//   Co<CustomPresentationContext, ExtraContext>().Template<any>(
//     intialiseTable<CustomPresentationContext, ExtraContext>(),
//     {
//       interval: 15,
//       runFilter: (props) => {
//         return (
//           (props.context.customFormState.initializationStatus ===
//             "not initialized" ||
//             props.context.customFormState.initializationStatus ===
//               "reinitializing") &&
//           props.context.customFormState.isFilteringInitialized
//         );
//       },
//     },
//   );
