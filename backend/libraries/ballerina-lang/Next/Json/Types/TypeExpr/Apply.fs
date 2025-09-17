namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Types.Model

[<AutoOpen>]
module Apply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json

  let private kindKey = "apply"
  let private fieldKey = "apply"

  type TypeExpr with
    static member FromJsonApply(fromJsonRoot: JsonParser<TypeExpr>) : JsonParser<TypeExpr> =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun applyFields ->
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

        JsonValue.Array [| functionJson; argumentJson |] |> Json.kind kindKey fieldKey
