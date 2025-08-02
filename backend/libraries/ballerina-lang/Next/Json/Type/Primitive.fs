namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module PrimitiveType =
  open Ballerina.StdLib.Json.Sum
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open FSharp.Data

  type PrimitiveType with

    static member private FromJsonUnit: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "unit" (fun _ -> sum { return PrimitiveType.Unit })

    static member private FromJsonGuid: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "guid" (fun _ -> sum { return PrimitiveType.Guid })

    static member private FromJsonInt: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "int" (fun _ -> sum { return PrimitiveType.Int })

    static member private FromJsonDecimal: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "decimal" (fun _ -> sum { return PrimitiveType.Decimal })

    static member private FromJsonString: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "string" (fun _ -> sum { return PrimitiveType.String })

    static member private FromJsonBool: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "bool" (fun _ -> sum { return PrimitiveType.Bool })

    static member private FromJsonDateTime: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "datetime" (fun _ -> sum { return PrimitiveType.DateTime })

    static member private FromJsonDateOnly: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "dateonly" (fun _ -> sum { return PrimitiveType.DateOnly })

    static member FromJson(json: JsonValue) : Sum<PrimitiveType, Errors> =
      sum.Any(
        PrimitiveType.FromJsonUnit(json),
        [ PrimitiveType.FromJsonGuid(json)
          PrimitiveType.FromJsonInt(json)
          PrimitiveType.FromJsonDecimal(json)
          PrimitiveType.FromJsonString(json)
          PrimitiveType.FromJsonBool(json)
          PrimitiveType.FromJsonDateTime(json)
          PrimitiveType.FromJsonDateOnly(json) ]
      )
      |> sum.MapError(Errors.HighestPriority)

    static member private ToJsonUnit() : JsonValue = JsonValue.Parse """{"kind": "unit"}"""

    static member private ToJsonGuid() : JsonValue = JsonValue.Parse """{"kind": "guid"}"""

    static member private ToJsonInt() : JsonValue = JsonValue.Parse """{"kind": "int"}"""

    static member private ToJsonDecimal() : JsonValue =
      JsonValue.Parse """{"kind": "decimal"}"""

    static member private ToJsonString() : JsonValue =
      JsonValue.Parse """{"kind": "string"}"""

    static member private ToJsonBool() : JsonValue = JsonValue.Parse """{"kind": "bool"}"""

    static member private ToJsonDateTime() : JsonValue =
      JsonValue.Parse """{"kind": "datetime"}"""

    static member private ToJsonDateOnly() : JsonValue =
      JsonValue.Parse """{"kind": "dateonly"}"""

    static member ToJson: PrimitiveType -> JsonValue =
      function
      | PrimitiveType.Unit -> PrimitiveType.ToJsonUnit()
      | PrimitiveType.Guid -> PrimitiveType.ToJsonGuid()
      | PrimitiveType.Int -> PrimitiveType.ToJsonInt()
      | PrimitiveType.Decimal -> PrimitiveType.ToJsonDecimal()
      | PrimitiveType.String -> PrimitiveType.ToJsonString()
      | PrimitiveType.Bool -> PrimitiveType.ToJsonBool()
      | PrimitiveType.DateTime -> PrimitiveType.ToJsonDateTime()
      | PrimitiveType.DateOnly -> PrimitiveType.ToJsonDateOnly()
