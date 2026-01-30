namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module PrimitiveType =
  open Ballerina.StdLib.Json.Sum
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open FSharp.Data
  open Ballerina.DSL.Next.Json.Keys

  let private WithDiscriminator (value: string) : string =
    $"""{{"{discriminatorKey}": "{value}"}}"""

  type PrimitiveType with

    static member private FromJsonUnit: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "unit" (fun _ -> sum { return PrimitiveType.Unit })

    static member private FromJsonGuid: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "guid" (fun _ -> sum { return PrimitiveType.Guid })

    static member private FromJsonInt32: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "int32" (fun _ -> sum { return PrimitiveType.Int32 })

    static member private FromJsonInt64: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "int64" (fun _ -> sum { return PrimitiveType.Int64 })

    static member private FromJsonFloat32: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "float32" (fun _ -> sum { return PrimitiveType.Float32 })

    static member private FromJsonFloat64: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "float64" (fun _ -> sum { return PrimitiveType.Float64 })

    static member private FromJsonDecimal: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "decimal" (fun _ -> sum { return PrimitiveType.Decimal })

    static member private FromJsonString: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "string" (fun _ -> sum { return PrimitiveType.String })

    static member private FromJsonBool: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "bool" (fun _ -> sum { return PrimitiveType.Bool })

    static member private FromJsonDateTime: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "datetime" (fun _ -> sum { return PrimitiveType.DateTime })

    static member private FromJsonDateOnly: JsonValue -> Sum<PrimitiveType, Errors<_>> =
      Sum.assertDiscriminatorAndContinue "dateonly" (fun _ -> sum { return PrimitiveType.DateOnly })

    static member FromJson(json: JsonValue) : Sum<PrimitiveType, Errors<_>> =
      sum.Any(
        PrimitiveType.FromJsonUnit(json),
        [ PrimitiveType.FromJsonGuid(json)
          PrimitiveType.FromJsonInt32(json)
          PrimitiveType.FromJsonInt64(json)
          PrimitiveType.FromJsonFloat32(json)
          PrimitiveType.FromJsonFloat64(json)
          PrimitiveType.FromJsonDecimal(json)
          PrimitiveType.FromJsonString(json)
          PrimitiveType.FromJsonBool(json)
          PrimitiveType.FromJsonDateTime(json)
          PrimitiveType.FromJsonDateOnly(json) ]
      )
      |> sum.MapError(Errors.HighestPriority)

    static member private ToJsonUnit() : JsonValue =
      JsonValue.Parse(WithDiscriminator "unit")

    static member private ToJsonGuid() : JsonValue =
      JsonValue.Parse(WithDiscriminator "guid")

    static member private ToJsonInt32() : JsonValue =
      JsonValue.Parse(WithDiscriminator "int32")

    static member private ToJsonInt64() : JsonValue =
      JsonValue.Parse(WithDiscriminator "int64")

    static member private ToJsonFloat32() : JsonValue =
      JsonValue.Parse(WithDiscriminator "float32")

    static member private ToJsonFloat64() : JsonValue =
      JsonValue.Parse(WithDiscriminator "float64")

    static member private ToJsonDecimal() : JsonValue =
      JsonValue.Parse(WithDiscriminator "decimal")

    static member private ToJsonString() : JsonValue =
      JsonValue.Parse(WithDiscriminator "string")

    static member private ToJsonBool() : JsonValue =
      JsonValue.Parse(WithDiscriminator "bool")

    static member private ToJsonDateTime() : JsonValue =
      JsonValue.Parse(WithDiscriminator "datetime")

    static member private ToJsonDateOnly() : JsonValue =
      JsonValue.Parse(WithDiscriminator "dateonly")

    static member private ToJsonTimeSpan() : JsonValue =
      JsonValue.Parse(WithDiscriminator "timespan")

    static member ToJson: PrimitiveType -> JsonValue =
      function
      | PrimitiveType.Unit -> PrimitiveType.ToJsonUnit()
      | PrimitiveType.Guid -> PrimitiveType.ToJsonGuid()
      | PrimitiveType.Int32 -> PrimitiveType.ToJsonInt32()
      | PrimitiveType.Float32 -> PrimitiveType.ToJsonFloat32()
      | PrimitiveType.Int64 -> PrimitiveType.ToJsonInt64()
      | PrimitiveType.Float64 -> PrimitiveType.ToJsonFloat64()
      | PrimitiveType.Decimal -> PrimitiveType.ToJsonDecimal()
      | PrimitiveType.String -> PrimitiveType.ToJsonString()
      | PrimitiveType.Bool -> PrimitiveType.ToJsonBool()
      | PrimitiveType.DateTime -> PrimitiveType.ToJsonDateTime()
      | PrimitiveType.DateOnly -> PrimitiveType.ToJsonDateOnly()
      | PrimitiveType.TimeSpan -> PrimitiveType.ToJsonTimeSpan()
