namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module FromTypeValueTypeExpr =
  open FSharp.Data
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.Errors

  let private discriminator = "fromTypeValue"

  type TypeExpr<'valueExt> with
    static member FromJsonFromTypeValue
      (typeValueFromJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors<_>>)
      : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue
        discriminator
        (typeValueFromJson >> sum.Map TypeExpr.FromTypeValue)

    static member ToJsonFromTypeValue
      (typeValueToJson: TypeValue<'valueExt> -> JsonValue)
      : TypeValue<'valueExt> -> JsonValue =
      typeValueToJson >> Json.discriminator discriminator
