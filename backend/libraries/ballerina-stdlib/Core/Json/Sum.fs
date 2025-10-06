namespace Ballerina.StdLib.Json

module Sum =
  open FSharp.Data
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type SumBuilder with
    member _.AssertDiscriminatorAndContinue<'ctx, 'T>
      (discriminatorKey: string)
      (discriminatorValue: string)
      (k: Unit -> Sum<'T, Errors>)
      (json: JsonValue)
      : Sum<'T, Errors> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        match fields.TryFind discriminatorKey with
        | Some jsonDiscriminatorValue ->
          let! jsonDiscriminatorValue = jsonDiscriminatorValue |> JsonValue.AsString
          let fields = fields |> Map.remove discriminatorKey

          if jsonDiscriminatorValue = discriminatorValue && fields |> Map.isEmpty |> not then
            return!
              $"Error: Expected no additional fields, but found {fields |> Map.count} ({(fields |> Map.keys).ToFSharpString.ReasonablyClamped})."
              |> Errors.Singleton
              |> Errors.WithPriority ErrorPriority.High
              |> sum.Throw
          elif jsonDiscriminatorValue = discriminatorValue then
            return! k () |> sum.MapError(Errors.WithPriority ErrorPriority.Medium)
          else
            return!
              $"Error: Expected discriminator '{discriminatorValue}', but found '{jsonDiscriminatorValue}'."
              |> Errors.Singleton
              |> sum.Throw
        | None ->
          return!
            $"Error: Expected field '{discriminatorKey}' in JSON object '{json.ToFSharpString}', but it was not found."
            |> Errors.Singleton
            |> sum.Throw
      }

    member sum.AssertDiscriminatorAndContinueWithValue<'ctx, 'T>
      (discriminatorKey: string)
      (valueKey: string)
      (discriminatorValue: string)
      (k: JsonValue -> Sum<'T, Errors>)
      (json: JsonValue)
      : Sum<'T, Errors> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        match fields.TryFind discriminatorKey with
        | Some jsonDiscriminatorValue ->
          let! jsonDiscriminatorValue = jsonDiscriminatorValue |> JsonValue.AsString
          let fields = fields |> Map.remove discriminatorKey

          if jsonDiscriminatorValue = discriminatorValue && fields |> Map.count <> 1 then
            return!
              $"Error: Expected exactly one field, but found {fields |> Map.count} ({(fields |> Map.keys).ToFSharpString.ReasonablyClamped})."
              |> Errors.Singleton
              |> Errors.WithPriority ErrorPriority.High
              |> sum.Throw
          elif jsonDiscriminatorValue = discriminatorValue then
            let! fieldValue = fields |> Map.tryFindWithError valueKey "fields" valueKey
            return! k fieldValue |> sum.MapError(Errors.WithPriority ErrorPriority.High)
          else
            return!
              $"Error: Expected discriminator '{discriminatorValue}', but found '{jsonDiscriminatorValue}'."
              |> Errors.Singleton
              |> sum.Throw
        | None ->
          return!
            $"Error: Expected field '{discriminatorKey}' in JSON object '{json.ToFSharpString}', but it was not found."
            |> Errors.Singleton
            |> sum.Throw
      }
