import {
  ValueRecord,
  DispatchOnChange,
  Guid,
  SimpleCallback,
  ValueCallbackWithOptionalFlags,
  Unit,
  CommonAbstractRendererReadonlyContext,
  MultiSelectionType,
} from "../../../../../../../../main";

import { View } from "../../../../../../../template/state";
import {
  DispatchBaseEnumContext,
  EnumAbstractRendererState,
} from "../enum/state";

export type EnumMultiselectAbstractRendererReadonlyContext<CustomContext> =
  CommonAbstractRendererReadonlyContext<
    MultiSelectionType<any>,
    ValueRecord,
    CustomContext
  > &
    DispatchBaseEnumContext;

export type EnumMultiselectAbstractRendererState = EnumAbstractRendererState;

export type EnumMultiselectAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<ValueRecord, Flags>;
  setNewValue: ValueCallbackWithOptionalFlags<Array<Guid>, Flags>;
  loadOptions: SimpleCallback<void>;
};

export type EnumMultiselectAbstractRendererView<
  CustomContext = Unit,
  Flags = Unit,
> = View<
  EnumMultiselectAbstractRendererReadonlyContext<CustomContext> &
    EnumMultiselectAbstractRendererState & {
      selectedIds: Array<Guid>;
      activeOptions: "unloaded" | "loading" | Array<ValueRecord>;
    },
  EnumMultiselectAbstractRendererState,
  EnumMultiselectAbstractRendererForeignMutationsExpected<Flags>
>;
