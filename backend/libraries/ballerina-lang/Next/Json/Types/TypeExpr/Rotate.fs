namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Rotate =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "rotate"

  type TypeExpr<'valueExt> with
    static member FromJsonRotate(fromJsonRoot: TypeExprParser<'valueExt>) : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fromJsonRoot >>= (TypeExpr.Rotate >> sum.Return))

    static member ToJsonRotate(rootToJson: TypeExpr<'valueExt> -> JsonValue) : TypeExpr<'valueExt> -> JsonValue =
      rootToJson >> Json.discriminator discriminator
