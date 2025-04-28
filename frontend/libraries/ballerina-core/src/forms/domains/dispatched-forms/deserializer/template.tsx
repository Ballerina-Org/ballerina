import { Unit } from "../../../../../main";
import { LoadAndDeserializeSpecification } from "./coroutines/runner";

export const DispatchFormsParserTemplate = <
  T extends { [key in keyof T]: { type: any; state: any } } = Unit,
>() => LoadAndDeserializeSpecification<T>();
