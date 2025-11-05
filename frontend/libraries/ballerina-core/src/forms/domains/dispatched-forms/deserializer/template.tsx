import { DispatchInjectablesTypes, Unit } from "../../../../../main";
import { LoadAndDeserializeSpecificationByDesiredLaunchers } from "./coroutines/runner";

export const DispatchFormsParserTemplate = <
  T extends DispatchInjectablesTypes<T> = Unit,
  Flags = Unit,
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  LoadAndDeserializeSpecificationByDesiredLaunchers<
    T,
    Flags,
    CustomPresentationContext,
    ExtraContext
  >();
