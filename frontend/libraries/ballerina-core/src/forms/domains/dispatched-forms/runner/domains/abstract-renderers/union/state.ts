import {
  BasicUpdater,
  CommonAbstractRendererReadonlyContext,
  MapRepo,
  simpleUpdater,
  Template,
  UnionType,
  Updater,
  ValueUnionCase,
  View,
  DispatchOnChange,
  Unit,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";
import { Map } from "immutable";

export type UnionAbstractRendererReadonlyContext<
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  UnionType<any>,
  ValueUnionCase,
  CustomPresentationContext,
  ExtraContext
>;

export type UnionAbstractRendererState = CommonAbstractRendererState & {
  caseFormStates: Map<string, CommonAbstractRendererState>;
};

export const UnionAbstractRendererState = {
  Default: (
    caseFormStates: UnionAbstractRendererState["caseFormStates"],
  ): UnionAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
    caseFormStates,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<UnionAbstractRendererState>()("commonFormState"),
      ...simpleUpdater<UnionAbstractRendererState>()("caseFormStates"),
    },
    Template: {
      upsertCaseFormState: (
        caseName: string,
        defaultState: () => any,
        updater: BasicUpdater<CommonAbstractRendererState>,
      ): Updater<UnionAbstractRendererState> =>
        UnionAbstractRendererState.Updaters.Core.caseFormStates(
          MapRepo.Updaters.upsert(caseName, defaultState, updater),
        ),
    },
  },
};

export type UnionAbstractRendererForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueUnionCase, Flags>;
};

export type UnionAbstractRendererViewForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueUnionCase, Flags>;
};

export type UnionAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  UnionAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    UnionAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  UnionAbstractRendererState,
  UnionAbstractRendererForeignMutationsExpected<Flags>,
  {
    embeddedCaseTemplate: (
      caseName: string,
    ) => (
      flags: Flags | undefined,
    ) => Template<
      UnionAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      > &
        UnionAbstractRendererState,
      UnionAbstractRendererState,
      UnionAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
