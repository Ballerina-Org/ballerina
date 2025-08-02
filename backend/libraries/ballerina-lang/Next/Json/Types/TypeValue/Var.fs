namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Var =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json

  type TypeValue with
    static member FromJsonVar: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "var" "var" (TypeVar.FromJson >> sum.Map TypeValue.Var)

    static member ToJsonVar: TypeVar -> JsonValue = TypeVar.ToJson >> Json.kind "var" "var"
