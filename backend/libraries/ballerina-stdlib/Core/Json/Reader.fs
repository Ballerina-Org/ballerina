namespace Ballerina.StdLib.Json

module Reader =
  open FSharp.Data
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Map
  open Ballerina.Errors

  type ReaderWithErrorBuilder with
    member _.AsRecord<'a, 'c>
      (fieldParsers: (List<string * (JsonValue -> Reader<'a, 'c, Errors>)>))
      (json: JsonValue)
      : Reader<Map<string, 'a>, 'c, Errors> =
      reader {
        let! jsonFields = json |> JsonValue.AsRecordMap |> reader.OfSum

        if jsonFields |> Map.count <> fieldParsers.Length then
          return!
            $"Error: Expected {fieldParsers.Length} fields, but found {jsonFields |> Map.count}."
            |> Errors.Singleton
            |> reader.Throw
        else

          let! fields =
            fieldParsers
            |> List.map (fun (name, parser) ->
              reader {
                let! jsonField = jsonFields |> Map.tryFindWithError name "fields" name |> reader.OfSum
                let! field = parser jsonField
                return name, field
              })
            |> reader.All

          return fields |> Map.ofList
      }

    member _.AssertKindAndContinue<'ctx, 'T>
      (kind: string)
      (k: Unit -> Reader<'T, 'ctx, Errors>)
      (json: JsonValue)
      : Reader<'T, 'ctx, Errors> =
      reader {
        let! fields = json |> JsonValue.AsRecordMap |> reader.OfSum

        match fields.TryFind "kind" with
        | Some kindValue ->
          let! kindValue = kindValue |> JsonValue.AsString |> reader.OfSum
          let fields = fields |> Map.remove "kind"

          if kindValue = kind && fields |> Map.isEmpty |> not then
            return!
              $"Error: Expected no additional fields, but found {fields |> Map.count} ({(fields |> Map.keys).ToFSharpString.ReasonablyClamped})."
              |> Errors.Singleton
              |> Errors.WithPriority ErrorPriority.High
              |> reader.Throw
          elif kindValue = kind then
            return! k () |> reader.MapError(Errors.WithPriority ErrorPriority.Medium)
          else
            return!
              $"Error: Expected kind '{kind}', but found '{kindValue}'."
              |> Errors.Singleton
              |> reader.Throw
        | None ->
          return!
            $"Error: Expected field 'kind' in JSON object, but it was not found."
            |> Errors.Singleton
            |> reader.Throw
      }

    member reader.AssertKindAndContinueWithField<'ctx, 'T>
      (kind: string)
      (fieldKey: string)
      (k: JsonValue -> Reader<'T, 'ctx, Errors>)
      : JsonValue -> Reader<'T, 'ctx, Errors> =
      fun json ->
        reader {
          let! fields = json |> JsonValue.AsRecordMap |> reader.OfSum

          match fields.TryFind "kind" with
          | Some kindValue ->
            let! kindValue = kindValue |> JsonValue.AsString |> reader.OfSum
            let fields = fields |> Map.remove "kind"

            if kindValue = kind && fields |> Map.count <> 1 then
              return!
                $"Error: Expected exactly one field, but found {fields |> Map.count} ({(fields |> Map.keys).ToFSharpString.ReasonablyClamped})."
                |> Errors.Singleton
                |> Errors.WithPriority ErrorPriority.High
                |> reader.Throw
            elif kindValue = kind then
              let! fieldValue = fields |> Map.tryFindWithError fieldKey "fields" fieldKey |> reader.OfSum
              return! k fieldValue |> reader.MapError(Errors.WithPriority ErrorPriority.High)
            else
              return!
                $"Error: Expected kind '{kind}', but found '{kindValue}'."
                |> Errors.Singleton
                |> reader.Throw
          | None ->
            return!
              $"Error: Expected field 'kind' in JSON object, but it was not found."
              |> Errors.Singleton
              |> reader.Throw

        }
