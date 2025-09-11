namespace Ballerina.Data.Spec.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Spec =

  open Ballerina.Errors
  open Ballerina.Collections.Sum
  open FSharp.Data
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.Data.Schema.Model
  open Ballerina.Data.Json.Schema
  open Ballerina.Data.Spec.Model
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns

  type Spec with
    static member private FromJsonTypesV2(jsonValue: JsonValue) : Sum<List<string * TypeExpr>, Errors> =

      sum {
        let! a = JsonValue.AsRecord jsonValue

        return!
          a
          |> List.ofArray
          |> List.map (fun (k, v) -> TypeExpr.FromJson v |> sum.Map(fun v -> k, v))
          |> sum.All
      }

    static member private ToJsonTypesV2(types: (string * TypeExpr) list) =
      types
      |> List.toArray
      |> Array.map (fun (k, v) -> k, TypeExpr.ToJson v)
      |> JsonValue.Record

    static member FromJson(jsonValue: JsonValue) : Sum<Spec, Errors> =
      sum {
        let! jsonMap = JsonValue.AsRecordMap jsonValue

        let! name = jsonMap |> Map.tryFindWithError "name" "spec" "Spec: 'name' field is required"
        let! name = JsonValue.AsString name

        let! fields = jsonMap |> Map.tryFindWithError "fields" "spec" "Spec: 'name' field is required"
        let! jsonMap = JsonValue.AsRecordMap fields

        let! types =
          jsonMap
          |> Map.tryFindWithError "typesV2" "spec" "Spec: 'name' field is required"

        let! schema = jsonMap |> Map.tryFindWithError "schema" "spec" "Spec: 'name' field is required"
        let! types = Spec.FromJsonTypesV2 types
        let! schema = Schema<TypeExpr>.FromJson schema |> Reader.Run TypeExpr.FromJson

        let body = { TypesV2 = types; Schema = schema }

        let spec = { Name = SpecName name; Body = body }
        return spec
      }

    static member ToJson(spec: Spec) : Reader<JsonValue, JsonEncoder<TypeExpr>, Errors> =
      reader {
        let (SpecName name) = spec.Name
        let! schema = Schema<TypeValue>.ToJson spec.Body.Schema

        return
          JsonValue.Record
            [| "name", JsonValue.String name
               "fields", JsonValue.Record [| "typesV2", Spec.ToJsonTypesV2 spec.Body.TypesV2; "schema", schema |] |]
      }
