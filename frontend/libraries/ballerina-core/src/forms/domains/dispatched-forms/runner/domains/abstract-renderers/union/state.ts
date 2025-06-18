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
} from "../../../../../../../../main";
import { Map } from "immutable";

export type UnionAbstractRendererReadonlyContext<CustomContext = Unit> =
  CommonAbstractRendererReadonlyContext<
    UnionType<any>,
    ValueUnionCase,
    CustomContext
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
  CustomContext = Unit,
  Flags = Unit,
> = View<
  UnionAbstractRendererReadonlyContext<CustomContext> &
    UnionAbstractRendererState,
  UnionAbstractRendererState,
  UnionAbstractRendererForeignMutationsExpected<Flags>,
  {
    embeddedCaseTemplate: (
      caseName: string,
    ) => (
      flags: Flags | undefined,
    ) => Template<
      UnionAbstractRendererReadonlyContext<CustomContext> &
        UnionAbstractRendererState,
      UnionAbstractRendererState,
      UnionAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
