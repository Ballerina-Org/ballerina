namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Var =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json

  let private kindKey = "var"
  let private fieldKey = "var"

  type TypeValue with
    static member FromJsonVar: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField kindKey fieldKey (TypeVar.FromJson >> sum.Map TypeValue.Var)

    static member ToJsonVar: TypeVar -> JsonValue = TypeVar.ToJson >> Json.kind kindKey fieldKey
