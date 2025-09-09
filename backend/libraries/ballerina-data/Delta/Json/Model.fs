namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types.Model

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

    static member ToJson: DeltaEncoder<'valueExtension> =
      fun delta ->
        match delta with
        | Delta.Multiple deltas -> Delta.ToJsonMultiple Delta.ToJson deltas
        | Delta.Replace v -> Delta.ToJsonReplace v
        | Delta.Record(fieldName, fieldDelta) -> Delta.ToJsonRecord Delta.ToJson fieldName fieldDelta
        | Delta.Union(caseName, caseDelta) -> Delta.ToJsonUnion Delta.ToJson caseName caseDelta
        | Delta.Tuple(fieldIndex, fieldDelta) -> Delta.ToJsonTuple Delta.ToJson fieldIndex fieldDelta
        | Delta.Sum(index, fieldDelta) -> Delta.ToJsonSum Delta.ToJson index fieldDelta
