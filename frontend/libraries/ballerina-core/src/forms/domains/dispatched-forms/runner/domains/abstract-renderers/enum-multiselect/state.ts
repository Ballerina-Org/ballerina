import { Value } from "../../../../../../../value/state";

import {
  ValueRecord,
  DispatchOnChange,
  FormLabel,
  Guid,
  SimpleCallback,
  DomNodeIdReadonlyContext,
  ValueCallbackWithOptionalFlags,
  Unit,
} from "../../../../../../../../main";

import { View } from "../../../../../../../template/state";
import {
  DispatchBaseEnumContext,
  EnumAbstractRendererState,
} from "../enum/state";

export type EnumMultiselectAbstractRendererView<
  Context extends FormLabel & DispatchBaseEnumContext,
  ForeignMutationsExpected,
  Flags = Unit,
> = View<
  Context &
    Value<ValueRecord> &
    DomNodeIdReadonlyContext &
    EnumAbstractRendererState & {
      selectedIds: Array<Guid>;
      activeOptions: "unloaded" | "loading" | Array<ValueRecord>;
      disabled: boolean;
    },
  EnumAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueRecord, Flags>;
    setNewValue: ValueCallbackWithOptionalFlags<Array<Guid>, Flags>;
    loadOptions: SimpleCallback<void>;
  }
>;
