namespace Ballerina.DSL.Next.Terms.Json

module Primitive =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Object
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.StdLib.Json.Sum
  open Ballerina.StdLib.Json.Reader

  let inline private (>>=) f g = fun x -> sum.Bind(f x, g) // Using bind

  type System.Int32 with
    static member FromString: string -> Sum<int, Errors> =
      System.Int32.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected int, found non-integer string")

  type System.Guid with
    static member FromString: string -> Sum<System.Guid, Errors> =
      System.Guid.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected Guid, found non-Guid string")

  type System.Decimal with
    static member FromString: string -> Sum<System.Decimal, Errors> =
      System.Decimal.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected Decimal, found non-Decimal string")

  type System.Boolean with
    static member FromString: string -> Sum<bool, Errors> =
      System.Boolean.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected boolean, found non-boolean string")

  type System.DateOnly with
    static member FromString: string -> Sum<DateOnly, Errors> =
      System.DateOnly.TryParse
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected DateOnly, found non-DateOnly string")

  type System.DateTime with
    static member FromString: string -> Sum<DateTime, Errors> =
      (fun date -> System.DateTime.TryParse(date, null, System.Globalization.DateTimeStyles.RoundtripKind))
      >> function
        | true, value -> sum.Return value
        | false, _ -> sum.Throw(Errors.Singleton "Error: expected DateTime, found non-DateTime string")

  type PrimitiveValue with
    static member private FromJsonInt =
      sum.AssertKindAndContinueWithField
        "int"
        "int"
        (JsonValue.AsString
         >>= System.Int32.FromString
         >>= (PrimitiveValue.Int >> sum.Return))

    static member private FromJsonDecimal =
      sum.AssertKindAndContinueWithField
        "decimal"
        "decimal"
        (JsonValue.AsString
         >>= System.Decimal.FromString
         >>= (PrimitiveValue.Decimal >> sum.Return))

    static member private FromJsonBoolean =
      sum.AssertKindAndContinueWithField
        "boolean"
        "boolean"
        (JsonValue.AsString
         >>= System.Boolean.FromString
         >>= (PrimitiveValue.Bool >> sum.Return))

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
         >>= System.DateOnly.FromString
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

    static member FromJson json =
      sum.Any(
        PrimitiveValue.FromJsonInt json,
        [ PrimitiveValue.FromJsonDecimal json
          PrimitiveValue.FromJsonBoolean json
          PrimitiveValue.FromJsonGuid json
          PrimitiveValue.FromJsonDate json
          PrimitiveValue.FromJsonDateTime json
          PrimitiveValue.FromJsonString json
          PrimitiveValue.FromJsonUnit json ]
      )
