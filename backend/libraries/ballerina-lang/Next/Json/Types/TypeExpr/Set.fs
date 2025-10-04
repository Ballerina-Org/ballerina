namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module SetTypeExpr =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "set"

  type TypeExpr with
    static member FromJsonSet(fromJsonRoot: TypeExprParser) : TypeExprParser =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fromJsonRoot >>= (TypeExpr.Set >> sum.Return))

    static member ToJsonSet(rootToJson: TypeExpr -> JsonValue) : TypeExpr -> JsonValue =
      rootToJson >> Json.discriminator discriminator
