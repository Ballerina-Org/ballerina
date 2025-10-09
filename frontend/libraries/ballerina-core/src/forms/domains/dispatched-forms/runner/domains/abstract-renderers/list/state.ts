import { Map } from "immutable";
import {
  BasicUpdater,
  Bindings,
  CommonAbstractRendererForeignMutationsExpected,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
  DispatchCommonFormState,
  DispatchDelta,
  DispatchOnChange,
  ListType,
  PredicateValue,
  SimpleCallback,
  ValueCallbackWithOptionalFlags,
  ValueUnit,
  VoidCallbackWithOptionalFlags,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { Template } from "../../../../../../../template/state";
import { View } from "../../../../../../../template/state";
import {
  simpleUpdater,
  simpleUpdaterWithChildren,
} from "../../../../../../../fun/domains/updater/domains/simpleUpdater/state";
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
  customFormState: {
    applyToAll: boolean;
  };
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
      customFormState: {
        applyToAll: false,
      },
    }),
  },
  Updaters: {
    Core: {
      ...simpleUpdater<ListAbstractRendererState>()("commonFormState"),
      ...simpleUpdater<ListAbstractRendererState>()("elementFormStates"),
      ...simpleUpdaterWithChildren<ListAbstractRendererState>()({
        ...simpleUpdater<ListAbstractRendererState["customFormState"]>()(
          "applyToAll",
        ),
      })("customFormState"),
    },
    Template: {},
  },
};

export type ListAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<ValueTuple, Flags>;
};

export type ListAbstractRendererViewForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<ValueTuple, Flags>;
  add?: (
    flags?: Flags,
  ) => (customUpdater?: BasicUpdater<PredicateValue>) => void;
  remove?: ValueCallbackWithOptionalFlags<number, Flags>;
  move?: (elementIndex: number, to: number, flags: Flags | undefined) => void;
  duplicate?: ValueCallbackWithOptionalFlags<number, Flags>;
  insert?: ValueCallbackWithOptionalFlags<number, Flags>;
  setApplyToAll: SimpleCallback<boolean>;
  applyToAll: ValueCallbackWithOptionalFlags<DispatchDelta<Flags>, Flags>;
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
    embeddedPlaceholderElementTemplate: (
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
