import { Map } from "immutable";
import {
  Bindings,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
  DispatchCommonFormState,
  DispatchOnChange,
  ListType,
  ValueCallbackWithOptionalFlags,
  ValueUnit,
  VoidCallbackWithOptionalFlags,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { Template } from "../../../../../../../template/state";
import { View } from "../../../../../../../template/state";
import { simpleUpdater } from "../../../../../../../fun/domains/updater/domains/simpleUpdater/state";
import { ValueTuple } from "../../../../../../../../main";

export type ListAbstractRendererReadonlyContext<
  CustomPresentationContext,
  ExtraContext,
> = CommonAbstractRendererReadonlyContext<
  ListType<any>,
  ValueTuple | ValueUnit,
  CustomPresentationContext,
  ExtraContext
>;

export type ListAbstractRendererState = CommonAbstractRendererState & {
  elementFormStates: Map<number, CommonAbstractRendererState>;
};

export const ListAbstractRendererState = {
  Default: {
    zero: () => ({
      ...CommonAbstractRendererState.Default(),
      elementFormStates: Map<number, CommonAbstractRendererState>(),
    }),
    elementFormStates: (
      elementFormStates: Map<number, CommonAbstractRendererState>,
    ): ListAbstractRendererState => ({
      ...CommonAbstractRendererState.Default(),
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

export type ListAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<ValueTuple, Flags>;
};

export type ListAbstractRendererViewForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<ValueTuple, Flags>;
  add?: VoidCallbackWithOptionalFlags<Flags>;
  remove?: ValueCallbackWithOptionalFlags<number, Flags>;
  move?: (elementIndex: number, to: number, flags: Flags | undefined) => void;
  duplicate?: ValueCallbackWithOptionalFlags<number, Flags>;
  insert?: ValueCallbackWithOptionalFlags<number, Flags>;
};

export type ListAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  ListAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext> &
    ListAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  ListAbstractRendererState,
  ListAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    embeddedElementTemplate: (
      elementIndex: number,
    ) => (
      flags: Flags | undefined,
    ) => Template<
      ListAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      > &
        ListAbstractRendererState,
      ListAbstractRendererState,
      ListAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
