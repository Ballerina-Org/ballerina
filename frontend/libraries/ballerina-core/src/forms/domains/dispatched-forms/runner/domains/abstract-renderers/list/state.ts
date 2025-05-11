import { Map } from "immutable";
import {
  BasicFun,
  Bindings,
  PredicateValue,
  SimpleCallback,
  DispatchCommonFormState,
  DispatchOnChange,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { Template } from "../../../../../../../template/state";
import { Value } from "../../../../../../../value/state";
import { View } from "../../../../../../../template/state";
import { FormLabel } from "../../../../../singleton/domains/form-label/state";
import { simpleUpdater } from "../../../../../../../fun/domains/updater/domains/simpleUpdater/state";
import { ValueTuple } from "../../../../../../../../main";

export type ListAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  elementFormStates: Map<number, any>;
};
export const ListAbstractRendererState = {
  Default: {
    zero: () => ({
      commonFormState: DispatchCommonFormState.Default(),
      elementFormStates: Map(),
    }),
    elementFormStates: (
      elementFormStates: Map<number, any>,
    ): ListAbstractRendererState => ({
      commonFormState: DispatchCommonFormState.Default(),
      elementFormStates,
    }),
  },
  Updaters: {
    Core: {
      ...simpleUpdater<ListAbstractRendererState>()("commonFormState"),
      ...simpleUpdater<ListAbstractRendererState>()("elementFormStates"),
    },
    Template: {},
  },
};
export type ListAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context & Value<ValueTuple> & ListAbstractRendererState,
  ListAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueTuple>;
    add: SimpleCallback<Unit>;
    remove: SimpleCallback<number>;
    move: (elementIndex: number, to: number) => void;
    duplicate: SimpleCallback<number>;
    insert: SimpleCallback<number>;
  },
  {
    embeddedElementTemplate: BasicFun<
      number,
      Template<
        Context &
          Value<ValueTuple> &
          ListAbstractRendererState & { bindings: Bindings; extraContext: any },
        ListAbstractRendererState,
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueTuple>;
        }
      >
    >;
  }
>;
