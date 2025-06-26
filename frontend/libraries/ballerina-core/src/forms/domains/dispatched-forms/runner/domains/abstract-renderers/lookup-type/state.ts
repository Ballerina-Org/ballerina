import {
  CommonAbstractRendererForeignMutationsExpected,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  DispatchOnChange,
  DispatchParsedType,
  PredicateValue,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { Template } from "../../../../../../../template/state";
import { View } from "../../../../../../../template/state";

export type LookupTypeAbstractRendererReadonlyContext<
  CustomPresentationContext,
> = CommonAbstractRendererReadonlyContext<
  DispatchParsedType<any>,
  PredicateValue,
  CustomPresentationContext
>;

export type LookupTypeAbstractRendererState = Unit;

export type LookupTypeAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<PredicateValue, Flags>;
};

export type LookupTypeAbstractRendererViewForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<PredicateValue, Flags>;
};

export type LookupTypeAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
> = View<
  LookupTypeAbstractRendererReadonlyContext<CustomPresentationContext> &
    LookupTypeAbstractRendererState,
  LookupTypeAbstractRendererState,
  LookupTypeAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    embeddedTemplate: Template<
      CommonAbstractRendererReadonlyContext<
        DispatchParsedType<any>,
        PredicateValue,
        CustomPresentationContext
      > &
        CommonAbstractRendererState,
      CommonAbstractRendererState,
      CommonAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
