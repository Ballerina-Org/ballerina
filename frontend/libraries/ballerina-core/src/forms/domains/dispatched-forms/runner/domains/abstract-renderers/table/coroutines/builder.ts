import { CoTypedFactory } from "../../../../../../../../../main";
import {
  TableAbstractRendererReadonlyContext,
  TableAbstractRendererState,
  Unit,
} from "../../../../../../../../../main";

export const Co = <CustomPresentationContext = Unit, ExtraContext = Unit>() =>
  CoTypedFactory<
    TableAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    >,
    TableAbstractRendererState
  >();
