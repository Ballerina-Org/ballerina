namespace Ballerina.Data.Delta

module Model =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model

  type Delta =
    | Multiple of List<Delta>
    | Replace of Value<TypeValue>
    | Record of string * Delta
    | Union of string * Delta
    | Tuple of int * Delta
    | Sum of int * Delta
