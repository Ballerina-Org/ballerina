namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeVar =

  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open FSharp.Data

  type TypeVar with
    static member FromJson(json: JsonValue) : Sum<TypeVar, Errors> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        let! name = fields |> (Map.tryFindWithError "name" "TypeVar" "name" >>= JsonValue.AsString)

        let! guid = fields |> (Map.tryFindWithError "guid" "TypeVar" "guid" >>= JsonValue.AsString)

        match Guid.TryParse(guid) with
        | true, parsedGuid -> return { Name = name; Guid = parsedGuid }
        | false, _ ->
          return!
            $"Error: Invalid GUID format '{guid}' in 'TypeVar'."
            |> Errors.Singleton
            |> sum.Throw
      }

    static member ToJson(t: TypeVar) : JsonValue =
      JsonValue.Record
        [| "name", JsonValue.String t.Name
           "guid", JsonValue.String(t.Guid.ToString()) |]
