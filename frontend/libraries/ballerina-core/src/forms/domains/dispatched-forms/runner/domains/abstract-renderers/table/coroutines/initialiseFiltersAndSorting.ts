import { Map } from "immutable";
import {
  Coroutine,
  DispatchParsedType,
  DispatchTableApiSource,
  DispatchTableFiltersAndSorting,
  PredicateValue,
  replaceWith,
  Sum,
  SumNType,
  TableAbstractRendererReadonlyContext,
  TableAbstractRendererState,
  Unit,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { Co } from "./builder";

export const InitialiseFiltersAndSorting = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>(
  tableApiSource: DispatchTableApiSource,
  parseFromApiByType: (
    type: DispatchParsedType<any>,
  ) => (raw: any) => ValueOrErrors<PredicateValue, string>,
  parseToApiByType: (
    type: DispatchParsedType<any>,
    value: PredicateValue,
    state: any,
  ) => ValueOrErrors<any, string>,
  filterTypes: Map<string, SumNType<any>>,
) => {
  const getDefaultFiltersAndSortingWithRetries =
    (maxRetries = 3) =>
    (
      attempt: number = 0,
    ): Coroutine<
      TableAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      > &
        TableAbstractRendererState,
      TableAbstractRendererState,
      Sum<"permanent error", DispatchTableFiltersAndSorting>
    > =>
      attempt < maxRetries
        ? Co<CustomPresentationContext, ExtraContext>()
            .Await(
              tableApiSource.getDefaultFiltersAndSorting(filterTypes)(
                parseFromApiByType,
              ),
              () =>
                console.error(
                  "error getting default filters and sorting from api",
                ),
            )
            .then((filtersAndSorting) =>
              filtersAndSorting.kind == "l"
                ? Co<CustomPresentationContext, ExtraContext>().Return(
                    Sum.Default.right(filtersAndSorting.value),
                  )
                : getDefaultFiltersAndSortingWithRetries(maxRetries)(
                    attempt + 1,
                  ),
            )
        : Co<CustomPresentationContext, ExtraContext>().Return(
            Sum.Default.left("permanent error"),
          );

  return filterTypes.size == 0
    ? Co<CustomPresentationContext, ExtraContext>().SetState(
        TableAbstractRendererState.Updaters.Core.customFormState.children.isFilteringInitialized(
          replaceWith<
            TableAbstractRendererState["customFormState"]["isFilteringInitialized"]
          >(true),
        ),
      )
    : Co<CustomPresentationContext, ExtraContext>().Seq([
        getDefaultFiltersAndSortingWithRetries()().then((filtersAndSorting) =>
          filtersAndSorting.kind == "r"
            ? Co<CustomPresentationContext, ExtraContext>().SetState(
                TableAbstractRendererState.Updaters.Core.customFormState.children
                  .filters(replaceWith(filtersAndSorting.value.filters))
                  .then(
                    TableAbstractRendererState.Updaters.Core.customFormState.children.sorting(
                      replaceWith(filtersAndSorting.value.sorting),
                    ),
                  )
                  .then(
                    TableAbstractRendererState.Updaters.Core.customFormState.children.filterAndSortParam(
                      replaceWith(
                        TableAbstractRendererState.Operations.parseFiltersAndSortingToBase64String(
                          filtersAndSorting.value.filters,
                          filterTypes,
                          filtersAndSorting.value.sorting,
                          parseToApiByType,
                        ),
                      ),
                    ),
                  ),
              ) // in case of permanent error, we don't want to block the flow, so just do nothing
            : Co<CustomPresentationContext, ExtraContext>().Wait(0),
        ),
        Co<CustomPresentationContext, ExtraContext>().SetState(
          TableAbstractRendererState.Updaters.Core.customFormState.children.isFilteringInitialized(
            // always set to true even if the first call fails so we don't block the flow
            replaceWith(true),
          ),
        ),
      ]);
};
