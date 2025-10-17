import {
  BaseFlags,
  DispatchDelta,
  DispatchParsedType,
  DispatchTableApiSource,
  Option,
  PredicateValue,
  TableAbstractRendererState,
  unit,
  Unit,
  ValueOrErrors,
  ValueTable,
} from "../../../../../../../../../main";
import { replaceWith } from "../../../../../../../../fun/domains/updater/domains/replaceWith/state";
import { InfiniteLoaderCo as Co } from "./builder";
import { Map } from "immutable";
import { TableLoadWithRetries } from "./loadWithRetries";

export const TableInfiniteLoader = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>(
  tableApiSource: DispatchTableApiSource,
  fromTableApiParser: (value: unknown) => ValueOrErrors<PredicateValue, string>,
) => {
  return Co<CustomPresentationContext, ExtraContext>().Seq([
    Co<CustomPresentationContext, ExtraContext>().SetState(
      TableAbstractRendererState.Updaters.Core.customFormState.children.loadMore(
        replaceWith<TableAbstractRendererState["customFormState"]["loadMore"]>(
          "loading more",
        ),
      ),
    ),
    Co<CustomPresentationContext, ExtraContext>()
      .GetState()
      .then((current) =>
        TableLoadWithRetries<CustomPresentationContext, ExtraContext>(
          tableApiSource,
          fromTableApiParser,
        )(3)().then((res) =>
          res.kind == "r"
            ? Co<CustomPresentationContext, ExtraContext>().Seq([
                Co<CustomPresentationContext, ExtraContext>().Do(() => {
                  const updater = replaceWith<ValueTable>(
                    ValueTable.Default.fromParsed(
                      0,
                      res.value.to,
                      res.value.hasMoreValues,
                      current.value.data.concat(res.value.data),
                    ),
                  );
                  // Only needed as a delta is mandatory for onchange but it is ignored upstream
                  const delta: DispatchDelta<BaseFlags> = {
                    kind: "UnitReplace",
                    replace: PredicateValue.Default.unit(),
                    state: {},
                    flags: {
                      kind: "localOnly",
                    },
                    type: DispatchParsedType.Default.primitive("unit"),
                    sourceAncestorLookupTypeNames:
                      current.lookupTypeAncestorNames,
                  };
                  current.onChange(Option.Default.some(updater), delta);
                }),
                Co<CustomPresentationContext, ExtraContext>().SetState(
                  TableAbstractRendererState.Updaters.Core.customFormState.children.loadMore(
                    replaceWith<
                      TableAbstractRendererState["customFormState"]["loadMore"]
                    >("don't load more"),
                  ),
                ),
              ])
            : Co<CustomPresentationContext, ExtraContext>().SetState(
                TableAbstractRendererState.Updaters.Core.customFormState.children.loadMore(
                  replaceWith<
                    TableAbstractRendererState["customFormState"]["loadMore"]
                  >("error loading more"),
                ),
              ),
        ),
      ),
  ]);
};
