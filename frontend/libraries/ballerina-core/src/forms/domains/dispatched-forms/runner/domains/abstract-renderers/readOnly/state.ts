import {
  CommonAbstractRendererForeignMutationsExpected,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
  DispatchParsedType,
  PredicateValue,
  ReadOnlyType,
} from "../../../../../../../../main";
import { Unit } from "../../../../../../../fun/domains/unit/state";
import { Template } from "../../../../../../../template/state";
import { View } from "../../../../../../../template/state";

export type ReadOnlyAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  CommonAbstractRendererReadonlyContext<
    ReadOnlyType<any>,
    PredicateValue,
    CustomPresentationContext,
    ExtraContext
  > &
    CommonAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext & {
      readOnlyType: ReadOnlyType<any>;
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