namespace Ballerina.Data.Json

module Model =
  open System
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Json
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Spec.Model
  open FSharp.Data

  type SpecData<'T, 'valueExtension> with
    static member ToJson
      (seeds: SpecData<'T, 'valueExtension>)
      : Reader<JsonValue, JsonEncoder<'T> * JsonEncoder<'valueExtension>, Errors> =
      reader {
        let! entities =
          seeds.Entities
          |> Map.toList
          |> List.map (fun (name, values) ->
            reader {
              let! values =
                values
                |> Map.toList
                |> Seq.map (fun (id, value) ->
                  reader {
                    let! value = value |> Value.ToJson
                    let! id = id |> PrimitiveValue.Guid |> Primitive |> Value.ToJson
                    return JsonValue.Record [| "id", id; "value", value |]
                  })
                |> reader.All

              let! name = name |> PrimitiveValue.String |> Primitive |> Value.ToJson
              return JsonValue.Record [| "name", name; "values", values |> Seq.toArray |> JsonValue.Array |]
            })
          |> reader.All

        let! lookups =
          seeds.Lookups
          |> Map.toList
          |> List.map (fun (name, values) ->
            reader {
              let! name = name |> PrimitiveValue.String |> Primitive |> Value.ToJson

              let! values =
                values
                |> Map.toList
                |> Seq.map (fun (id, v) ->
                  reader {
                    let! id = id |> PrimitiveValue.Guid |> Primitive |> Value.ToJson

                    let! value =
                      v
                      |> Set.toArray
                      |> Array.map (PrimitiveValue.Guid >> Primitive >> Value.ToJson)
                      |> reader.All

                    return JsonValue.Record [| "id", id; "value", value |> List.toArray |> JsonValue.Array |]
                  })
                |> reader.All

              return JsonValue.Record [| "name", name; "values", values |> Seq.toArray |> JsonValue.Array |]
            })
          |> reader.All

        return
          JsonValue.Record
            [| "entities", JsonValue.Array(entities |> List.toArray)
               "lookups", JsonValue.Array(lookups |> List.toArray) |]
      }

    static member FromJson
      (json: JsonValue, fromJsonExt: JsonParser<'valueExtension>)
      : Sum<SpecData<TypeValue, 'valueExtension>, Errors> =

      let parser = Value.FromJson fromJsonExt >> Reader.Run TypeValue.FromJson

      sum {
        let! container = JsonValue.AsRecord json

        let! entities =
          container
          |> Map.ofArray
          |> Map.tryFindWithError "entities" "entities" "entities"

        let! lookups = container |> Map.ofArray |> Map.tryFindWithError "lookups" "lookups" "lookups"

        let! entities = JsonValue.AsArray entities
        let! lookups = JsonValue.AsArray lookups

        let! e =
          entities
          |> Array.map (fun e ->
            sum {
              let! e = JsonValue.AsRecord e
              let! name = e |> Map.ofArray |> Map.tryFindWithError "name" "name" "name"
              let! name = JsonValue.AsString name
              let! values = e |> Map.ofArray |> Map.tryFindWithError "values" "values" "values"
              let! values = JsonValue.AsArray values

              let! vs =
                values
                |> Array.map (fun v ->
                  sum {
                    let! v = JsonValue.AsRecord v
                    let! id = v |> Map.ofArray |> Map.tryFindWithError "id" "id" "id"
                    let! value = v |> Map.ofArray |> Map.tryFindWithError "value" "value" "value"
                    let! id = JsonValue.AsString id
                    let id = Guid.Parse id
                    let! value = parser value
                    return id, value
                  })
                |> sum.All

              return name, vs |> Map.ofList
            })
          |> sum.All

        let e = e |> Map.ofList

        let! l =
          lookups
          |> Array.map (fun e ->
            sum {
              let! e = JsonValue.AsRecord e
              let! name = e |> Map.ofArray |> Map.tryFindWithError "name" "name" "name"
              let! name = JsonValue.AsString name
              let! values = e |> Map.ofArray |> Map.tryFindWithError "values" "values" "values"
              let! values = JsonValue.AsArray values

              let! vs =
                values
                |> Array.map (fun v ->
                  sum {
                    let! v = JsonValue.AsRecord v
                    let! id = v |> Map.ofArray |> Map.tryFindWithError "id" "id" "id"
                    let! value = v |> Map.ofArray |> Map.tryFindWithError "value" "value" "value"
                    let! id = JsonValue.AsString id
                    let id = Guid.Parse id
                    let! value = JsonValue.AsArray value
                    let! value = value |> Array.map JsonValue.AsString |> sum.All
                    let value = value |> List.map Guid.Parse |> Set.ofList
                    return id, value
                  })
                |> sum.All

              let vs = vs |> Map.ofList
              return name, vs
            })
          |> sum.All

        return
          { Entities = e
            Lookups = l |> Map.ofList }
      }
