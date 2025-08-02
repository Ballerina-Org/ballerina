namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module TypeLambda =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json

  type Value<'T> with
    static member FromJsonTypeLambda(_fromJsonRoot: JsonValue -> ExprParser<'T>) : JsonValue -> ValueParser<'T> =
      fun json ->
        reader {
          return!
            reader.AssertKindAndContinueWithField
              "type-lambda"
              "type-lambda"
              (fun typeParamJson ->
                reader {
                  let! (typeParam, body) = typeParamJson |> JsonValue.AsPair |> reader.OfSum
                  let! typeParam = typeParam |> TypeParameter.FromJson |> reader.OfSum
                  let! body = body |> Expr.FromJson
                  return Value.TypeLambda(typeParam, body)
                })
              (json)
        }

    static member ToJsonTypeLambda(toJsonRoot: Expr<'T> -> JsonValue) : TypeParameter * Expr<'T> -> JsonValue =
      fun (tp, body) ->
        let tp = TypeParameter.ToJson tp
        let bodyJson = toJsonRoot body
        [| tp; bodyJson |] |> JsonValue.Array |> Json.kind "type-lambda" "type-lambda"
