namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module List =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model

  type TypeValue with
    static member FromJsonList
      (fromRootJson: JsonValue -> Sum<TypeValue, Errors>)
      : JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "list" "list" (fun elementType ->
        sum {
          let! elementType = elementType |> fromRootJson
          return TypeValue.List elementType
        })


    static member ToJsonList(toRootJson: TypeValue -> JsonValue) : TypeValue -> JsonValue =
      toRootJson >> Json.kind "list" "list"
