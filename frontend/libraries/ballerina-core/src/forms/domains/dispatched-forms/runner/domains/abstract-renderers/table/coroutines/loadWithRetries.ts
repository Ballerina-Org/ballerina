import {
  Coroutine,
  DispatchTableApiSource,
  PredicateValue,
  Sum,
  TableAbstractRendererForeignMutationsExpected,
  TableAbstractRendererReadonlyContext,
  TableAbstractRendererState,
  Unit,
  ValueOrErrors,
  ValueTable,
} from "../../../../../../../../../main";
import { InfiniteLoaderCo as Co } from "./builder";

const DEFAULT_CHUNK_SIZE = 20;

export const TableLoadWithRetries =
  <CustomPresentationContext = Unit, ExtraContext = Unit>(
    tableApiSource: DispatchTableApiSource,
    fromTableApiParser: (
      value: unknown,
    ) => ValueOrErrors<PredicateValue, string>,
  ) =>
  (maxRetries = 3) =>
  (
    attempt: number = 0,
  ): Coroutine<
    TableAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      Pick<TableAbstractRendererForeignMutationsExpected, "onChange"> &
      TableAbstractRendererState,
    TableAbstractRendererState,
    Sum<"permanent error", ValueTable>
  > =>
    attempt < maxRetries
      ? Co<CustomPresentationContext, ExtraContext>()
          .GetState()
          .then((current) =>
            Co<CustomPresentationContext, ExtraContext>()
              .Wait(attempt > 0 ? 1000 : 0)
              .then(() =>
                Co<CustomPresentationContext, ExtraContext>().Await(
                  () =>
                    tableApiSource.getMany(fromTableApiParser)({
                      chunkSize: DEFAULT_CHUNK_SIZE,
                      from:
                        current.customFormState.loadingState ==
                          "reload from 0" || current.value.to == 0
                          ? 0
                          : current.value.to + 1,
                      filtersAndSorting:
                        current.customFormState.filterAndSortParam === ""
                          ? undefined
                          : current.customFormState.filterAndSortParam,
                    }),
                  () => "error" as const,
                ),
              ),
          )
          .then((apiResult) =>
            apiResult.kind == "l"
              ? Co<CustomPresentationContext, ExtraContext>().Return(
                  Sum.Default.right(apiResult.value),
                )
              : TableLoadWithRetries<CustomPresentationContext, ExtraContext>(
                  tableApiSource,
                  fromTableApiParser,
                )(maxRetries)(attempt + 1),
          )
      : Co<CustomPresentationContext, ExtraContext>().Return(
          Sum.Default.left("permanent error"),
        );
