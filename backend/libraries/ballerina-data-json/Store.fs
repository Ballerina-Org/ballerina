namespace Ballerina.Data.Json

open Ballerina.Data.Schema.Model

module Model =
  open System
  open Ballerina
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
      (rootToJson: ValueEncoder<'T, 'valueExtension>)
      (seeds: SpecData<'T, 'valueExtension>)
      : ValueEncoderReader<'T, 'valueExtension> =
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
                    let! value = value |> rootToJson
                    let! id = id |> PrimitiveValue.Guid |> Primitive |> rootToJson
                    return JsonValue.Record [| "id", id; "value", value |]
                  })
                |> reader.All

              let! name = name.EntityName |> PrimitiveValue.String |> Primitive |> rootToJson
              return JsonValue.Record [| "name", name; "values", values |> Seq.toArray |> JsonValue.Array |]
            })
          |> reader.All

        let! lookups =
          seeds.Lookups
          |> Map.toList
          |> List.map (fun (name, values) ->
            reader {
              let! name = name.LookupName |> PrimitiveValue.String |> Primitive |> rootToJson

              let! values =
                values
                |> Map.toList
                |> Seq.map (fun (id, v) ->
                  reader {
                    let! id = id |> PrimitiveValue.Guid |> Primitive |> rootToJson

                    let! value =
                      v
                      |> Set.toArray
                      |> Array.map (PrimitiveValue.Guid >> Primitive >> rootToJson)
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
      (rootFromJson: ValueParser<TypeValue<'valueExtension>, ResolvedIdentifier, 'valueExtension>)
      (json: JsonValue)
      : Sum<SpecData<TypeValue<'valueExtension>, 'valueExtension>, Errors<unit>> =

      let rootExprFromJson =
        Expr.FromJson >> Reader.Run(TypeValue.FromJson, ResolvedIdentifier.FromJson)

      let parser =
        rootFromJson
        >> Reader.Run(rootExprFromJson, TypeValue.FromJson, ResolvedIdentifier.FromJson)

      sum {
        let! container = JsonValue.AsRecord json

        let! entities =
          container
          |> Map.ofArray
          |> Map.tryFindWithError "entities" "entities" (fun () -> "entities") ()

        let! lookups =
          container
          |> Map.ofArray
          |> Map.tryFindWithError "lookups" "lookups" (fun () -> "lookups") ()

        let! entities = JsonValue.AsArray entities
        let! lookups = JsonValue.AsArray lookups

        let! e =
          entities
          |> Array.map (fun e ->
            sum {
              let! e = JsonValue.AsRecord e
              let! name = e |> Map.ofArray |> Map.tryFindWithError "name" "name" (fun () -> "name") ()
              let! name = JsonValue.AsString name

              let! values =
                e
                |> Map.ofArray
                |> Map.tryFindWithError "values" "values" (fun () -> "values") ()

              let! values = JsonValue.AsArray values

              let! vs =
                values
                |> Array.map (fun v ->
                  sum {
                    let! v = JsonValue.AsRecord v
                    let! id = v |> Map.ofArray |> Map.tryFindWithError "id" "id" (fun () -> "id") ()
                    let! value = v |> Map.ofArray |> Map.tryFindWithError "value" "value" (fun () -> "value") ()
                    let! id = JsonValue.AsString id
                    let id = Guid.Parse id
                    let! value = parser value
                    return id, value
                  })
                |> sum.All

              return { EntityName = name }, vs |> Map.ofList
            })
          |> sum.All

        let e = e |> Map.ofList

        let! l =
          lookups
          |> Array.map (fun e ->
            sum {
              let! e = JsonValue.AsRecord e
              let! name = e |> Map.ofArray |> Map.tryFindWithError "name" "name" (fun () -> "name") ()
              let! name = JsonValue.AsString name

              let! values =
                e
                |> Map.ofArray
                |> Map.tryFindWithError "values" "values" (fun () -> "values") ()

              let! values = JsonValue.AsArray values

              let! vs =
                values
                |> Array.map (fun v ->
                  sum {
                    let! v = JsonValue.AsRecord v
                    let! id = v |> Map.ofArray |> Map.tryFindWithError "id" "id" (fun () -> "id") ()
                    let! value = v |> Map.ofArray |> Map.tryFindWithError "value" "value" (fun () -> "value") ()
                    let! id = JsonValue.AsString id
                    let id = Guid.Parse id
                    let! value = JsonValue.AsArray value
                    let! value = value |> Array.map JsonValue.AsString |> sum.All
                    let value = value |> List.map Guid.Parse |> Set.ofList
                    return id, value
                  })
                |> sum.All

              let vs = vs |> Map.ofList
              return { LookupName = name }, vs
            })
          |> sum.All

        return
          { Entities = e
            Lookups = l |> Map.ofList }
      }
