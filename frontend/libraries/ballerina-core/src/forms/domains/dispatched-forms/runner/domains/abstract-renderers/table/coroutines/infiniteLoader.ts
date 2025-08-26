import {
  BaseFlags,
  Coroutine,
  DispatchDelta,
  DispatchTableApiSource,
  Option,
  PredicateValue,
  Sum,
  TableAbstractRendererForeignMutationsExpected,
  TableAbstractRendererReadonlyContext,
  TableAbstractRendererState,
  unit,
  Unit,
  ValueOrErrors,
  ValueTable,
} from "../../../../../../../../../main";
import { replaceWith } from "../../../../../../../../fun/domains/updater/domains/replaceWith/state";
import { InfiniteLoaderCo as Co } from "./builder";
import { Map } from "immutable";

const DEFAULT_CHUNK_SIZE = 20;

export const TableInfiniteLoader =
  <CustomPresentationContext = Unit, ExtraContext = Unit>(
    tableApiSource: DispatchTableApiSource,
    fromTableApiParser: (
      value: unknown,
    ) => ValueOrErrors<PredicateValue, string>,
  ) =>
  (maxRetries = 3) => {
    const loadWithRetries = <
      CustomPresentationContext = Unit,
      ExtraContext = Unit,
    >(
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
              Co<CustomPresentationContext, ExtraContext>().Await(
                () =>
                  tableApiSource.getMany(fromTableApiParser)({
                    chunkSize: DEFAULT_CHUNK_SIZE,
                    from: current.value.data.size, // TODO: should be reset to 0 when the filters and sorting change
                    filtersAndSorting:
                      current.customFormState.filterAndSortParam === ""
                        ? undefined
                        : current.customFormState.filterAndSortParam,
                  }),
                () => "error" as const,
              ),
            )
            .then((apiResult) =>
              apiResult.kind == "l"
                ? Co<CustomPresentationContext, ExtraContext>().Return(
                    Sum.Default.right(apiResult.value),
                  )
                : loadWithRetries<CustomPresentationContext, ExtraContext>(
                    attempt + 1,
                  ),
            )
        : Co<CustomPresentationContext, ExtraContext>().Return(
            Sum.Default.left("permanent error"),
          );

    return Co<CustomPresentationContext, ExtraContext>().Seq([
      Co<CustomPresentationContext, ExtraContext>().SetState(
        TableAbstractRendererState.Updaters.Core.customFormState.children.loadingState(
          replaceWith<
            TableAbstractRendererState["customFormState"]["loadingState"]
          >("loading"),
        ),
      ),
      loadWithRetries<CustomPresentationContext, ExtraContext>().then((res) =>
        res.kind == "r"
          ? Co<CustomPresentationContext, ExtraContext>().Seq([
              Co<CustomPresentationContext, ExtraContext>().SetState(
                TableAbstractRendererState.Updaters.Core.customFormState.children.loadingState(
                  replaceWith<
                    TableAbstractRendererState["customFormState"]["loadingState"]
                  >("loaded"),
                ),
              ),
              Co<CustomPresentationContext, ExtraContext>()
                .GetState()
                .then((current) => {
                  const updater = replaceWith<ValueTable>(
                    ValueTable.Default.fromParsed(
                      0,
                      current.value.data.size + res.value.data.size, // validate this
                      res.value.hasMoreValues,
                      current.value.data.concat(res.value.data),
                    ),
                  );
                  const delta: DispatchDelta<BaseFlags> = {
                    kind: "CustomDelta",
                    value: {
                      kind: "TableReplace",
                      replace: updater,
                    },
                    flags: {
                      kind: "localOnly",
                    },
                    sourceAncestorLookupTypeNames:
                      current.lookupTypeAncestorNames,
                  };
                  current.onChange(Option.Default.some(updater), delta);
                  return Co<CustomPresentationContext, ExtraContext>().Return(
                    unit,
                  );
                }),
            ])
          : Co<CustomPresentationContext, ExtraContext>().SetState(
              TableAbstractRendererState.Updaters.Core.customFormState.children.loadingState(
                replaceWith<
                  TableAbstractRendererState["customFormState"]["loadingState"]
                >("error"),
              ),
            ),
      ),
    ]);
  };
