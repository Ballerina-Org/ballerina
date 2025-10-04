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

  type TypeExpr with
    static member FromJsonRotate(fromJsonRoot: TypeExprParser) : TypeExprParser =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fromJsonRoot >>= (TypeExpr.Rotate >> sum.Return))

    static member ToJsonRotate(rootToJson: TypeExpr -> JsonValue) : TypeExpr -> JsonValue =
      rootToJson >> Json.discriminator discriminator
