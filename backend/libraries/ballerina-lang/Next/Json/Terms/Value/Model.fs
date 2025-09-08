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
      (fromJsonExt: JsonParser<'valueExtension>)
      (json: JsonValue)
      : ValueParser<'T, 'valueExtension> =
      reader.Any(
        Value.FromJsonPrimitive json,
        [ Value.FromJsonRecord (Value.FromJson fromJsonExt) json
          Value.FromJsonUnion (Value.FromJson fromJsonExt) json
          Value.FromJsonTuple (Value.FromJson fromJsonExt) json
          Value.FromJsonSum (Value.FromJson fromJsonExt) json
          Value.FromJsonVar json
          Value.FromJsonLambda Expr.FromJson json
          Value.FromJsonTypeLambda Expr.FromJson json
          $"Unknown Value JSON: {json.ToFSharpString.ReasonablyClamped}"
          |> Errors.Singleton
          |> Errors.WithPriority ErrorPriority.Medium
          |> reader.Throw ]
      )
      |> reader.MapError(Errors.HighestPriority)

    static member ToJson: ('valueExtension -> JsonValue) -> Value<'T, 'valueExtension> -> JsonValue =
      fun toJsonExt value ->
        match value with
        | Value.Primitive p -> Value.ToJsonPrimitive p
        | Value.Record m -> Value.ToJsonRecord (Value.ToJson toJsonExt) m
        | Value.UnionCase(s, v) -> Value.ToJsonUnion (Value.ToJson toJsonExt) (s, v)
        | Value.Tuple vs -> Value.ToJsonTuple (Value.ToJson toJsonExt) vs
        | Value.Sum(i, v) -> Value.ToJsonSum (Value.ToJson toJsonExt) (i, v)
        | Value.Var v -> Value.ToJsonVar v
        | Value.Lambda(a, b) -> Value.ToJsonLambda Expr.ToJson (a, b)
        | Value.TypeLambda(a, b) -> Value.ToJsonTypeLambda Expr.ToJson (a, b)
        | Value.Ext e -> toJsonExt e
