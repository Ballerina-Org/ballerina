namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module SumTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns

  let private kindKey = "sum"
  let private fieldKey = "sum"

  type TypeExpr with
    static member FromJsonSum(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun sumFields ->
        sum {
          let! sumFields = sumFields |> JsonValue.AsArray
          let! caseTypes = sumFields |> Array.map fromJsonRoot |> sum.All
          return TypeExpr.Sum(caseTypes)
        })

    static member ToJsonSum(rootToJson: TypeExpr -> JsonValue) : List<TypeExpr> -> JsonValue =
      List.map rootToJson
      >> List.toArray
      >> JsonValue.Array
      >> Json.kind kindKey fieldKey
