namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module MapTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  type TypeExpr with
    static member FromJsonMap(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField "map" "map" (fun mapFields ->
        sum {
          let! (key, value) = mapFields |> JsonValue.AsPair
          let! keyType = key |> fromJsonRoot
          let! valueType = value |> fromJsonRoot
          return TypeExpr.Map(keyType, valueType)
        })

    static member ToJsonMap(rootToJson: TypeExpr -> JsonValue) : TypeExpr * TypeExpr -> JsonValue =
      fun (keyType, valueType) ->
        let keyJson = keyType |> rootToJson
        let valueJson = valueType |> rootToJson
        JsonValue.Array [| keyJson; valueJson |] |> Json.kind "map" "map"
