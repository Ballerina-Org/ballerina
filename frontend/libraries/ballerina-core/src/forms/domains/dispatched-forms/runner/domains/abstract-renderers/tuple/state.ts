import { Map } from "immutable";
import {
  BasicUpdater,
  MapRepo,
  Template,
  Updater,
  ValueTuple,
  DispatchOnChange,
  Unit,
  CommonAbstractRendererReadonlyContext,
  TupleType,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";
import { View } from "../../../../../../../../main";
import { simpleUpdater } from "../../../../../../../../main";

export type TupleAbstractRendererReadonlyContext<
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  TupleType<any>,
  ValueTuple,
  CustomPresentationContext,
  ExtraContext
>;

export type TupleAbstractRendererState = CommonAbstractRendererState & {
  itemFormStates: Map<number, CommonAbstractRendererState>;
};

export const TupleAbstractRendererState = {
  Default: (
    itemFormStates: TupleAbstractRendererState["itemFormStates"],
  ): TupleAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
    itemFormStates: itemFormStates,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<TupleAbstractRendererState>()("commonFormState"),
      ...simpleUpdater<TupleAbstractRendererState>()("itemFormStates"),
    },
    Template: {
      upsertItemFormState: (
        itemIndex: number,
        defaultState: () => any,
        updater: BasicUpdater<CommonAbstractRendererState>,
      ): Updater<TupleAbstractRendererState> =>
        TupleAbstractRendererState.Updaters.Core.itemFormStates(
          MapRepo.Updaters.upsert(itemIndex, defaultState, updater),
        ),
    },
  },
};

export type TupleAbstractRendererForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueTuple, Flags>;
};

export type TupleAbstractRendererViewForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueTuple, Flags>;
};

export type TupleAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  TupleAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    TupleAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  TupleAbstractRendererState,
  TupleAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    embeddedItemTemplates: (
      itemIndex: number,
    ) => (
      flags: Flags | undefined,
    ) => Template<
      TupleAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      > &
        TupleAbstractRendererState,
      TupleAbstractRendererState,
      TupleAbstractRendererViewForeignMutationsExpected<Flags>
    >;
  }
>;
