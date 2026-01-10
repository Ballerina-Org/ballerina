namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Exclude =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "exclude"

  type TypeExpr<'valueExt> with
    static member FromJsonExclude(fromJsonRoot: TypeExprParser<'valueExt>) : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun excludeFields ->
        sum {
          let! (type1, type2) = excludeFields |> JsonValue.AsPair
          let! type1 = type1 |> fromJsonRoot
          let! type2 = type2 |> fromJsonRoot
          return TypeExpr.Exclude(type1, type2)
        })

    static member ToJsonExclude
      (rootToJson: TypeExpr<'valueExt> -> JsonValue)
      : TypeExpr<'valueExt> * TypeExpr<'valueExt> -> JsonValue =
      fun (type1, type2) ->
        let type1 = rootToJson type1
        let type2 = rootToJson type2
        JsonValue.Array [| type1; type2 |] |> Json.discriminator discriminator
