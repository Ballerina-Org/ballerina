import { DispatchInjectablesTypes, Unit } from "../../../../../main";
import { LoadAndDeserializeSpecification } from "./coroutines/runner";

export const DispatchFormsParserTemplate = <
  T extends DispatchInjectablesTypes<T> = Unit,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
>() =>
  LoadAndDeserializeSpecification<
    T,
    Flags,
    CustomPresentationContexts,
    ExtraContext
  >();
