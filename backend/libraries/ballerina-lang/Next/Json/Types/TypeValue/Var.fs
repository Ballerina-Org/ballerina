namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Var =
  open FSharp.Data
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "var"

  type TypeValue<'valueExt> with
    static member FromJsonVar: JsonValue -> Sum<TypeValue<'valueExt>, Errors<_>> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (TypeVar.FromJson >> sum.Map TypeValue.Var)

    static member ToJsonVar: TypeVar -> JsonValue =
      TypeVar.ToJson >> Json.discriminator discriminator
