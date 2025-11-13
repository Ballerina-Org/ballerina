namespace Ballerina.Data.Delta

[<AutoOpen>]
module Model =
  open System
  open Ballerina.StdLib.String
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model

  type Delta<'valueExtension, 'deltaExtension> =
    | Multiple of List<Delta<'valueExtension, 'deltaExtension>>
    | Replace of Value<TypeValue, 'valueExtension>
    | Record of string * Delta<'valueExtension, 'deltaExtension>
    | Union of string * Delta<'valueExtension, 'deltaExtension>
    | Tuple of int * Delta<'valueExtension, 'deltaExtension>
    | Sum of int * Delta<'valueExtension, 'deltaExtension>
    | Ext of 'deltaExtension

    override this.ToString() =
      match this with
      | Multiple deltas -> let deltas = String.join ", " (deltas |> List.map string) in $"[ {deltas}]"
      | Replace v -> v.ToString()
      | Record(fieldName, fieldDelta) -> $"({fieldName}: {fieldDelta})"
      | Union(caseName, caseDelta) -> $"({caseName} of {caseDelta})"
      | Tuple(index, indexDelta) -> $"({index}, {indexDelta})"
      | Sum(index, indexDelta) -> $"({index} of {indexDelta})"
      | Ext ext -> ext.ToString()
