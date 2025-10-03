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

  type TypeExpr with
    static member FromJsonKeyOf(fromJsonRoot: TypeExprParser) : TypeExprParser =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fromJsonRoot >>= (TypeExpr.KeyOf >> sum.Return))

    static member ToJsonKeyOf(rootToJson: TypeExpr -> JsonValue) : TypeExpr -> JsonValue =
      rootToJson >> Json.discriminator discriminator
