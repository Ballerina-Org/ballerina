import {
  CommonAbstractRendererForeignMutationsExpected,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
  DispatchParsedType,
  PredicateValue,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { Template } from "../../../../../../../template/state";
import { View } from "../../../../../../../template/state";

export type LookupTypeAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
> = View<
  CommonAbstractRendererReadonlyContext<
    DispatchParsedType<any>,
    PredicateValue,
    CustomPresentationContext
  > &
    CommonAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererForeignMutationsExpected<Flags>,
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
