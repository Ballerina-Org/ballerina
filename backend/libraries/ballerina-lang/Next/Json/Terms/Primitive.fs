namespace Ballerina.DSL.Next.Terms.Json

open System.Globalization
open Ballerina.DSL.Next.Terms.Model

module Primitive =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Json
  open Ballerina.StdLib.Json.Sum


  type Int32 with
    static member FromString: string -> Sum<int, Errors> =
      Int32.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected int, found non-integer string")

    static member ToJsonString: int -> JsonValue =
      fun i ->
        JsonValue.Record
          [| "kind", JsonValue.String "int32"
             "int32", JsonValue.String(i.ToString(CultureInfo.InvariantCulture)) |]

  type Int64 with
    static member FromString: string -> Sum<int64, Errors> =
      Int64.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected int64, found non-integer string")

    static member ToJsonString: int64 -> JsonValue =
      fun i ->
        JsonValue.Record
          [| "kind", JsonValue.String "int64"
             "int64", JsonValue.String(i.ToString(CultureInfo.InvariantCulture)) |]

  type Single with
    static member FromString: string -> Sum<float32, Errors> =
      Single.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected float32, found non-float32 string")

    static member ToJsonString: float32 -> JsonValue =
      fun f ->
        JsonValue.Record
          [| "kind", JsonValue.String "float32"
             "float32", JsonValue.String(f.ToString(CultureInfo.InvariantCulture)) |]

  type Double with
    static member FromString: string -> Sum<float, Errors> =
      Double.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected float64, found non-float64 string")

    static member ToJsonString: float -> JsonValue =
      fun f ->
        JsonValue.Record
          [| "kind", JsonValue.String "float64"
             "float64", JsonValue.String(f.ToString(CultureInfo.InvariantCulture)) |]

  type Guid with
    static member FromString: string -> Sum<System.Guid, Errors> =
      Guid.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected Guid, found non-Guid string")

    static member ToJsonString: System.Guid -> JsonValue =
      _.ToString("D", System.Globalization.CultureInfo.InvariantCulture)
      >> JsonValue.String
      >> Json.kind "guid" "guid"

  type Decimal with
    static member FromString: string -> Sum<System.Decimal, Errors> =
      Decimal.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected Decimal, found non-Decimal string")

    static member ToJsonString: Decimal -> JsonValue =
      fun d ->
        JsonValue.Record
          [| "kind", JsonValue.String "decimal"
             "decimal", JsonValue.String(d.ToString(CultureInfo.InvariantCulture)) |]

  type Boolean with
    static member FromString: string -> Sum<bool, Errors> =
      Boolean.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected boolean, found non-boolean string")

    static member ToJsonString: bool -> JsonValue =
      fun b ->
        JsonValue.Record
          [| "kind", JsonValue.String "boolean"
             "boolean", JsonValue.String(b.ToString().ToLowerInvariant()) |]

  type DateOnly with
    static member FromString: string -> Sum<DateOnly, Errors> =
      DateOnly.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected DateOnly, found non-DateOnly string")

    static member ToJsonString: DateOnly -> JsonValue =
      fun d ->
        JsonValue.Record
          [| "kind", JsonValue.String "date"
             "date", JsonValue.String(d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)) |]

  type DateTime with
    static member FromString: string -> Sum<DateTime, Errors> =
      (fun date -> System.DateTime.TryParse(date, null, System.Globalization.DateTimeStyles.RoundtripKind))
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected DateTime, found non-DateTime string")

    static member ToJsonString: DateTime -> JsonValue =
      fun d ->
        JsonValue.Record
          [| "kind", JsonValue.String "datetime"
             "datetime", JsonValue.String(d.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture)) |]

  type PrimitiveValue with
    static member private FromJsonInt32 =
      sum.AssertKindAndContinueWithField
        "int32"
        "int32"
        (JsonValue.AsString >>= Int32.FromString >>= (PrimitiveValue.Int32 >> sum.Return))

    static member private ToJsonInt32: int -> JsonValue = Int32.ToJsonString

    static member private FromJsonInt64 =
      sum.AssertKindAndContinueWithField
        "int64"
        "int64"
        (JsonValue.AsString >>= Int64.FromString >>= (PrimitiveValue.Int64 >> sum.Return))

    static member private ToJsonInt64: int64 -> JsonValue = Int64.ToJsonString

    static member private FromJsonFloat32 =
      sum.AssertKindAndContinueWithField
        "float32"
        "float32"
        (JsonValue.AsString
         >>= Single.FromString
         >>= (PrimitiveValue.Float32 >> sum.Return))

    static member private ToJsonFloat32: float32 -> JsonValue = Single.ToJsonString

    static member private FromJsonFloat64 =
      sum.AssertKindAndContinueWithField
        "float64"
        "float64"
        (JsonValue.AsString
         >>= Double.FromString
         >>= (PrimitiveValue.Float64 >> sum.Return))

    static member private ToJsonFloat64: float -> JsonValue = Double.ToJsonString

    static member private FromJsonDecimal =
      sum.AssertKindAndContinueWithField
        "decimal"
        "decimal"
        (JsonValue.AsString
         >>= fun s ->
           match
             System.Decimal.TryParse(
               s,
               System.Globalization.NumberStyles.Number,
               System.Globalization.CultureInfo.InvariantCulture
             )
           with
           | true, v -> sum.Return v
           | false, _ -> sum.Throw(Errors.Singleton $"Invalid decimal: '{s}'"))
      >>= (PrimitiveValue.Decimal >> sum.Return)

    static member private FromJsonBoolean =
      fun json ->
        sum.Any2
          (sum.AssertKindAndContinueWithField
            "boolean"
            "boolean"
            (JsonValue.AsString
             >>= Boolean.FromString
             >>= (PrimitiveValue.Bool >> sum.Return))
           <| json)
          (sum.AssertKindAndContinueWithField
            "bool"
            "bool"
            (JsonValue.AsString
             >>= Boolean.FromString
             >>= (PrimitiveValue.Bool >> sum.Return))
           <| json)

    static member private FromJsonGuid =
      sum.AssertKindAndContinueWithField
        "guid"
        "guid"
        (JsonValue.AsString
         >>= System.Guid.FromString
         >>= (PrimitiveValue.Guid >> sum.Return))

    static member private FromJsonDate =
      sum.AssertKindAndContinueWithField
        "date"
        "date"
        (JsonValue.AsString
         >>= DateOnly.FromString
         >>= (PrimitiveValue.Date >> sum.Return))

    static member private FromJsonDateTime =
      sum.AssertKindAndContinueWithField
        "datetime"
        "datetime"
        (JsonValue.AsString
         >>= System.DateTime.FromString
         >>= (PrimitiveValue.DateTime >> sum.Return))

    static member private FromJsonString =
      sum.AssertKindAndContinueWithField
        "string"
        "string"
        (JsonValue.AsString >>= (PrimitiveValue.String >> sum.Return))

    static member private FromJsonUnit =
      sum.AssertKindAndContinue "unit" (fun _ -> PrimitiveValue.Unit |> sum.Return)

    static member private ToJsonString: string -> JsonValue =
      JsonValue.String >> Json.kind "string" "string"

    static member private ToJsonUnit: JsonValue =
      JsonValue.Record [| "kind", JsonValue.String "unit" |]

    static member FromJson json =
      sum.Any(
        PrimitiveValue.FromJsonInt32 json,
        [ PrimitiveValue.FromJsonInt64 json
          PrimitiveValue.FromJsonFloat32 json
          PrimitiveValue.FromJsonFloat64 json
          PrimitiveValue.FromJsonDecimal json
          PrimitiveValue.FromJsonBoolean json
          PrimitiveValue.FromJsonGuid json
          PrimitiveValue.FromJsonDate json
          PrimitiveValue.FromJsonDateTime json
          PrimitiveValue.FromJsonString json
          PrimitiveValue.FromJsonUnit json ]
      )

    static member ToJson: PrimitiveValue -> JsonValue =
      fun pt ->
        match pt with
        | PrimitiveValue.Int32 p -> Int32.ToJsonString p
        | PrimitiveValue.Int64 p -> Int64.ToJsonString p
        | PrimitiveValue.Float32 p -> Single.ToJsonString p
        | PrimitiveValue.Float64 p -> Double.ToJsonString p
        | PrimitiveValue.Decimal p -> Decimal.ToJsonString p
        | PrimitiveValue.Bool p -> Boolean.ToJsonString p
        | PrimitiveValue.Guid p -> System.Guid.ToJsonString p
        | PrimitiveValue.Date p -> DateOnly.ToJsonString p
        | PrimitiveValue.DateTime p -> DateTime.ToJsonString p
        | PrimitiveValue.String p -> PrimitiveValue.ToJsonString p
        | PrimitiveValue.Unit -> PrimitiveValue.ToJsonUnit
