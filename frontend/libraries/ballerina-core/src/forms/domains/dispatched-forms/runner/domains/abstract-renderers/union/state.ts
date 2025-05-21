import {
  BasicFun,
  BasicUpdater,
  CommonAbstractRendererReadonlyContext,
  DispatchCommonFormState,
  MapRepo,
  simpleUpdater,
  Template,
  UnionType,
  Updater,
  ValueUnionCase,
  View,
  DispatchOnChange,
  DomNodeIdReadonlyContext,
} from "../../../../../../../../main";
import { Map } from "immutable";

export type UnionAbstractRendererReadonlyContext =
  CommonAbstractRendererReadonlyContext<UnionType<any>, ValueUnionCase>;

export type UnionAbstractRendererState<
  CaseFormState extends { commonFormState: DispatchCommonFormState },
> = {
  commonFormState: DispatchCommonFormState;
  caseFormStates: Map<string, CaseFormState>;
};

export const UnionAbstractRendererState = <
  CaseFormState extends { commonFormState: DispatchCommonFormState },
>() => ({
  Default: (
    caseFormStates: UnionAbstractRendererState<CaseFormState>["caseFormStates"],
  ): UnionAbstractRendererState<CaseFormState> => ({
    commonFormState: DispatchCommonFormState.Default(),
    caseFormStates,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<UnionAbstractRendererState<CaseFormState>>()(
        "commonFormState",
      ),
      ...simpleUpdater<UnionAbstractRendererState<CaseFormState>>()(
        "caseFormStates",
      ),
    },
    Template: {
      upsertCaseFormState: (
        caseName: string,
        defaultState: () => any,
        updater: BasicUpdater<CaseFormState>,
      ): Updater<UnionAbstractRendererState<CaseFormState>> =>
        UnionAbstractRendererState<CaseFormState>().Updaters.Core.caseFormStates(
          MapRepo.Updaters.upsert(caseName, defaultState, updater),
        ),
    },
  },
});
export type UnionAbstractRendererView<
  CaseFormState extends { commonFormState: DispatchCommonFormState },
  ForeignMutationsExpected,
> = View<
  UnionAbstractRendererReadonlyContext &
    UnionAbstractRendererState<CaseFormState> &
    DomNodeIdReadonlyContext,
  UnionAbstractRendererState<CaseFormState>,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueUnionCase>;
  },
  {
    embeddedCaseTemplate: BasicFun<
      string,
      Template<
        UnionAbstractRendererReadonlyContext &
          UnionAbstractRendererState<CaseFormState>,
        UnionAbstractRendererState<CaseFormState>,
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueUnionCase>;
        }
      >
    >;
  }
>;
