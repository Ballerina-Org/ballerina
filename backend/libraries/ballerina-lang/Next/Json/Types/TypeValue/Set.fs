namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Set =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns

  type TypeValue with
    static member FromJsonSet(fromRootJson: JsonValue -> Sum<TypeValue, Errors>) : JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "set" "set" (fun elementType ->
        sum {
          let! elementType = elementType |> fromRootJson
          return TypeValue.Set elementType
        })

    static member ToJsonSet(toRootJson: TypeValue -> JsonValue) : TypeValue -> JsonValue =
      toRootJson >> Json.kind "set" "set"
