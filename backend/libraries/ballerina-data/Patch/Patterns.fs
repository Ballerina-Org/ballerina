namespace Ballerina.Data.Patch

module Patterns =
  open Ballerina.Data.Patch.Model
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type Patch<'valueExtension, 'deltaExtension> with
    static member AsStructural(patch: Patch<'valueExtension, 'deltaExtension>) =
      match patch with
      | Structural delta -> delta |> sum.Return
      | Relation _ -> sum.Throw(Errors.Singleton "Expected a structural delta but got relation")

    static member AsRelation(patch: Patch<'valueExtension, 'deltaExtension>) =
      match patch with
      | Relation r -> r |> sum.Return
      | Structural _ -> sum.Throw(Errors.Singleton "Expected a relation but got structural delta")
