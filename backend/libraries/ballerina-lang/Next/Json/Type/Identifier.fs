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

  let private kindKey = "id"
  let private fieldKey = "name"

  type Identifier with
    static member FromJson: JsonValue -> Sum<Identifier, Errors> =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun nameJson ->
        sum {
          let! name = nameJson |> JsonValue.AsString
          return name |> Identifier.LocalScope
        })

    static member ToJson: Identifier -> JsonValue =
      fun id ->
        match id with
        | Identifier.LocalScope name -> name |> JsonValue.String |> Json.kind kindKey fieldKey
        | Identifier.FullyQualified(scope, name) ->
          (name :: scope |> Seq.map JsonValue.String |> Seq.toArray)
          |> JsonValue.Array
          |> Json.kind kindKey fieldKey
