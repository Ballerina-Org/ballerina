import {
  ValueRecord,
  DispatchOnChange,
  Guid,
  SimpleCallback,
  ValueCallbackWithOptionalFlags,
  Unit,
  CommonAbstractRendererReadonlyContext,
  MultiSelectionType,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";

import { View } from "../../../../../../../template/state";
import {
  DispatchBaseEnumContext,
  EnumAbstractRendererState,
} from "../enum/state";

export type EnumMultiselectAbstractRendererReadonlyContext<
  CustomPresentationContext,
  ExtraContext,
> = CommonAbstractRendererReadonlyContext<
  MultiSelectionType<any>,
  ValueRecord,
  CustomPresentationContext,
  ExtraContext
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
  ExtraContext = Unit,
> = View<
  EnumMultiselectAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    EnumMultiselectAbstractRendererState & {
      selectedIds: Array<Guid>;
      activeOptions: "unloaded" | "loading" | Array<ValueRecord>;
    } & CommonAbstractRendererViewOnlyReadonlyContext,
  EnumMultiselectAbstractRendererState,
  EnumMultiselectAbstractRendererForeignMutationsExpected<Flags>
>;
