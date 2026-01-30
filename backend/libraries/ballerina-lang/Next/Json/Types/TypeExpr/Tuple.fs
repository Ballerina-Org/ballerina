namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TupleTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "tuple"

  type TypeExpr<'valueExt> with
    static member FromJsonTuple<'ve when 've: comparison>
      (fromJsonRoot: TypeExprParser<'valueExt>)
      : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun tupleFields ->
        sum {
          let! elements = tupleFields |> JsonValue.AsArray
          let! elementTypes = elements |> Array.map (fun element -> element |> fromJsonRoot) |> sum.All
          return TypeExpr.Tuple(elementTypes)
        })

    static member ToJsonTuple<'ve when 've: comparison>
      (rootToJson: TypeExpr<'valueExt> -> JsonValue)
      : List<TypeExpr<'valueExt>> -> JsonValue =
      List.map rootToJson
      >> List.toArray
      >> JsonValue.Array
      >> Json.discriminator discriminator
