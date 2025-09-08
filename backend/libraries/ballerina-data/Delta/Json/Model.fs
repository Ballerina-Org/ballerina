namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module Model =
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open Ballerina.Reader.WithError
  open Ballerina.Data.Delta.Model
  open FSharp.Data
  open Ballerina.Errors

  type Delta<'valueExtension> with

    static member FromJson: DeltaParser<'valueExtension> =
      fun json ->
        reader.Any(
          Delta.FromJsonMultiple Delta.FromJson json,
          [ Delta.FromJsonReplace json
            Delta.FromJsonRecord Delta.FromJson json
            Delta.FromJsonUnion Delta.FromJson json
            Delta.FromJsonTuple Delta.FromJson json
            Delta.FromJsonSum Delta.FromJson json
            $"Unknown TypeValue JSON: {json.ToFSharpString.ReasonablyClamped}"
            |> Errors.Singleton
            |> Errors.WithPriority ErrorPriority.High
            |> reader.Throw ]
        )
        |> reader.MapError(Errors.HighestPriority)

    static member ToJson: ('valueExtension -> JsonValue) -> Delta<'valueExtension> -> JsonValue =
      fun extToJson delta ->
        match delta with
        | Delta.Multiple deltas -> Delta.ToJsonMultiple (Delta.ToJson extToJson) deltas
        | Delta.Replace v -> Delta.ToJsonReplace extToJson v
        | Delta.Record(fieldName, fieldDelta) -> Delta.ToJsonRecord (Delta.ToJson extToJson) (fieldName, fieldDelta)
        | Delta.Union(caseName, caseDelta) -> Delta.ToJsonUnion (Delta.ToJson extToJson) (caseName, caseDelta)
        | Delta.Tuple(fieldIndex, fieldDelta) -> Delta.ToJsonTuple (Delta.ToJson extToJson) (fieldIndex, fieldDelta)
        | Delta.Sum(index, fieldDelta) -> Delta.ToJsonSum (Delta.ToJson extToJson) (index, fieldDelta)
