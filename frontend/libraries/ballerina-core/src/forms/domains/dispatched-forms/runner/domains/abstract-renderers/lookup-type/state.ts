import {
  CommonAbstractRendererForeignMutationsExpected,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
  DispatchParsedType,
  LookupType,
  PredicateValue,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { Template } from "../../../../../../../template/state";
import { View } from "../../../../../../../template/state";

export type LookupTypeAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  CommonAbstractRendererReadonlyContext<
    DispatchParsedType<any>,
    PredicateValue,
    CustomPresentationContext,
    ExtraContext
  > &
    CommonAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext<PredicateValue> & {
      lookupType: LookupType;
    },
  CommonAbstractRendererState,
  CommonAbstractRendererForeignMutationsExpected<Flags>,
  {
    embeddedTemplate: Template<
      CommonAbstractRendererReadonlyContext<
        DispatchParsedType<any>,
        PredicateValue,
        CustomPresentationContext,
        ExtraContext
      > &
        CommonAbstractRendererState,
      CommonAbstractRendererState,
      CommonAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
