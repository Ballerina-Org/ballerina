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
import { TableLoadWithRetries } from "./loadWithRetries";

export const InitialiseTable = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>(
  tableApiSource: DispatchTableApiSource,
  fromTableApiParser: (value: unknown) => ValueOrErrors<PredicateValue, string>,
) => {
  return Co<CustomPresentationContext, ExtraContext>().Seq([
    Co<CustomPresentationContext, ExtraContext>()
      .GetState()
      .then((current) => {
        const co = Co<CustomPresentationContext, ExtraContext>();
        type StateWithParentId = { current: typeof current; parentId?: string };

        const local = current.bindings.get("local");
        if (!local || !PredicateValue.Operations.IsRecord(local)) {
          return co.Return<StateWithParentId>({
            current,
            parentId: undefined,
          });
        }

        const parentId = local.fields.get("Id");
        if (!parentId || !PredicateValue.Operations.IsString(parentId)) {
          return co.Return<StateWithParentId>({
            current,
            parentId: undefined,
          });
        }

        return co.Return<StateWithParentId>({
          current,
          parentId,
        });
      })
      .then(({ current, parentId }) =>
        (current.value.data.size == 0 && current.value.hasMoreValues) ||
        current.customFormState.loadingState == "reload from 0"
          ? TableLoadWithRetries<CustomPresentationContext, ExtraContext>(
              tableApiSource,
              fromTableApiParser,
              parentId,
            )(3)().then((res) =>
              res.kind == "r"
                ? Co<CustomPresentationContext, ExtraContext>().Seq([
                    Co<CustomPresentationContext, ExtraContext>().Do(() => {
                      const updater = replaceWith<ValueTable>(
                        ValueTable.Default.fromParsed(
                          0,
                          res.value.to,
                          res.value.hasMoreValues,
                          res.value.data,
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
                      return Co<
                        CustomPresentationContext,
                        ExtraContext
                      >().Return(unit);
                    }),
                    Co<CustomPresentationContext, ExtraContext>().SetState(
                      TableAbstractRendererState.Updaters.Core.customFormState.children.loadingState(
                        replaceWith<
                          TableAbstractRendererState["customFormState"]["loadingState"]
                        >("loaded"),
                      ),
                    ),
                  ])
                : Co<CustomPresentationContext, ExtraContext>().SetState(
                    TableAbstractRendererState.Updaters.Core.customFormState.children.loadingState(
                      replaceWith<
                        TableAbstractRendererState["customFormState"]["loadingState"]
                      >("error"),
                    ),
                  ),
            )
          : Co<CustomPresentationContext, ExtraContext>().SetState(
              TableAbstractRendererState.Updaters.Core.customFormState.children.loadingState(
                replaceWith<
                  TableAbstractRendererState["customFormState"]["loadingState"]
                >("loaded"),
              ),
            ),
      ),
  ]);
};
