namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module SumTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "sum"

  type TypeExpr<'valueExt> with
    static member FromJsonSum(fromJsonRoot: TypeExprParser<'valueExt>) : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun sumFields ->
        sum {
          let! sumFields = sumFields |> JsonValue.AsArray
          let! caseTypes = sumFields |> Array.map fromJsonRoot |> sum.All
          return TypeExpr.Sum(caseTypes)
        })

    static member ToJsonSum(rootToJson: TypeExpr<'valueExt> -> JsonValue) : List<TypeExpr<'valueExt>> -> JsonValue =
      List.map rootToJson
      >> List.toArray
      >> JsonValue.Array
      >> Json.discriminator discriminator
