namespace Ballerina.Data.Delta

module Model =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model

  type Delta<'valueExtension> =
    | Multiple of List<Delta<'valueExtension>>
    | Replace of Value<TypeValue, 'valueExtension>
    | Record of string * Delta<'valueExtension>
    | Union of string * Delta<'valueExtension>
    | Tuple of int * Delta<'valueExtension>
    | Sum of int * Delta<'valueExtension>
