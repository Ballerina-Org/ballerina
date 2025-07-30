namespace Ballerina.StdLib.Json

module Sum =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type SumBuilder with
    member _.AsRecord<'a>
      (fieldParsers: List<string * (JsonValue -> Sum<'a, Errors>)>)
      (json: JsonValue)
      : Sum<Map<string, 'a>, Errors> =
      sum {
        let! jsonFields = json |> JsonValue.AsRecordMap

        if jsonFields |> Map.count <> fieldParsers.Length then
          return!
            $"Error: Expected {fieldParsers.Length} fields, but found {jsonFields |> Map.count}."
            |> Errors.Singleton
            |> sum.Throw
        else

          let! fields =
            fieldParsers
            |> List.map (fun (name, parser) ->
              sum {
                let! jsonField = jsonFields |> Map.tryFindWithError name "fields" name
                let! field = parser jsonField
                return name, field
              })
            |> sum.All

          return fields |> Map.ofList
      }

    member _.AssertKindAndContinue<'T>
      (kind: string)
      (k: Map<string, JsonValue> -> Sum<'T, Errors>)
      (json: JsonValue)
      : Sum<'T, Errors> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        match fields.TryFind "kind" with
        | Some kindValue ->
          let! kindValue = kindValue |> JsonValue.AsString
          let fields = fields |> Map.remove "kind"

          if kindValue = kind then
            return! k fields |> sum.MapError(Errors.WithPriority ErrorPriority.Medium)
          else
            return!
              $"Error: Expected kind '{kind}', but found '{kindValue}'."
              |> Errors.Singleton
              |> sum.Throw
        | None ->
          return!
            $"Error: Expected field 'kind' in JSON object, but it was not found."
            |> Errors.Singleton
            |> sum.Throw
      }

    member sum.AssertKindAndContinueWithField<'T>
      (kind: string)
      (fieldKey: string)
      (k: JsonValue -> Sum<'T, Errors>)
      : JsonValue -> Sum<'T, Errors> =
      sum.AssertKindAndContinue kind (fun fields ->
        sum {
          if fields |> Map.count <> 1 then
            return!
              $"Error: Expected exactly one field, but found {fields |> Map.count}."
              |> Errors.Singleton
              |> sum.Throw
          else
            let! fieldValue = fields |> Map.tryFindWithError fieldKey "fields" fieldKey
            return! k fieldValue |> sum.MapError(Errors.WithPriority ErrorPriority.High)
        })
