namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module SetTypeExpr =
  open FSharp.Data
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "set"

  type TypeExpr<'valueExt> with
    static member FromJsonSet(fromJsonRoot: TypeExprParser<'valueExt>) : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fromJsonRoot >>= (TypeExpr.Set >> sum.Return))

    static member ToJsonSet(rootToJson: TypeExpr<'valueExt> -> JsonValue) : TypeExpr<'valueExt> -> JsonValue =
      rootToJson >> Json.discriminator discriminator
