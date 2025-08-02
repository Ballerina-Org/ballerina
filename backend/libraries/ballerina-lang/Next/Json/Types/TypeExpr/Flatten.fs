namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Flatten =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  type TypeExpr with
    static member FromJsonFlatten(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField "flatten" "flatten" (fun flattenFields ->
        sum {
          let! (type1, type2) = flattenFields |> JsonValue.AsPair
          let! type1 = type1 |> fromJsonRoot
          let! type2 = type2 |> fromJsonRoot
          return TypeExpr.Flatten(type1, type2)
        })

    static member ToJsonFlatten(rootToJson: TypeExpr -> JsonValue) : TypeExpr * TypeExpr -> JsonValue =
      fun (type1, type2) ->
        let type1 = rootToJson type1
        let type2 = rootToJson type2
        JsonValue.Array [| type1; type2 |] |> Json.kind "flatten" "flatten"
