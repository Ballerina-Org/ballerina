namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Flatten =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "flatten"

  type TypeExpr<'valueExt> with
    static member FromJsonFlatten(fromJsonRoot: TypeExprParser<'valueExt>) : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun flattenFields ->
        sum {
          let! (type1, type2) = flattenFields |> JsonValue.AsPair
          let! type1 = type1 |> fromJsonRoot
          let! type2 = type2 |> fromJsonRoot
          return TypeExpr.Flatten(type1, type2)
        })

    static member ToJsonFlatten
      (rootToJson: TypeExpr<'valueExt> -> JsonValue)
      : TypeExpr<'valueExt> * TypeExpr<'valueExt> -> JsonValue =
      fun (type1, type2) ->
        let type1 = rootToJson type1
        let type2 = rootToJson type2
        JsonValue.Array [| type1; type2 |] |> Json.discriminator discriminator
