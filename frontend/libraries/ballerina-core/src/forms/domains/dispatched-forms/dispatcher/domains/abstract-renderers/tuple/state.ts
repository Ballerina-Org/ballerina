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

export type TupleAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  itemFormStates: Map<number, any>;
};

export const TupleAbstractRendererState = <
  ItemFormStates extends Map<number, any>,
>() => ({
  Default: (itemFormStates: ItemFormStates): TupleAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    itemFormStates: itemFormStates,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<TupleAbstractRendererState>()("itemFormStates"),
    },
    Template: {
      upsertItemFormState: (
        itemIndex: number,
        defaultState: () => any,
        updater: BasicUpdater<any>,
      ): Updater<TupleAbstractRendererState> =>
        TupleAbstractRendererState().Updaters.Core.itemFormStates(
          MapRepo.Updaters.upsert(itemIndex, defaultState, updater),
        ),
    },
  },
});
export type TupleAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context & Value<ValueTuple> & TupleAbstractRendererState,
  TupleAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueTuple>;
  },
  {
    embeddedItemTemplates: BasicFun<
      number,
      Template<
        Context &
          Value<ValueTuple> &
          TupleAbstractRendererState & {
            bindings: Bindings;
            extraContext: any;
          },
        TupleAbstractRendererState,
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueTuple>;
        }
      >
    >;
  }
>;
