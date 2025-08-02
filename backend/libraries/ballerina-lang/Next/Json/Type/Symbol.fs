namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeSymbolJson =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open FSharp.Data

  type TypeSymbol with
    static member FromJson(json: JsonValue) : Sum<TypeSymbol, Errors> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        let! name =
          fields
          |> (Map.tryFindWithError "name" "TypeSymbol" "name" >>= JsonValue.AsString)

        let! guid =
          fields
          |> (Map.tryFindWithError "guid" "TypeSymbol" "guid" >>= JsonValue.AsString)

        match Guid.TryParse(guid) with
        | true, parsedGuid -> return { Name = name; Guid = parsedGuid }
        | false, _ ->
          return!
            $"Error: Invalid GUID format '{guid}' in 'TypeSymbol'."
            |> Errors.Singleton
            |> sum.Throw
      }

    static member ToJson(ts: TypeSymbol) : JsonValue =
      JsonValue.Record
        [| "name", JsonValue.String ts.Name
           "guid", JsonValue.String(ts.Guid.ToString()) |]
