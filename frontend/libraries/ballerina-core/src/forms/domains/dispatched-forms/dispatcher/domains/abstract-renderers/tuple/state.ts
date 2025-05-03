import { Map } from "immutable";
import {
  BasicFun,
  BasicUpdater,
  MapRepo,
  Bindings,
  Template,
  Updater,
  Value,
  ValueTuple,
  DispatchCommonFormState,
} from "../../../../../../../../main";
import { FormLabel, View } from "../../../../../../../../main";
import { simpleUpdater } from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";

export type TupleAbstractRendererState<
  ItemFormState extends { commonFormState: DispatchCommonFormState },
> = {
  commonFormState: DispatchCommonFormState;
  itemFormStates: Map<number, ItemFormState>;
};

export const TupleAbstractRendererState = <
  ItemFormState extends { commonFormState: DispatchCommonFormState },
>() => ({
  Default: (
    itemFormStates: TupleAbstractRendererState<ItemFormState>["itemFormStates"],
  ): TupleAbstractRendererState<ItemFormState> => ({
    commonFormState: DispatchCommonFormState.Default(),
    itemFormStates: itemFormStates,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<TupleAbstractRendererState<ItemFormState>>()(
        "commonFormState",
      ),
      ...simpleUpdater<TupleAbstractRendererState<ItemFormState>>()(
        "itemFormStates",
      ),
    },
    Template: {
      upsertItemFormState: (
        itemIndex: number,
        defaultState: () => any,
        updater: BasicUpdater<ItemFormState>,
      ): Updater<TupleAbstractRendererState<ItemFormState>> =>
        TupleAbstractRendererState<ItemFormState>().Updaters.Core.itemFormStates(
          MapRepo.Updaters.upsert(itemIndex, defaultState, updater),
        ),
    },
  },
});
export type TupleAbstractRendererView<
  ItemFormState extends { commonFormState: DispatchCommonFormState },
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context & Value<ValueTuple> & TupleAbstractRendererState<ItemFormState>,
  TupleAbstractRendererState<ItemFormState>,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueTuple>;
  },
  {
    embeddedItemTemplates: BasicFun<
      number,
      Template<
        Context &
          Value<ValueTuple> &
          TupleAbstractRendererState<ItemFormState> & {
            bindings: Bindings;
            extraContext: any;
          },
        TupleAbstractRendererState<ItemFormState>,
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueTuple>;
        }
      >
    >;
  }
>;
