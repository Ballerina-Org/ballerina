import { Map, Set } from "immutable";
import { ValueInfiniteStreamState } from "../../../../../../../../value-infinite-data-stream/state";
import {
  DispatchParsedType,
  TableAbstractRendererForeignMutationsExpected,
  DispatchTableApiSource,
  PredicateValue,
  SumNType,
  Unit,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { PendingOperationsCo, Co, InfiniteLoaderCo } from "./builder";
import {} from "./initialiseFiltersAndSorting";
import { InitialiseFiltersAndSorting } from "./initialiseFiltersAndSorting";
import { TableInfiniteLoader } from "./infiniteLoader";
import { InitialiseTable } from "./initialiseTable";
import { AddBatch } from "./applyEdits";
import { DequeueRemoveOps } from "./dequeueRemoveOps";
import { TableAbstractRendererPendingOps } from "../domains/pending-operation/state";

export const TableInitialiseFiltersAndSortingRunner = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>(
  filterTypes: Map<string, SumNType<any>>,
  tableApiSource: DispatchTableApiSource,
  parseFromApiByType: (
    type: DispatchParsedType<any>,
  ) => (raw: any) => ValueOrErrors<PredicateValue, string>,
  parseToApiByType: (
    type: DispatchParsedType<any>,
    value: PredicateValue,
    state: any,
  ) => ValueOrErrors<any, string>,
) =>
  Co<CustomPresentationContext, ExtraContext>().Template<any>(
    InitialiseFiltersAndSorting<CustomPresentationContext, ExtraContext>(
      tableApiSource,
      parseFromApiByType,
      parseToApiByType,
      filterTypes,
    ),
    {
      interval: 15,
      runFilter: (props) =>
        props.context.customFormState.isFilteringInitialized == false,
    },
  );

export const TableInitialiseTableRunner = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>(
  tableApiSource: DispatchTableApiSource,
  fromTableApiParser: (value: unknown) => ValueOrErrors<PredicateValue, string>,
) =>
  InfiniteLoaderCo<CustomPresentationContext, ExtraContext>().Template<any>(
    InitialiseTable<CustomPresentationContext, ExtraContext>(
      tableApiSource,
      fromTableApiParser,
    ),
    {
      interval: 15,
      runFilter: (props) =>
        props.context.customFormState.isFilteringInitialized &&
        (props.context.customFormState.loadingState == "loading" ||
          props.context.customFormState.loadingState == "reload from 0"),
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
    ),
    {
      interval: 15,
      runFilter: (props) =>
        props.context.customFormState.isFilteringInitialized &&
        props.context.customFormState.loadingState == "loaded" &&
        (props.context.customFormState.loadMore == "load more" ||
          props.context.customFormState.loadMore == "loading more"),
    },
  );

export const ApplyEditsRunner = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  PendingOperationsCo<CustomPresentationContext, ExtraContext>().Template<any>(
    AddBatch<CustomPresentationContext, ExtraContext>(),
    {
      interval: 15,
      runFilter: (props) =>
        TableAbstractRendererPendingOps.Operations.hasNewData(
          props.context.value.data,
          props.context.customFormState.pendingOps,
        ),
    },
  );

export const DequeueRemoveOpsRunner = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  PendingOperationsCo<CustomPresentationContext, ExtraContext>().Template<any>(
    DequeueRemoveOps<CustomPresentationContext, ExtraContext>(),
    {
      interval: 15,
      runFilter: (props) =>
        TableAbstractRendererPendingOps.Operations.dataHasBeenRemoved(
          props.context.value.data,
          props.context.customFormState.pendingOps,
        ),
    },
  );
