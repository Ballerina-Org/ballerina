namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Union =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json

  type TypeValue with
    static member FromJsonUnion
      (fromRootJson: JsonValue -> Sum<TypeValue, Errors>)
      : JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "union" "union" (fun unionFields ->
        sum {
          let! cases = unionFields |> JsonValue.AsArray

          let! caseTypes =
            cases
            |> Array.map (fun case ->
              sum {
                let! (caseKey, caseValue) = case |> JsonValue.AsPair
                let! caseType = fromRootJson caseValue
                let! caseKey = caseKey |> TypeSymbol.FromJson
                return (caseKey, caseType)
              })
            |> sum.All
            |> sum.Map Map.ofSeq

          return TypeValue.Union(caseTypes)
        })

    static member ToJsonUnion(rootToJson: TypeValue -> JsonValue) : Map<TypeSymbol, TypeValue> -> JsonValue =
      Map.toArray
      >> Array.map (fun (symbol, value) -> JsonValue.Array [| TypeSymbol.ToJson symbol; rootToJson value |])
      >> JsonValue.Array
      >> Json.kind "union" "union"
