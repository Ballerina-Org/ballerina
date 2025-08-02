namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeIdentifier =
  open Ballerina.StdLib.Json.Sum
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json
  open FSharp.Data

  type TypeIdentifier with
    static member FromJson: JsonValue -> Sum<TypeIdentifier, Errors> =
      sum.AssertKindAndContinueWithField "type-id" "name" (fun nameJson ->
        sum {
          let! name = nameJson |> JsonValue.AsString
          return name |> TypeIdentifier.Create
        })

    static member ToJson: TypeIdentifier -> JsonValue = fun ti -> ti.Name |> JsonValue.String //|> Json.kind "type-id" "name"
