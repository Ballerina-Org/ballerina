namespace Ballerina.Data.Spec.Json

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
  open Ballerina.DSL.Next.Json
  open Ballerina.State.WithError

  type V2Format with
    static member FromJsonTypesV2(jsonValue: JsonValue) : Sum<List<string * TypeExpr>, Errors> =

      sum {
        let! a = JsonValue.AsRecord jsonValue

        return!
          a
          |> List.ofArray
          |> List.map (fun (k, v) -> TypeExpr.FromJson v |> sum.Map(fun v -> k, v))
          |> sum.All
      }

    static member FromJsonTypesV2Many(topLevels: (string * JsonValue) array) : Sum<List<string * TypeExpr>, Errors> =

      sum {
        return!
          topLevels
          |> List.ofArray
          |> List.map (fun (k, v) -> TypeExpr.FromJson v |> sum.Map(fun v -> k, v))
          |> sum.All
      }

    static member ToJsonTypesV2(types: (string * TypeExpr) list) =
      types
      |> List.toArray
      |> Array.map (fun (k, v) -> k, TypeExpr.ToJson v)
      |> JsonValue.Record

    static member FromJson(jsonValue: JsonValue) : Sum<V2Format, Errors> =
      sum {
        let! jsonMap = JsonValue.AsRecordMap jsonValue

        let! types =
          jsonMap
          |> Map.tryFindWithError "typesV2" "spec" "Spec: 'name' field is required"

        let! schema = jsonMap |> Map.tryFindWithError "schema" "spec" "Spec: 'name' field is required"
        let! types = V2Format.FromJsonTypesV2 types
        let! schema = Schema<TypeExpr>.FromJson schema |> Reader.Run TypeExpr.FromJson

        return { TypesV2 = types; Schema = schema }
      }

    static member ParseV2Schema(jsonValue: JsonValue) : Sum<Schema<TypeExpr>, Errors> =
      sum {
        let! jsonMap = JsonValue.AsRecordMap jsonValue


        let! schema = jsonMap |> Map.tryFindWithError "schema" "spec" "Spec: 'name' field is required"
        let! schema = Schema<TypeExpr>.FromJson schema |> Reader.Run TypeExpr.FromJson

        return schema
      }

    static member ToJson(spec: V2Format) : Reader<JsonValue, JsonEncoder<TypeExpr>, Errors> =
      reader {
        let! schema = Schema<TypeValue>.ToJson spec.Schema

        return JsonValue.Record [| "typesV2", V2Format.ToJsonTypesV2 spec.TypesV2; "schema", schema |]
      }
