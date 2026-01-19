namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeValue =
  open FSharp.Data
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json

  type TypeValue<'valueExt> with
    static member FromJson(json: JsonValue) : Sum<TypeValue<'valueExt>, Errors> =

      let inline parse
        (fromJson: (JsonValue -> Sum<TypeValue<'valueExt>, Errors>) -> JsonValue -> Sum<'v, Errors>)
        (ctor: WithSourceMapping<'v, 'valueExt> -> TypeValue<'valueExt>)
        json
        =
        WithSourceMapping.FromJson (fromJson TypeValue.FromJson) TypeExpr.FromJson json
        |> sum.Map ctor

      sum.Any(
        TypeValue.FromJsonPrimitive json,
        [ TypeValue.FromJsonVar(json)
          TypeValue.FromJsonLookup(json)
          parse TypeValue.FromJsonArrow TypeValue.Arrow json
          (WithSourceMapping.FromJson (TypeValue.FromJsonLambda TypeExpr.FromJson) TypeExpr.FromJson json
           |> sum.Map TypeValue.Lambda)
          parse TypeValue.FromJsonApplication TypeValue.Application json
          parse TypeValue.FromJsonRecord TypeValue.Record json
          parse TypeValue.FromJsonTuple TypeValue.Tuple json
          parse TypeValue.FromJsonUnion TypeValue.Union json
          parse TypeValue.FromJsonSum TypeValue.Sum json
          parse TypeValue.FromJsonSet TypeValue.Set json
          parse TypeValue.FromJsonMap TypeValue.Map json
          TypeValue.FromJsonImported TypeValue.FromJson json
          $"Unknown TypeValue JSON: {json.AsFSharpString.ReasonablyClamped}"
          |> Errors.Singleton
          |> Errors.WithPriority ErrorPriority.Medium
          |> sum.Throw ]
      )
      |> sum.MapError(Errors.HighestPriority)

    static member ToJson(v: TypeValue<'valueExt>) : JsonValue =
      let inline serialize makeJson mapping =
        WithSourceMapping<_, _>.ToJson (makeJson TypeValue.ToJson) TypeExpr.ToJson mapping

      match v with
      | TypeValue.Primitive primitive -> TypeValue.ToJsonPrimitive primitive.value
      | TypeValue.Var var -> TypeValue.ToJsonVar var
      | TypeValue.Lookup lookup -> TypeValue.ToJsonLookup lookup
      | TypeValue.Arrow mapping -> serialize TypeValue.ToJsonArrow mapping
      | TypeValue.Lambda mapping ->
        WithSourceMapping<TypeParameter * TypeExpr<'valueExt>, 'valueExt>.ToJson
          (TypeValue.ToJsonLambda TypeExpr.ToJson)
          TypeExpr.ToJson
          mapping
      | TypeValue.Application mapping -> serialize TypeValue.ToJsonApplication mapping
      | TypeValue.Record mapping -> serialize TypeValue.ToJsonRecord mapping
      | TypeValue.Tuple mapping -> serialize TypeValue.ToJsonTuple mapping
      | TypeValue.Union mapping -> serialize TypeValue.ToJsonUnion mapping
      | TypeValue.Sum mapping -> serialize TypeValue.ToJsonSum mapping
      | TypeValue.Set mapping -> serialize TypeValue.ToJsonSet mapping
      | TypeValue.Map mapping -> serialize TypeValue.ToJsonMap mapping
      | TypeValue.Imported i -> TypeValue.ToJsonImported TypeValue.ToJson i
      | TypeValue.Schema _ -> failwith "Schema ToJson not implemented"
      | TypeValue.Entity _ -> failwith "Schema Entity ToJson not implemented"
      | TypeValue.Entities _ -> failwith "Schema Entities ToJson not implemented"
      | TypeValue.Relations _ -> failwith "Schema Relations ToJson not implemented"
      | TypeValue.Relation _ -> failwith "Schema Relation ToJson not implemented"
      | TypeValue.LookupMaybe _ -> failwith "Schema LookupMaybe ToJson not implemented"
      | TypeValue.LookupOne _ -> failwith "Schema LookupOne ToJson not implemented"
      | TypeValue.LookupMany _ -> failwith "Schema LookupMany ToJson not implemented"
