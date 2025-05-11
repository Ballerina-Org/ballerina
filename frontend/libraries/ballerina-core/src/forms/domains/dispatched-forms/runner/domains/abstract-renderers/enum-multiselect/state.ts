import { Value } from "../../../../../../../value/state";

import {
  ValueRecord,
  DispatchOnChange,
  FormLabel,
  Guid,
  SimpleCallback,
} from "../../../../../../../../main";

import { View } from "../../../../../../../template/state";
import {
  DispatchBaseEnumContext,
  EnumAbstractRendererState,
} from "../enum/state";

export type EnumMultiselectAbstractRendererView<
  Context extends FormLabel & DispatchBaseEnumContext,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<ValueRecord> &
    EnumAbstractRendererState & {
      selectedIds: Array<Guid>;
      activeOptions: "unloaded" | "loading" | Array<ValueRecord>;
      disabled: boolean;
    },
  EnumAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueRecord>;
    setNewValue: SimpleCallback<Array<Guid>>;
    loadOptions: SimpleCallback<void>;
  }
>;
