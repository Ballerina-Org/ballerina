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

  type TypeValue with
    static member FromJson(json: JsonValue) : Sum<TypeValue, Errors> =
      sum.Any(
        TypeValue.FromJsonPrimitive json,
        [ TypeValue.FromJsonVar(json)
          TypeValue.FromJsonLookup(json)
          TypeValue.FromJsonArrow TypeValue.FromJson json
          TypeValue.FromJsonLambda TypeExpr.FromJson json
          TypeValue.FromJsonApplication TypeValue.FromJson json
          TypeValue.FromJsonRecord TypeValue.FromJson json
          TypeValue.FromJsonTuple TypeValue.FromJson json
          TypeValue.FromJsonUnion TypeValue.FromJson json
          TypeValue.FromJsonSum TypeValue.FromJson json
          TypeValue.FromJsonSet TypeValue.FromJson json
          TypeValue.FromJsonMap TypeValue.FromJson json
          TypeValue.FromJsonImported TypeValue.FromJson json
          $"Unknown TypeValue JSON: {json.ToFSharpString.ReasonablyClamped}"
          |> Errors.Singleton
          |> Errors.WithPriority ErrorPriority.Medium
          |> sum.Throw ]
      )
      |> sum.MapError(Errors.HighestPriority)

    static member ToJson(v: TypeValue) : JsonValue =
      match v with
      | TypeValue.Primitive primitive -> TypeValue.ToJsonPrimitive primitive.value
      | TypeValue.Var var -> TypeValue.ToJsonVar var
      | TypeValue.Lookup lookup -> TypeValue.ToJsonLookup lookup
      | TypeValue.Arrow { value = fromType, toType } -> TypeValue.ToJsonArrow TypeValue.ToJson (fromType, toType)
      | TypeValue.Lambda { value = paramType, returnType } ->
        TypeValue.ToJsonLambda TypeExpr.ToJson (paramType, returnType)
      | TypeValue.Application { value = symbolicApp } -> TypeValue.ToJsonApplication TypeValue.ToJson symbolicApp
      | TypeValue.Record { value = fields } -> TypeValue.ToJsonRecord TypeValue.ToJson fields
      | TypeValue.Tuple { value = fields } -> TypeValue.ToJsonTuple TypeValue.ToJson fields
      | TypeValue.Union { value = cases } -> TypeValue.ToJsonUnion TypeValue.ToJson cases
      | TypeValue.Sum { value = values } -> TypeValue.ToJsonSum TypeValue.ToJson values
      | TypeValue.Set { value = itemType } -> TypeValue.ToJsonSet TypeValue.ToJson itemType
      | TypeValue.Map { value = keyType, valueType } -> TypeValue.ToJsonMap TypeValue.ToJson (keyType, valueType)
      | TypeValue.Imported i -> TypeValue.ToJsonImported TypeValue.ToJson i
