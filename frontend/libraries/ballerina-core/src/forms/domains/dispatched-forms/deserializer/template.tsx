import { DispatchInjectablesTypes, Unit } from "../../../../../main";
import { LoadAndDeserializeSpecification } from "./coroutines/runner";

export const DispatchFormsParserTemplate = <
  T extends DispatchInjectablesTypes<T> = Unit,
  Flags = Unit,
  CustomContexts = Unit,
>() => LoadAndDeserializeSpecification<T, Flags, CustomContexts>();
