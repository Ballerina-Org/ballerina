import { Map } from "immutable";
import {
  DispatchParsedType,
  DispatchTableApiSource,
  PredicateValue,
  replaceWith,
  SumNType,
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
  filterTypes: Map<string, SumNType<any>>,
) => {
  return filterTypes.size == 0
    ? Co<CustomPresentationContext, ExtraContext>().SetState(
        TableAbstractRendererState.Updaters.Core.customFormState.children.isFilteringInitialized(
          // always set to true even if the first call fails so we don't block the flow
          replaceWith<TableAbstractRendererState["customFormState"]["isFilteringInitialized"]>(true),
        ),
      )
    : Co<CustomPresentationContext, ExtraContext>().Seq([
        Co<CustomPresentationContext, ExtraContext>()
          .GetState()
          .then((current) => {
            const getDefaultFiltersAndSorting =
              tableApiSource.getDefaultFiltersAndSorting(filterTypes);
            return Co<CustomPresentationContext, ExtraContext>()
              .Await(getDefaultFiltersAndSorting(parseFromApiByType), () =>
                console.error(
                  "error getting default filters and sorting from api",
                ),
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
