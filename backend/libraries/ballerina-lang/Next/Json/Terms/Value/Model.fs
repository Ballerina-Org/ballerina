namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Value =
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open FSharp.Data
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Json

  type Value<'T, 'valueExtension> with
    static member FromJson
      (fromJsonRoot: ValueParser<'T, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, 'valueExtension> =
      reader.Any(
        Value.FromJsonPrimitive json,
        [ Value.FromJsonRecord fromJsonRoot json
          Value.FromJsonUnion fromJsonRoot json
          Value.FromJsonTuple fromJsonRoot json
          Value.FromJsonSum fromJsonRoot json
          Value.FromJsonVar json
          Value.FromJsonLambda json
          Value.FromJsonTypeLambda json
          $"Unknown Value JSON: {json.ToFSharpString.ReasonablyClamped}"
          |> Errors.Singleton
          |> Errors.WithPriority ErrorPriority.Medium
          |> reader.Throw ]
      )
      |> reader.MapError(Errors.HighestPriority)

    static member ToJson
      (toJsonRoot: ValueEncoder<'T, 'valueExtension>)
      (value: Value<'T, 'valueExtension>)
      : ValueEncoderReader<'T> =
      match value with
      | Value.Primitive p -> Value.ToJsonPrimitive p
      | Value.Record m -> Value.ToJsonRecord toJsonRoot m
      | Value.UnionCase(s, v) -> Value.ToJsonUnion toJsonRoot s v
      | Value.Tuple vs -> Value.ToJsonTuple toJsonRoot vs
      | Value.Sum(i, v) -> Value.ToJsonSum toJsonRoot i v
      | Value.Var v -> Value.ToJsonVar v |> reader.Return
      | Value.Lambda(a, b) -> Value.ToJsonLambda a b
      | Value.TypeLambda(a, b) -> Value.ToJsonTypeLambda a b
      | Value.Ext e -> reader.Throw(Errors.Singleton $"Extension parsing not yet implemented: {e}")
