namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.Collections.Sum

[<AutoOpen>]
module Model =
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open Ballerina.Reader.WithError
  open Ballerina.Data.Delta.Model
  open Ballerina.Errors

  type Delta<'valueExtension, 'deltaExtension> with

    static member FromJson: DeltaParser<'valueExtension, 'deltaExtension> =
      fun json ->
        reader {
          let! _, extParser = reader.GetContext()

          return!
            reader.Any(
              Delta.FromJsonMultiple Delta.FromJson json,
              [ Delta.FromJsonReplace json
                Delta.FromJsonRecord Delta.FromJson json
                Delta.FromJsonUnion Delta.FromJson json
                Delta.FromJsonTuple Delta.FromJson json
                Delta.FromJsonSum Delta.FromJson json
                extParser json |> sum.Map Delta.Ext |> reader.OfSum
                $"Unknown Delta JSON: {json.AsFSharpString.ReasonablyClamped}"
                |> Errors.Singleton
                |> Errors.WithPriority ErrorPriority.High
                |> reader.Throw ]
            )
            |> reader.MapError(Errors.HighestPriority)
        }


    static member ToJson
      (delta: Delta<'valueExtension, 'deltaExtension>)
      : DeltaEncoderReader<'valueExtension, 'deltaExtension> =
      reader {
        let! _, extEncoder = reader.GetContext()

        return!
          match delta with
          | Delta.Multiple deltas -> Delta.ToJsonMultiple Delta.ToJson deltas
          | Delta.Replace v -> Delta.ToJsonReplace v
          | Delta.Record(fieldName, fieldDelta) -> Delta.ToJsonRecord Delta.ToJson fieldName fieldDelta
          | Delta.Union(caseName, caseDelta) -> Delta.ToJsonUnion Delta.ToJson caseName caseDelta
          | Delta.Tuple(fieldIndex, fieldDelta) -> Delta.ToJsonTuple Delta.ToJson fieldIndex fieldDelta
          | Delta.Sum(index, fieldDelta) -> Delta.ToJsonSum Delta.ToJson index fieldDelta
          | Delta.Ext ext -> extEncoder ext |> reader.OfSum
      }
