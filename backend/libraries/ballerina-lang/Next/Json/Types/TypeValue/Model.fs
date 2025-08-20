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
          TypeValue.FromJsonRecord TypeValue.FromJson json
          TypeValue.FromJsonTuple TypeValue.FromJson json
          TypeValue.FromJsonUnion TypeValue.FromJson json
          TypeValue.FromJsonSum TypeValue.FromJson json
          TypeValue.FromJsonList TypeValue.FromJson json
          TypeValue.FromJsonSet TypeValue.FromJson json
          TypeValue.FromJsonMap TypeValue.FromJson json
          $"Unknown TypeValue JSON: {json.ToFSharpString.ReasonablyClamped}"
          |> Errors.Singleton
          |> Errors.WithPriority ErrorPriority.Medium
          |> sum.Throw ]
      )
      |> sum.MapError(Errors.HighestPriority)

    static member ToJson: TypeValue -> JsonValue =
      fun typeValue ->
        match typeValue with
        | TypeValue.Primitive primitive -> TypeValue.ToJsonPrimitive primitive
        | TypeValue.Var var -> TypeValue.ToJsonVar var
        | TypeValue.Lookup lookup -> TypeValue.ToJsonLookup lookup
        | TypeValue.Arrow(fromType, toType) -> TypeValue.ToJsonArrow TypeValue.ToJson (fromType, toType)
        | TypeValue.Lambda(paramType, returnType) -> TypeValue.ToJsonLambda TypeExpr.ToJson (paramType, returnType)
        | TypeValue.Record fields -> TypeValue.ToJsonRecord TypeValue.ToJson fields
        | TypeValue.Tuple fields -> TypeValue.ToJsonTuple TypeValue.ToJson fields
        | TypeValue.Union cases -> TypeValue.ToJsonUnion TypeValue.ToJson cases
        | TypeValue.Sum values -> TypeValue.ToJsonSum TypeValue.ToJson values
        | TypeValue.List itemType -> TypeValue.ToJsonList TypeValue.ToJson itemType
        | TypeValue.Set itemType -> TypeValue.ToJsonSet TypeValue.ToJson itemType
        | TypeValue.Map(keyType, valueType) -> TypeValue.ToJsonMap TypeValue.ToJson (keyType, valueType)
