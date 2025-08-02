namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Apply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  type TypeExpr with
    static member FromJsonApply(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField "apply" "apply" (fun applyFields ->
        sum {
          let! (functionField, argumentField) = applyFields |> JsonValue.AsPair

          let! functionType = functionField |> fromJsonRoot
          let! argumentType = argumentField |> fromJsonRoot

          return TypeExpr.Apply(functionType, argumentType)
        })

    static member ToJsonApply(rootToJson: TypeExpr -> JsonValue) : TypeExpr * TypeExpr -> JsonValue =
      fun (functionType, argumentType) ->
        let functionJson = functionType |> rootToJson
        let argumentJson = argumentType |> rootToJson

        JsonValue.Array [| functionJson; argumentJson |] |> Json.kind "apply" "apply"
