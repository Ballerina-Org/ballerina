namespace Ballerina.DSL.Next.Types.Json

open Ballerina.LocalizedErrors

[<AutoOpen>]
module TypeVar =

  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open FSharp.Data

  type TypeVar with
    static member FromJson(json: JsonValue) : Sum<TypeVar, Errors<unit>> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        let! name =
          fields
          |> (Map.tryFindWithError "name" "TypeVar" (fun () -> "name") ()
              >>= JsonValue.AsString)

        let! guid =
          fields
          |> (Map.tryFindWithError "guid" "TypeVar" (fun () -> "guid") ()
              >>= JsonValue.AsString)

        match Guid.TryParse(guid) with
        | true, parsedGuid ->
          return
            { Name = name
              Guid = parsedGuid
              Synthetic = false }
        | false, _ ->
          return! sum.Throw(Errors.Singleton () (fun () -> $"Error: Invalid GUID format '{guid}' in 'TypeVar'."))
      }

    static member ToJson(t: TypeVar) : JsonValue =
      JsonValue.Record
        [| "name", JsonValue.String t.Name
           "guid", JsonValue.String(t.Guid.ToString()) |]
