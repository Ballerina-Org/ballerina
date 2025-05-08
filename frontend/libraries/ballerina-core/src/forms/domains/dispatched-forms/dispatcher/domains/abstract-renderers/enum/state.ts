import { Value } from "../../../../../../../value/state";
import {
  PredicateValue,
  SimpleCallback,
  ValueOption,
  DispatchCommonFormState,
} from "../../../../../../../../main";
import { View } from "../../../../../../../template/state";
import { FormLabel } from "../../../../../../../../main";
import { Synchronized } from "../../../../../../../async/domains/synchronized/state";
import { Unit, unit } from "../../../../../../../fun/domains/unit/state";
import { ValueRecord } from "../../../../../../../../main";
import { OrderedMap } from "immutable";
import { Guid } from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";

export type DispatchBaseEnumContext = {
  getOptions: () => Promise<OrderedMap<Guid, ValueRecord>>;
};
export type EnumAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: {
    options: Synchronized<Unit, OrderedMap<Guid, ValueRecord>>;
    shouldLoad: boolean;
  };
};
export const EnumAbstractRendererState = () => ({
  Default: (): EnumAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {
      options: Synchronized.Default(unit),
      shouldLoad: false,
    },
  }),
});

export type EnumAbstractRendererView<
  Context extends FormLabel & DispatchBaseEnumContext,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<ValueOption> &
    EnumAbstractRendererState & {
      activeOptions: "unloaded" | "loading" | Array<ValueRecord>;
    } & { disabled: boolean },
  EnumAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueOption>;
    setNewValue: SimpleCallback<Guid>;
    loadOptions: SimpleCallback<void>;
  }
>;
