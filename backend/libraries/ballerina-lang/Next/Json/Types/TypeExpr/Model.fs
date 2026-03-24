namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json

  type TypeExpr<'valueExt> with
    static member FromJsonWith
      (typeValueFromJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors<_>>)
      (json: JsonValue)
      : Sum<TypeExpr<'valueExt>, Errors<_>> =
      let rec fromJson json =
        sum.Any(
          TypeExpr.FromJsonPrimitive json,
          [ TypeExpr.FromJsonApply fromJson json
            TypeExpr.FromJsonRotate fromJson json
            TypeExpr.FromJsonLambda fromJson json
            TypeExpr.FromJsonArrow fromJson json
            TypeExpr.FromJsonRecord fromJson json
            TypeExpr.FromJsonTuple fromJson json
            TypeExpr.FromJsonLookup json
            TypeExpr.FromJsonUnion fromJson json
            TypeExpr.FromJsonSum fromJson json
            TypeExpr.FromJsonSet fromJson json
            TypeExpr.FromJsonKeyOf fromJson json
            TypeExpr.FromJsonFlatten fromJson json
            TypeExpr.FromJsonExclude fromJson json
            TypeExpr.FromJsonFromTypeValue typeValueFromJson json
            fun () -> $"Unknown TypeExpr JSON: {json.AsFSharpString.ReasonablyClamped}"
            |> Errors.Singleton()
            |> Errors.MapPriority(replaceWith ErrorPriority.High)
            |> sum.Throw ]
        )
        |> sum.MapError(Errors.HighestPriority)

      fromJson json

    static member FromJson(json: JsonValue) : Sum<TypeExpr<'valueExt>, Errors<_>> =
      TypeExpr.FromJsonWith
        (fun _ ->
          sum.Throw(Errors.Singleton () (fun () -> "Unexpected TypeExpr.FromTypeValue in TypeExpr-only context")))
        json

    static member ToJsonWith(typeValueToJson: TypeValue<'valueExt> -> JsonValue) : TypeExpr<'valueExt> -> JsonValue =
      let toJsonFromTypeValue = TypeExpr.ToJsonFromTypeValue typeValueToJson

      let rec toJson typeExpr =
        match typeExpr with
        | TypeExpr.Primitive p -> TypeExpr.ToJsonPrimitive p
        | TypeExpr.Apply(a, b) -> TypeExpr.ToJsonApply toJson (a, b)
        | TypeExpr.Rotate r -> TypeExpr.ToJsonRotate toJson r
        | TypeExpr.Lambda(a, b) -> TypeExpr.ToJsonLambda toJson (a, b)
        | TypeExpr.Arrow(a, b) -> TypeExpr.ToJsonArrow toJson (a, b)
        | TypeExpr.Record r -> TypeExpr.ToJsonRecord toJson r
        | TypeExpr.Tuple t -> TypeExpr.ToJsonTuple toJson t
        | TypeExpr.Lookup l -> TypeExpr.ToJsonLookup l
        | TypeExpr.Union u -> TypeExpr.ToJsonUnion toJson u
        | TypeExpr.Sum s -> TypeExpr.ToJsonSum toJson s
        | TypeExpr.Set s -> TypeExpr.ToJsonSet toJson s
        | TypeExpr.KeyOf k -> TypeExpr.ToJsonKeyOf toJson k
        | TypeExpr.Flatten(a, b) -> TypeExpr.ToJsonFlatten toJson (a, b)
        | TypeExpr.Exclude(a, b) -> TypeExpr.ToJsonExclude toJson (a, b)
        | TypeExpr.NewSymbol _ -> JsonValue.Null
        | TypeExpr.Let(_, _, rest) -> toJson rest
        | TypeExpr.LetSymbols(_, _, rest) -> toJson rest
        | TypeExpr.FromTypeValue tv -> toJsonFromTypeValue tv
        | TypeExpr.Imported _ -> failwith "Imported ToJson not implemented"
        | TypeExpr.Schema _ -> failwith "Schema ToJson not implemented"
        | TypeExpr.Entities _ -> failwith "Entities ToJson not implemented"
        | TypeExpr.Relations _ -> failwith "Relations ToJson not implemented"
        | TypeExpr.Entity _ -> failwith "Entity ToJson not implemented"
        | TypeExpr.Relation _ -> failwith "Relation ToJson not implemented"
        | TypeExpr.RecordDes(_, _) -> failwith "RecordDes ToJson not implemented"
        | TypeExpr.RelationLookupOne _ -> failwith "RelationLookupOne ToJson not implemented"
        | TypeExpr.RelationLookupOption _ -> failwith "RelationLookupOption ToJson not implemented"
        | TypeExpr.RelationLookupMany _ -> failwith "RelationLookupMany ToJson not implemented"
        | TypeExpr.FromQueryRow -> failwith "FromQueryRow Not Implemented"
        | TypeExpr.QueryRow(_) -> failwith "QueryRow Not Implemented"

      toJson

    static member ToJson(typeExpr: TypeExpr<'valueExt>) : JsonValue =
      TypeExpr.ToJsonWith
        // This lambda should never be called, but is here as a safeguard to prevent accidental use of TypeExpr.ToJson in TypeValue-only context
        (fun _ -> failwith "Unexpected TypeExpr.FromTypeValue in TypeExpr-only context")
        typeExpr
