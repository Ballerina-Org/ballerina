import {
  SimpleCallback,
  ValueOption,
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  CommonAbstractRendererReadonlyContext,
  SingleSelectionType,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
  ValueUnit,
} from "../../../../../../../../main";
import { View } from "../../../../../../../template/state";
import { Synchronized } from "../../../../../../../async/domains/synchronized/state";
import { Unit, unit } from "../../../../../../../fun/domains/unit/state";
import { ValueRecord } from "../../../../../../../../main";
import { OrderedMap } from "immutable";
import { Guid } from "../../../../../../../../main";

export type DispatchBaseEnumContext = {
  getOptions: () => Promise<OrderedMap<Guid, ValueRecord>>;
};

export type EnumAbstractRendererReadonlyContext<
  CustomPresentationContext,
  ExtraContext,
> = CommonAbstractRendererReadonlyContext<
  SingleSelectionType<any>,
  ValueOption | ValueUnit,
  CustomPresentationContext,
  ExtraContext
> &
  DispatchBaseEnumContext;

export type EnumAbstractRendererState = CommonAbstractRendererState & {
  customFormState: {
    options: Synchronized<Unit, OrderedMap<Guid, ValueRecord>>;
    shouldLoad: boolean;
  };
};
export const EnumAbstractRendererState = () => ({
  Default: (): EnumAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
    customFormState: {
      options: Synchronized.Default(unit),
      shouldLoad: false,
    },
  }),
});

export type EnumAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<ValueOption, Flags>;
  setNewValue: ValueCallbackWithOptionalFlags<Guid, Flags>;
  loadOptions: SimpleCallback<void>;
};

export type EnumAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  EnumAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext> &
    EnumAbstractRendererState & {
      activeOptions: "unloaded" | "loading" | Array<ValueRecord>;
    } & CommonAbstractRendererViewOnlyReadonlyContext,
  EnumAbstractRendererState,
  EnumAbstractRendererForeignMutationsExpected<Flags>
>;
