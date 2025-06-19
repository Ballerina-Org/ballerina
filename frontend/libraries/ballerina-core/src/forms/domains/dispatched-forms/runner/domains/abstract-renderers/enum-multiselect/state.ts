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

export type EnumMultiselectAbstractRendererReadonlyContext<
  CustomPresentationContext,
> = CommonAbstractRendererReadonlyContext<
  MultiSelectionType<any>,
  ValueRecord,
  CustomPresentationContext
> &
  DispatchBaseEnumContext;

export type EnumMultiselectAbstractRendererState = EnumAbstractRendererState;

export type EnumMultiselectAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<ValueRecord, Flags>;
  setNewValue: ValueCallbackWithOptionalFlags<Array<Guid>, Flags>;
  loadOptions: SimpleCallback<void>;
};

export type EnumMultiselectAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
> = View<
  EnumMultiselectAbstractRendererReadonlyContext<CustomPresentationContext> &
    EnumMultiselectAbstractRendererState & {
      selectedIds: Array<Guid>;
      activeOptions: "unloaded" | "loading" | Array<ValueRecord>;
    },
  EnumMultiselectAbstractRendererState,
  EnumMultiselectAbstractRendererForeignMutationsExpected<Flags>
>;
