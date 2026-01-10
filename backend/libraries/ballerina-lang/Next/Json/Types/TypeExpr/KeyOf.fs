namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module KeyOf =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "keyOf"
  open Ballerina.DSL.Next.Json.Keys

  type TypeExpr<'valueExt> with
    static member FromJsonKeyOf(fromJsonRoot: TypeExprParser<'valueExt>) : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fromJsonRoot >>= (TypeExpr.KeyOf >> sum.Return))

    static member ToJsonKeyOf(rootToJson: TypeExpr<'valueExt> -> JsonValue) : TypeExpr<'valueExt> -> JsonValue =
      rootToJson >> Json.discriminator discriminator
