namespace Ballerina.DSL.Next.Terms.Json

open System.Globalization
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types

module Primitive =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina
  open Ballerina.Collections.Sum.Operators
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Json
  open Ballerina.StdLib.Formats
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json.Keys

  type Int32 with
    static member FromString: string -> Sum<int, Errors<_>> =
      Int32.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton () (fun () -> "Error: expected int, found non-integer string"))

    static member ToJsonString: int -> JsonValue =
      fun i ->
        JsonValue.Record
          [| discriminatorKey, JsonValue.String "int32"
             valueKey, JsonValue.String(i.ToString(CultureInfo.InvariantCulture)) |]

  type Int64 with
    static member FromString: string -> Sum<int64, Errors<_>> =
      Int64.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton () (fun () -> "Error: expected int64, found non-integer string"))

    static member ToJsonString: int64 -> JsonValue =
      fun i ->
        JsonValue.Record
          [| discriminatorKey, JsonValue.String "int64"
             valueKey, JsonValue.String(i.ToString(CultureInfo.InvariantCulture)) |]

  type Single with
    static member FromString: string -> Sum<float32, Errors<_>> =
      Single.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton () (fun () -> "Error: expected float32, found non-float32 string"))

    static member ToJsonString: float32 -> JsonValue =
      fun f ->
        JsonValue.Record
          [| discriminatorKey, JsonValue.String "float32"
             valueKey, JsonValue.String(f.ToString(CultureInfo.InvariantCulture)) |]

  type Double with
    static member FromString: string -> Sum<float, Errors<_>> =
      Double.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton () (fun () -> "Error: expected float64, found non-float64 string"))

    static member ToJsonString: float -> JsonValue =
      fun f ->
        JsonValue.Record
          [| discriminatorKey, JsonValue.String "float64"
             valueKey, JsonValue.String(f.ToString(CultureInfo.InvariantCulture)) |]

  type Guid with
    static member FromString: string -> Sum<System.Guid, Errors<_>> =
      Guid.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton () (fun () -> "Error: expected Guid, found non-Guid string"))

    static member ToJsonString: System.Guid -> JsonValue =
      _.ToString("D", System.Globalization.CultureInfo.InvariantCulture)
      >> JsonValue.String
      >> Json.discriminator "guid"

  type Decimal with
    static member FromString: string -> Sum<System.Decimal, Errors<_>> =
      Decimal.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton () (fun () -> "Error: expected Decimal, found non-Decimal string"))

    static member ToJsonString: Decimal -> JsonValue =
      fun d ->
        JsonValue.Record
          [| discriminatorKey, JsonValue.String "decimal"
             valueKey, JsonValue.String(d.ToString(CultureInfo.InvariantCulture)) |]

  type Boolean with
    static member FromString: string -> Sum<bool, Errors<_>> =
      Boolean.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton () (fun () -> "Error: expected boolean, found non-boolean string"))

    static member ToJsonString: bool -> JsonValue =
      fun b ->
        JsonValue.Record
          [| discriminatorKey, JsonValue.String "boolean"
             valueKey, JsonValue.String(b.ToString().ToLowerInvariant()) |]

  type DateOnly with
    static member FromString: string -> Sum<DateOnly, Errors<_>> =
      Iso8601.DateOnly.tryParse
      >> function
        | Some value -> sum.Return value
        | None -> sum.Throw(Errors.Singleton () (fun () -> "Error: expected DateOnly, found non-DateOnly string"))

    static member ToJsonString date =
      JsonValue.Record
        [| discriminatorKey, JsonValue.String "date"
           valueKey, date |> Iso8601.DateOnly.print |> JsonValue.String |]

  type DateTime with
    static member FromString: string -> Sum<DateTime, Errors<_>> =
      Iso8601.DateTime.tryParse
      >> function
        | Some value -> sum.Return value
        | None -> sum.Throw(Errors.Singleton () (fun () -> "Error: expected DateTime, found non-DateTime string"))

    static member ToJsonString: DateTime -> JsonValue =
      fun date ->
        JsonValue.Record
          [| discriminatorKey, JsonValue.String "datetime"
             valueKey, date |> Iso8601.DateTime.printUtc |> JsonValue.String |]

  type TimeSpan with
    static member FromString: string -> Sum<TimeSpan, Errors<_>> =
      TimeSpan.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton () (fun () -> "Error: expected TimeSpan, found non-TimeSpan string"))

    static member ToJsonString: TimeSpan -> JsonValue =
      fun t ->
        JsonValue.Record
          [| discriminatorKey, JsonValue.String "timespan"
             valueKey, JsonValue.String(t.ToString()) |]

  type PrimitiveValue with
    static member private FromJsonInt32 =
      Sum.assertDiscriminatorAndContinueWithValue
        "int32"
        (JsonValue.AsString >>= Int32.FromString >>= (PrimitiveValue.Int32 >> sum.Return))

    static member private ToJsonInt32: int -> JsonValue = Int32.ToJsonString

    static member private FromJsonInt64 =
      Sum.assertDiscriminatorAndContinueWithValue
        "int64"
        (JsonValue.AsString >>= Int64.FromString >>= (PrimitiveValue.Int64 >> sum.Return))

    static member private ToJsonInt64: int64 -> JsonValue = Int64.ToJsonString

    static member private FromJsonFloat32 =
      Sum.assertDiscriminatorAndContinueWithValue
        "float32"
        (JsonValue.AsString
         >>= Single.FromString
         >>= (PrimitiveValue.Float32 >> sum.Return))

    static member private FromJsonFloat64 =
      Sum.assertDiscriminatorAndContinueWithValue
        "float64"
        (JsonValue.AsString
         >>= Double.FromString
         >>= (PrimitiveValue.Float64 >> sum.Return))

    static member private FromJsonDecimal =
      Sum.assertDiscriminatorAndContinueWithValue "decimal" (fun valueJson ->
        sum {
          let! s = valueJson |> JsonValue.AsString

          let! v =
            match
              System.Decimal.TryParse(
                s,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture
              )
            with
            | true, v -> sum.Return v
            | false, _ -> sum.Throw(Errors.Singleton () (fun () -> $"Invalid decimal: '{s}'"))

          return PrimitiveValue.Decimal v
        })

    static member private FromJsonBoolean(json: JsonValue) =
      sum.Any2
        (Sum.assertDiscriminatorAndContinueWithValue
          "boolean"
          (JsonValue.AsString
           >>= Boolean.FromString
           >>= (PrimitiveValue.Bool >> sum.Return))
          json)
        (Sum.assertDiscriminatorAndContinueWithValue
          "bool"
          (JsonValue.AsString
           >>= Boolean.FromString
           >>= (PrimitiveValue.Bool >> sum.Return))
          json)

    static member private FromJsonGuid(json: JsonValue) =
      Sum.assertDiscriminatorAndContinueWithValue
        "guid"
        (JsonValue.AsString
         >>= System.Guid.FromString
         >>= (PrimitiveValue.Guid >> sum.Return))
        json

    static member private FromJsonDate(json: JsonValue) =
      Sum.assertDiscriminatorAndContinueWithValue
        "date"
        (JsonValue.AsString
         >>= DateOnly.FromString
         >>= (PrimitiveValue.Date >> sum.Return))
        json

    static member private FromJsonDateTime(json: JsonValue) =
      Sum.assertDiscriminatorAndContinueWithValue
        "datetime"
        (JsonValue.AsString
         >>= DateTime.FromString
         >>= (PrimitiveValue.DateTime >> sum.Return))
        json

    static member private FromJsonString(json: JsonValue) =
      Sum.assertDiscriminatorAndContinueWithValue
        "string"
        (JsonValue.AsString >>= (PrimitiveValue.String >> sum.Return))
        json

    static member private FromJsonTimeSpan(json: JsonValue) =
      Sum.assertDiscriminatorAndContinueWithValue
        "timespan"
        (JsonValue.AsString
         >>= TimeSpan.FromString
         >>= (PrimitiveValue.TimeSpan >> sum.Return))
        json

    static member private FromJsonUnit(json: JsonValue) =
      sum.AssertDiscriminatorAndContinue discriminatorKey "unit" (fun _ -> PrimitiveValue.Unit |> sum.Return) json

    static member private ToJsonString: string -> JsonValue =
      JsonValue.String >> Json.discriminator "string"

    static member private ToJsonUnit: JsonValue =
      JsonValue.Record [| discriminatorKey, JsonValue.String "unit" |]

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
          PrimitiveValue.FromJsonTimeSpan json
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
        | PrimitiveValue.TimeSpan p -> TimeSpan.ToJsonString p
        | PrimitiveValue.String p -> PrimitiveValue.ToJsonString p
        | PrimitiveValue.Unit -> PrimitiveValue.ToJsonUnit
