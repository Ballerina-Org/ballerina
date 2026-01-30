namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeSymbolJson =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open FSharp.Data

  type TypeSymbol with
    static member FromJson(json: JsonValue) : Sum<TypeSymbol, Errors<unit>> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        let! name =
          fields
          |> (Map.tryFindWithError "name" "TypeSymbol" (fun () -> "name") ()
              >>= JsonValue.AsString)


        let! guid =
          fields
          |> (Map.tryFindWithError "guid" "TypeSymbol" (fun () -> "guid") ()
              >>= JsonValue.AsString)

        match Guid.TryParse(guid) with
        | true, parsedGuid ->
          return
            { Name = name |> Identifier.LocalScope
              Guid = parsedGuid }
        | false, _ ->
          return! sum.Throw(Errors.Singleton () (fun () -> $"Error: Invalid GUID format '{guid}' in 'TypeSymbol'."))
      }

    static member ToJson(ts: TypeSymbol) : JsonValue =
      JsonValue.Record
        [| "name", JsonValue.String(ts.Name.ToString())
           "guid", JsonValue.String(ts.Guid.ToString()) |]
