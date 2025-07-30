namespace Ballerina.StdLib.Json

module Patterns =
  open FSharp.Data
  open Ballerina.StdLib.Object
  open Ballerina.StdLib.String
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type JsonValue with
    static member AsEmptyRecord json =
      match json with
      | JsonValue.Record [||] -> sum.Return()
      | _ ->
        sum.Throw(Errors.Singleton $"Error: expected empty record, found '{json.ToFSharpString.ReasonablyClamped}'")

    static member AsRecord json =
      match json with
      | JsonValue.Record fields -> sum.Return fields
      | _ -> sum.Throw(Errors.Singleton $"Error: expected record, found '{json.ToFSharpString.ReasonablyClamped}'")

    static member AsRecordMap json =
      match json with
      | JsonValue.Record fields -> fields |> Map.ofSeq |> sum.Return
      | _ -> sum.Throw(Errors.Singleton $"Error: expected record, found '{json.ToFSharpString.ReasonablyClamped}'")

    static member AsArray json =
      match json with
      | JsonValue.Array fields -> sum.Return fields
      | _ -> sum.Throw(Errors.Singleton $"Error: expected array, found '{json.ToFSharpString.ReasonablyClamped}'")

    static member AsSingleton json =
      match json with
      | JsonValue.Array [| firstJson |] -> sum.Return(firstJson)
      | _ ->
        sum.Throw(
          Errors.Singleton
            $"Error: expected singleton (array with one element), found '{json.ToFSharpString.ReasonablyClamped}'"
        )

    static member AsPair json =
      match json with
      | JsonValue.Array [| firstJson; secondJson |] -> sum.Return(firstJson, secondJson)
      | _ -> sum.Throw(Errors.Singleton $"Error: expected pair, found '{json.ToFSharpString.ReasonablyClamped}'")

    static member AsTriple json =
      match json with
      | JsonValue.Array [| firstJson; secondJson; thirdJson |] -> sum.Return(firstJson, secondJson, thirdJson)
      | _ -> sum.Throw(Errors.Singleton $"Error: expected triple, found '{json.ToFSharpString.ReasonablyClamped}'")

    static member AsString json =
      match json with
      | JsonValue.String fields -> sum.Return fields
      | _ -> sum.Throw(Errors.Singleton $"Error: expected string, found '{json.ToFSharpString.ReasonablyClamped}'")

    static member AsEnum options json =
      match json with
      | JsonValue.String value when options |> Set.contains value -> sum.Return value
      | _ ->
        sum.Throw(
          Errors.Singleton
            $"Error: expected enum in {options.ToFSharpString}, found '{json.ToFSharpString.ReasonablyClamped}'"
        )

    static member AsBoolean json =
      match json with
      | JsonValue.Boolean fields -> sum.Return fields
      | _ -> sum.Throw(Errors.Singleton $"Error: expected boolean, found '{json.ToFSharpString.ReasonablyClamped}'")

    static member AsInt json =
      match json with
      | JsonValue.Number fields ->
        if fields |> System.Decimal.IsInteger then
          sum.Return(int fields)
        else
          sum.Throw(Errors.Singleton $"Error: expected integer, found '{json.ToFSharpString.ReasonablyClamped}'")
      | _ -> sum.Throw(Errors.Singleton $"Error: expected number, found '{json.ToFSharpString.ReasonablyClamped}'")

    static member AsNumber json =
      match json with
      | JsonValue.Number fields -> sum.Return fields
      | _ -> sum.Throw(Errors.Singleton $"Error: expected number, found '{json.ToFSharpString.ReasonablyClamped}'")
