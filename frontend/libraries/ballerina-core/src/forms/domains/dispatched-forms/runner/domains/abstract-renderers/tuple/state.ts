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
} from "../../../../../../../../main";
import { View } from "../../../../../../../../main";
import { simpleUpdater } from "../../../../../../../../main";

export type TupleAbstractRendererReadonlyContext<CustomContext = Unit> =
  CommonAbstractRendererReadonlyContext<
    TupleType<any>,
    ValueTuple,
    CustomContext
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
  CustomContext = Unit,
  Flags = Unit,
> = View<
  TupleAbstractRendererReadonlyContext<CustomContext> &
    TupleAbstractRendererState,
  TupleAbstractRendererState,
  TupleAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    embeddedItemTemplates: (
      itemIndex: number,
    ) => (
      flags: Flags | undefined,
    ) => Template<
      TupleAbstractRendererReadonlyContext<CustomContext> &
        TupleAbstractRendererState,
      TupleAbstractRendererState,
      TupleAbstractRendererViewForeignMutationsExpected<Flags>
    >;
  }
>;
