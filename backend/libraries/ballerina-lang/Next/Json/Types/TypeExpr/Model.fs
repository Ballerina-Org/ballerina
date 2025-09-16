namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json

  type TypeExpr with
    static member FromJson(json: JsonValue) : Sum<TypeExpr, Errors> =
      sum.Any(
        TypeExpr.FromJsonPrimitive json,
        [ TypeExpr.FromJsonApply TypeExpr.FromJson json
          TypeExpr.FromJsonRotate TypeExpr.FromJson json
          TypeExpr.FromJsonLambda TypeExpr.FromJson json
          TypeExpr.FromJsonArrow TypeExpr.FromJson json
          TypeExpr.FromJsonRecord TypeExpr.FromJson json
          TypeExpr.FromJsonTuple TypeExpr.FromJson json
          TypeExpr.FromJsonLookup json
          TypeExpr.FromJsonUnion TypeExpr.FromJson json
          TypeExpr.FromJsonSum TypeExpr.FromJson json
          TypeExpr.FromJsonSet TypeExpr.FromJson json
          TypeExpr.FromJsonMap TypeExpr.FromJson json
          TypeExpr.FromJsonKeyOf TypeExpr.FromJson json
          TypeExpr.FromJsonFlatten TypeExpr.FromJson json
          TypeExpr.FromJsonExclude TypeExpr.FromJson json
          $"Unknown TypeValue JSON: {json.ToFSharpString.ReasonablyClamped}"
          |> Errors.Singleton
          |> Errors.WithPriority ErrorPriority.High
          |> sum.Throw ]
      )
      |> sum.MapError(Errors.HighestPriority)

    static member ToJson: TypeExpr -> JsonValue =
      fun typeExpr ->
        match typeExpr with
        | TypeExpr.Primitive p -> TypeExpr.ToJsonPrimitive p
        | TypeExpr.Apply(a, b) -> TypeExpr.ToJsonApply TypeExpr.ToJson (a, b)
        | TypeExpr.Rotate r -> TypeExpr.ToJsonRotate TypeExpr.ToJson r
        | TypeExpr.Lambda(a, b) -> TypeExpr.ToJsonLambda TypeExpr.ToJson (a, b)
        | TypeExpr.Arrow(a, b) -> TypeExpr.ToJsonArrow TypeExpr.ToJson (a, b)
        | TypeExpr.Record r -> TypeExpr.ToJsonRecord TypeExpr.ToJson r
        | TypeExpr.Tuple t -> TypeExpr.ToJsonTuple TypeExpr.ToJson t
        | TypeExpr.Lookup l -> TypeExpr.ToJsonLookup l
        | TypeExpr.Union u -> TypeExpr.ToJsonUnion TypeExpr.ToJson u
        | TypeExpr.Sum s -> TypeExpr.ToJsonSum TypeExpr.ToJson s
        | TypeExpr.Set s -> TypeExpr.ToJsonSet TypeExpr.ToJson s
        | TypeExpr.Map(a, b) -> TypeExpr.ToJsonMap TypeExpr.ToJson (a, b)
        | TypeExpr.KeyOf k -> TypeExpr.ToJsonKeyOf TypeExpr.ToJson k
        | TypeExpr.Flatten(a, b) -> TypeExpr.ToJsonFlatten TypeExpr.ToJson (a, b)
        | TypeExpr.Exclude(a, b) -> TypeExpr.ToJsonExclude TypeExpr.ToJson (a, b)
        | TypeExpr.NewSymbol _ -> JsonValue.Null
        | TypeExpr.Let(_, _, rest) -> TypeExpr.ToJson rest
        | TypeExpr.Imported _ -> failwith "Not implemented"
